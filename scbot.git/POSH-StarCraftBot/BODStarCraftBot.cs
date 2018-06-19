using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SWIG.BWAPI;
using BWAPI;
using POSH.sys.strict;
using SWIG.BWTA;
using log4net;
using POSH_StarCraftBot.logic;

namespace POSH_StarCraftBot
{
    public enum BuildSite { None = 0, StartingLocation = 1, Natural = 2, Extension = 3, Choke = 4, NaturalChoke = 5, EnemyChoke = 6};
    public enum ForceLocations { NotAssigned = 0, OwnStart = 1, Natural = 2, Extension = 3, NaturalChoke = 4, EnemyNatural = 5, EnemyStart = 6, ArmyOne = 7, ArmyTwo = 8, Build = 9, Scout = 10, EnemyChoke = 11};
    public enum GamePhase { Early, Mid, End }

    public class BODStarCraftBot : IStarcraftBot
    {

        protected internal Dictionary<string, Player> ActivePlayers { get; private set; }
        protected internal Dictionary<long, Unit> UnitDiscovered { get; private set; }
        protected internal Dictionary<long, Unit> UnitEvade { get; private set; }

        protected internal Dictionary<long, Unit> UnitShow { get; private set; }
        protected internal Dictionary<long, Unit> UnitHide { get; private set; }

        protected internal Dictionary<long, Unit> UnitCreated { get; private set; }
        protected internal Dictionary<long, Unit> UnitDestroyed { get; private set; }
        protected internal Dictionary<long, Unit> UnitMorphed { get; private set; }
        protected internal Dictionary<long, Unit> UnitTrained { get; private set; }
        protected internal Dictionary<long, Unit> UnitRenegade { get; private set; }

        private int[] mapDim;
        protected log4net.ILog LOG;

        protected internal POSH_StarCraftBot.behaviours.AStarCraftBehaviour.Races enemyRace { get; set; }
		
		/// <summary>
        /// Contains upto 7 forces of different size. The forces are at forcePoints identified by the same force location key.
        /// Forces ArmyOne and ArmyTwo are mobile and have no fixed TilePosition. 
        /// </summary>
        protected internal Dictionary<ForceLocations, List<UnitAgent>> forces;

        /// <summary>
        /// The forcePoints identify upto 8 different locations based on the ForceLocation associated with a forcePoint. 
        /// Forces ArmyOne and ArmyTwo are moving forces whereas the others are static at specific locations.
        /// </summary>
        public Dictionary<ForceLocations, TilePosition> forcePoints;
        protected internal ForceLocations currentForcePoint;

        /// <summary>
        /// The base locations used within the game. "0" is starting location, "1" is natural, "2" is first Extension
        /// </summary>
        public Dictionary<int, TilePosition> baseLocations { get; set; }



        /// <summary>
        /// The base we want to build at. "O" means starting base, "1" is natural "2" is first Extension "-1" is error state.
        /// </summary>
        public BuildSite currentBuildSite;
        public Chokepoint chokepoint;
        public TilePosition buildingChoke;
		public TilePosition enemyBuildingChoke;
		public TilePosition naturalBuild;
		public bool enemyBaseFound = false;
		public bool naturalHasBeenFound = false;
		public TilePosition enemyBaseLocation = null;
		protected internal IOrderedEnumerable<BaseLocation> basePositions;
		
        private int[] mineralPatchIDs = new int[3] { bwapi.UnitTypes_Resource_Mineral_Field.getID(), 
                bwapi.UnitTypes_Resource_Mineral_Field_Type_2.getID(), 
                bwapi.UnitTypes_Resource_Mineral_Field_Type_3.getID() };
        public BODStarCraftBot(ILog log)
        {
            this.LOG = log;
        }

        void IStarcraftBot.onStart()
        {
            bwapi.Broodwar.setLocalSpeed(0);
            bwapi.Broodwar.setFrameSkip(200);
            System.Console.WriteLine("Starting Match!");
            bwapi.Broodwar.sendText("Hello world! This is POSH!");
            mapDim = new int[2];
            ActivePlayers = new Dictionary<string, Player>();
            UnitDiscovered = new Dictionary<long, Unit>();
            UnitEvade = new Dictionary<long, Unit>();
            UnitShow = new Dictionary<long, Unit>();
            UnitHide = new Dictionary<long, Unit>();

            UnitCreated = new Dictionary<long, Unit>();
            UnitDestroyed = new Dictionary<long, Unit>();
            UnitMorphed = new Dictionary<long, Unit>();
            UnitTrained = new Dictionary<long, Unit>();
            UnitRenegade = new Dictionary<long, Unit>();

            baseLocations = new Dictionary<int, TilePosition>();
            currentBuildSite = BuildSite.StartingLocation;

            foreach (Player pl in bwapi.Broodwar.getPlayers())
                ActivePlayers.Add(pl.getName(), pl);
            if (ActivePlayers.ContainsKey(Self().getName()))
                ActivePlayers.Remove(Self().getName());

            forces = new Dictionary<ForceLocations, List<UnitAgent>>();
            forcePoints = new Dictionary<ForceLocations, TilePosition>();

            // initiating the starting location
            if (Self().getStartLocation() is TilePosition)
            {
                baseLocations[(int)BuildSite.StartingLocation] = Self().getStartLocation();
                forcePoints[ForceLocations.OwnStart] = Self().getStartLocation();

            }

            // Get all base locations to be used for scouting and attacking
			basePositions = bwta.getBaseLocations().Where(baseLoc => bwta.getGroundDistance(Self().getStartLocation(), baseLoc.getTilePosition()) > 0).OrderBy(baseLoc => bwta.getGroundDistance(Self().getStartLocation(), baseLoc.getTilePosition()));
			basePositions.Reverse();
			
            currentForcePoint = ForceLocations.OwnStart;
            currentBuildSite = BuildSite.StartingLocation;
        }

        //
        // own Player
        //
        public Player Self()
        {
            return bwapi.Broodwar.self();
        }

        public bool GameRunning()
        {
			return bwapi.Broodwar.isInGame();  
        }

        public TilePosition StartLocation()
        {
            return baseLocations[1];
        }

        public int GetMapHeight()
        {
            if (mapDim[0] < 1)
                mapDim[0] = bwapi.Broodwar.mapHeight();

            return mapDim[0];
        }

        public int GetMapWidht()
        {
            if (mapDim[1] < 1)
                mapDim[1] = bwapi.Broodwar.mapHeight();

            return mapDim[1];
        }

        //
        //
        // Resources
        //
        //

        public int MineralCount()
        {
            return bwapi.Broodwar.self().minerals();
        }

        public int GasCount()
        {
            return bwapi.Broodwar.self().gas();
        }

        public int AvailableSupply()
        {
            return (bwapi.Broodwar.self().supplyTotal() - bwapi.Broodwar.self().supplyUsed()) / 2;
        }

        public int SupplyCount()
        {
            return (bwapi.Broodwar.self().supplyUsed()) / 2;
        }

        /// <summary>
        /// Supply is devided by 2 because BWAPI doubled the amount to have a mininimum of 1 supply for Zerglings which is normally 0.5
        /// </summary>
        /// <returns></returns>
        public int TotalSupply()
        {
            return bwapi.Broodwar.self().supplyTotal() / 2;
        }

		public IEnumerable<Unit> GetAllUnits(bool worker)
        {
            return bwapi.Broodwar.self().getUnits().Where(unit =>
                !unit.getType().isBuilding() &&
                (worker) ? unit.getType().isWorker() : !unit.getType().isWorker()
                );
        }

        public IEnumerable<Unit> GetAllBuildings()
        {
            return bwapi.Broodwar.self().getUnits().Where(unit =>
                unit.getType().isBuilding()
                );
        }

        public IEnumerable<Unit> GetGeysers()
        {
            return bwapi.Broodwar.getGeysers().Where(patch => patch.getResources() > 0);
        }

        public IEnumerable<Unit> GetMineralPatches()
        {
            return bwapi.Broodwar.getMinerals().Where(patch => patch.getResources() > 0);
        }

        //Function to return an available builder
        protected internal Unit GetBuilder(TilePosition location, bool selectNew)
        {
            if (!forces.ContainsKey(ForceLocations.Build) || !(forces[ForceLocations.Build] is List<UnitAgent>))
            {
                forces[ForceLocations.Build] = new List<UnitAgent>();
            }
            else
            {
				forces[ForceLocations.Build].Clear();
            }
            //if (forces[ForceLocations.Build].Count > 0)
              //  return forces[ForceLocations.Build].OrderBy(unit => unit.SCUnit.getDistance(new Position(location))).First().SCUnit;

			Unit builder = GetProbes().Where(probe => probe.getHitPoints() > 0).OrderBy(probe => probe.getDistance(new Position(location))).First();

			if (builder == null || selectNew)
			{
				Unit altBuilder = GetProbes().OrderBy(probe => probe.getDistance(new Position(location))).Where(probe => probe.getHitPoints() > 0).Last();
				forces[ForceLocations.Build].Add(new UnitAgent(altBuilder, null, null));
				return altBuilder;
			}
			forces[ForceLocations.Build].Add(new UnitAgent(builder, null, null));
            return builder;
        }

        //
        //
        // Protoss Units
        //
        //

        //Return the number of Probes under the AI's control
        public int ProbeCount()
        {
			if (bwapi.Broodwar.self().allUnitCount(bwapi.UnitTypes_Protoss_Probe) <= 0)
				return 0;
			else
				return bwapi.Broodwar.self().allUnitCount(bwapi.UnitTypes_Protoss_Probe);
        }


        //Return the number of Zealots under the AI's control
        public int ZealotCount()
        {
			if (bwapi.Broodwar.self().allUnitCount(bwapi.UnitTypes_Protoss_Zealot) <= 0)
				return 0;
			else
				return bwapi.Broodwar.self().allUnitCount(bwapi.UnitTypes_Protoss_Zealot);
        }


        //Return the number of Dragoons under the AI's control
        public int DragoonCount()
        {
			if (bwapi.Broodwar.self().allUnitCount(bwapi.UnitTypes_Protoss_Dragoon) <= 0)
				return 0;
			else
				return bwapi.Broodwar.self().allUnitCount(bwapi.UnitTypes_Protoss_Dragoon);
        }

		//Return the number of Dark Templar under the AI's control
		public int DarkTemplarCount()
		{
			if (bwapi.Broodwar.self().allUnitCount(bwapi.UnitTypes_Protoss_Dark_Templar) <= 0)
				return 0;
			else
				return bwapi.Broodwar.self().allUnitCount(bwapi.UnitTypes_Protoss_Dark_Templar);
		}


        //Return the number of Corsairs under the AI's control
        public int CorsairCount()
        {
			if (bwapi.Broodwar.self().allUnitCount(bwapi.UnitTypes_Protoss_Corsair) <= 0)
				return 0;
			else
				return bwapi.Broodwar.self().allUnitCount(bwapi.UnitTypes_Protoss_Corsair);
        }


        //Return the number of Observers under the AI's control
        public int ObserverCount()
        {
			if (bwapi.Broodwar.self().allUnitCount(bwapi.UnitTypes_Protoss_Observer) <= 0)
				return 0;
			else
				return bwapi.Broodwar.self().allUnitCount(bwapi.UnitTypes_Protoss_Observer);
        }

		//Return the number of Observers under the AI's control
		public int CarrierCount()
		{
			if (bwapi.Broodwar.self().allUnitCount(bwapi.UnitTypes_Protoss_Carrier) <= 0)
				return 0;
			else
				return bwapi.Broodwar.self().allUnitCount(bwapi.UnitTypes_Protoss_Carrier);
		}

        //Return the number of Probes under the AI's control Plus their location
        public IEnumerable<Unit> GetProbes(int amount)
        {
            return GetProbes().Take(amount);
        }


        //Return the Location of all Probes under the AI's control
        public IEnumerable<Unit> GetProbes()
        {
            return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == bwapi.UnitTypes_Protoss_Probe.getID());
        }


        //Return any Idle Probes under the AI's control
        public IEnumerable<Unit> GetIdleProbes()
        {
            return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == bwapi.UnitTypes_Protoss_Probe.getID()).Where(probe => probe.isIdle() && !IsBuilder(probe) );
        }


        //Test to see if a Probe can build
        protected internal bool IsBuilder(Unit probe)
        {
            if (!forces.ContainsKey(ForceLocations.Build) || !(forces[ForceLocations.Build] is List<UnitAgent>))
                return false;

            forces[ForceLocations.Build].RemoveAll(unit => unit.SCUnit.getBuildType().isBuilding() || unit.SCUnit.getHitPoints() <= 0);
            return (forces[ForceLocations.Build].Where(unit => unit.SCUnit.getID() == probe.getID()).Count() > 0) ? true : false;

        }


        //Return the number of Zealots under the AI's control Plus their location
        public IEnumerable<Unit> GetZealot(int amount)
        {
            return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == bwapi.UnitTypes_Protoss_Zealot.getID()).Take(amount);
        }


        //Return the number of Dragoons under the AI's control Plus their location
        public IEnumerable<Unit> GetDragoon(int amount)
        {
            return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == bwapi.UnitTypes_Protoss_Dragoon.getID()).Take(amount);
        }


        //Return the number of Corsairs under the AI's control Plus their location
        public IEnumerable<Unit> GetCorsair(int amount)
        {
            return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == bwapi.UnitTypes_Protoss_Corsair.getID()).Take(amount);
        }

		//Return the number of Carriers under the AI's control Plus their location
		public IEnumerable<Unit> GetCarrier()
		{
			return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == bwapi.UnitTypes_Protoss_Carrier.getID());
		}

        //Return the number of Observers under the AI's control Plus their location
        public IEnumerable<Unit> GetObserver(int amount)
        {
            return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == bwapi.UnitTypes_Protoss_Observer.getID()).Take(amount);
        }

        //
        //
        // Protoss Buildings
        //
        //

        //Return the number of Nexus' under the AI's control Plus their location
        public IEnumerable<Unit> GetNexus()
        {
            return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == bwapi.UnitTypes_Protoss_Nexus.getID());
        }

        //Return the number of Forges under the AI's control Plus their location
        public IEnumerable<Unit> GetForge()
        {
            return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == bwapi.UnitTypes_Protoss_Forge.getID());
        }
		
        //Return the number of Cybernetics Cores under the AI's control Plus their location
        public IEnumerable<Unit> GetCyberneticsCore()
        {
            return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == bwapi.UnitTypes_Protoss_Cybernetics_Core.getID());
        }

        //Return the number of Observatories under the AI's control Plus their location
		public IEnumerable<Unit> GetObservatory()
		{
			return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == bwapi.UnitTypes_Protoss_Observatory.getID());
		}

        //Return the number of Fleet Beacons under the AI's control Plus their location
        public IEnumerable<Unit> GetFleetbeacon()
        {
            return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == bwapi.UnitTypes_Protoss_Fleet_Beacon.getID());
        }

		//Return the number of Citadel of Adun under the AI's control Plus their location
		public IEnumerable<Unit> GetCitadel()
		{
			return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == bwapi.UnitTypes_Protoss_Citadel_of_Adun.getID());
		}

		//Return the number of Templar Archives under the AI's control Plus their location
		public IEnumerable<Unit> GetTemplarArchives()
		{
			return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == bwapi.UnitTypes_Protoss_Templar_Archives.getID());
		}

		//Return the building ID
		public IEnumerable<Unit> GetBuilding(UnitType Building)
		{
			return bwapi.Broodwar.self().getUnits().Where(unit => unit.getType().getID() == Building.getID());
		}
        
        ////////////////////////////////////////////////////////////////////////End of James' Code////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Clears the internal dictionaries which keep track of the incomming information from the game.
        /// </summary>
        /// <param name="history">
        ///     Specifies length of time in milliseconds from the current time until the last event to remember.
        ///     Everything older that that is removed from the memory.
        /// </param>
        public void ReleaseOldInfo(long history = 60000L)
        {
            long leaseTime = Core.Timer.Time() - history;
            if (leaseTime < 0L)
                leaseTime = 0L;

            emptyDictionaryBeforeTimeStamp(10000L, new Dictionary<long, Unit>[] { UnitDestroyed, UnitEvade, UnitShow, UnitHide, UnitCreated, UnitDestroyed, UnitMorphed, UnitRenegade });
        }

        private void emptyDictionaryBeforeTimeStamp(long timestamp, Dictionary<long, Unit>[] memories)
        {
            foreach (Dictionary<long, Unit> memory in memories)
                emptyDictionaryBeforeTimeStamp(timestamp, memory);
        }
        private void emptyDictionaryBeforeTimeStamp(long timestamp, Dictionary<long, Unit> memory)
        {
            foreach (long evnt in memory.Keys)
                if (evnt <= timestamp)
                    memory.Remove(evnt);
        }

        void IStarcraftBot.onEnd(bool isWinner)
        {
            //throw new NotImplementedException();
			Environment.Exit(-1);
        }

        void IStarcraftBot.onFrame()
        {
            //UnitPtrSet set =  bwapi.Broodwar.getMinerals();
        }

        void IStarcraftBot.onSendText(string text)
        {
            //throw new NotImplementedException();
        }

        void IStarcraftBot.onReceiveText(SWIG.BWAPI.Player player, string text)
        {
            //throw new NotImplementedException();
        }

        void IStarcraftBot.onPlayerLeft(SWIG.BWAPI.Player player)
        {
            if (ActivePlayers.ContainsKey(player.getName()))
                ActivePlayers.Remove(player.getName());
        }

        void IStarcraftBot.onNukeDetect(SWIG.BWAPI.Position target)
        {
            //throw new NotImplementedException();
        }

        void IStarcraftBot.onUnitDiscover(SWIG.BWAPI.Unit unit)
        {
            UnitDiscovered[Core.Timer.Time()] = unit;
        }

        void IStarcraftBot.onUnitEvade(SWIG.BWAPI.Unit unit)
        {
            UnitEvade[Core.Timer.Time()] = unit;
        }

        void IStarcraftBot.onUnitShow(SWIG.BWAPI.Unit unit)
        {
            UnitShow[Core.Timer.Time()] = unit;
        }

        void IStarcraftBot.onUnitHide(SWIG.BWAPI.Unit unit)
        {
            UnitHide[Core.Timer.Time()] = unit;
        }

        void IStarcraftBot.onUnitCreate(SWIG.BWAPI.Unit unit)
        {
            UnitCreated[Core.Timer.Time()] = unit;
        }

        void IStarcraftBot.onUnitDestroy(SWIG.BWAPI.Unit unit)
        {
            UnitDestroyed[Core.Timer.Time()] = unit;
        }

        void IStarcraftBot.onUnitMorph(SWIG.BWAPI.Unit unit)
        {
            UnitMorphed[Core.Timer.Time()] = unit;
        }

        void IStarcraftBot.onUnitRenegade(SWIG.BWAPI.Unit unit)
        {
            UnitRenegade[Core.Timer.Time()] = unit;
        }

        void IStarcraftBot.onSaveGame(string gameName)
        {
            //throw new NotImplementedException();
        }
    }
}

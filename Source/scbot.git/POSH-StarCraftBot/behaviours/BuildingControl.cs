using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using POSH.sys.annotations;
using SWIG.BWAPI;
using SWIG.BWTA;
using POSH_StarCraftBot.logic;

namespace POSH_StarCraftBot.behaviours
{
    public class BuildingControl : AStarCraftBehaviour
    {

        TilePosition buildLocation;
        Dictionary<int, Unit> destroyedBuildings;
        Unit buildingToRepair;
        Unit repairDrone;
        Unit builder;
		private bool needNewBuilder = false;
		private bool needBuilding = true;
		private int iterations = 150;
		private int textCounter = 0;

        /// <summary>
        /// contains the current location and build queue 
        /// </summary>
        Dictionary<int, List<TilePosition>> buildQueue;

        /// <summary>
        /// contains the buildings which are currently built and still in progress. Once a building is complete it 
        /// gets removed from both dictionaries buildQueue and currentlyBuilt.
        /// </summary>
        Dictionary<Unit, TilePosition> buildingInProgress;

        public BuildingControl(AgentBase agent)
            : base(agent,
            new string[] { },
            new string[] { })
        {
            buildQueue = new Dictionary<int, List<TilePosition>>();
            buildingInProgress = new Dictionary<Unit, TilePosition>();
            destroyedBuildings = new Dictionary<int, Unit>();
        }


        //
        // INTERNAL
        //
		//Count the building input into this function
		private int countBuilding(UnitType building)
		{
			try
			{
				return Interface().GetBuilding(building).Count() + CountBuildingsinProgress(building) + CountUnbuiltBuildings(building);
			}
			catch
			{
				return 0;
			}
		}

        private TilePosition GetBaseLocation()
        {
            TilePosition baseLoc = Interface().baseLocations.ContainsKey((int)Interface().currentBuildSite) ? Interface().baseLocations[(int)Interface().currentBuildSite] : null;
            if (!(baseLoc is TilePosition))
                baseLoc = Interface().baseLocations[(int)BuildSite.StartingLocation];
            

            return baseLoc;
        }

        //////////////////////////////////////////////This function came with the source code of the base project//////////////////////////////////////////////
		private TilePosition addToTile(TilePosition pos, int x, int y)
        {
			TilePosition output = new TilePosition(pos.xConst() + x, pos.yConst() + y);

			return output;
        }

        // Returns a build location, there is a small chance that this will fail
        private TilePosition PossibleBuildLocation(TilePosition start, Unit builder, UnitType building)
        {
            // This function uses the Fibonacci spiral for optimisation purposes
			int x = 0;
			int y = 0;
			int dx = 0;
			int dy = -1;
			int t = (int)Math.Sqrt(iterations);
            int i = 0;
            while( i < iterations)
            {
                if ((-Math.Sqrt(iterations) / 2 < x && x <= Math.Sqrt(iterations) / 2) && (-Math.Sqrt(iterations) / 2 < y && y <= Math.Sqrt(iterations) / 2))
                {
                    if (bwapi.Broodwar.canBuildHere(builder, addToTile(start, x, y), building))
                    {
                        Console.Out.WriteLine("building " + building.getName() + " at:" + addToTile(start, x, y).xConst() + ":" + addToTile(start, x, y).yConst());
                        return addToTile(start, x, y);
                    }
                }
                if (x == y || (x < 0 && x == -y) || (x > 0 && x == 1 - y))
                {
                    t = dx;
                    dx = -dy;
                    dy = t;
                }
                x += dx;
                y += dy;
                i++;
            }

			return null;
        }
        //////////////////////////////////////////////This function came with the source code of the base project//////////////////////////////////////////////

        protected int CountUnbuiltBuildings(UnitType type)
        {
            int count = 0;
            if (!buildQueue.ContainsKey(type.getID()) || !(buildQueue[type.getID()] is List<TilePosition>))
                return count;
            
            // Get all buildings of the input type
            IEnumerable<Unit> currentBuilding = Interface().GetAllBuildings().Where(building => building.getType() == type);
			System.Threading.Thread.Sleep(10);
			foreach (Unit building in currentBuilding)
			{
				if (building.isBeingConstructed())
				{
					buildingInProgress[building] = building.getTilePosition();
					count++;
				}
				else if (building.getHitPoints() <= 0 || building.isCompleted())
				{
					buildQueue[type.getID()].Remove(building.getTilePosition());
				}
			}
			return count;
        }

        protected int CountBuildingsinProgress(UnitType type)
        {
            for (int i = buildingInProgress.Keys.Count() - 1; i > 0; i--)
            {
                Unit unit = buildingInProgress.Keys.ElementAt(i);
                if (unit.getHitPoints() == 0 || unit.isCompleted())
                {
                    buildingInProgress.Remove(unit);
                }

            }

            return buildingInProgress.Where(pair => pair.Key.getType().getID() == type.getID()).Count();
        }

        //Function to build a building
        protected bool Build(UnitType type, int timeout = 10)
        {
			try
			{
				bool building = false;
				if (builder is Unit)
				{
                    // Add to build queue
					if (!buildQueue.ContainsKey(type.getID()) || !(buildQueue[type.getID()] is List<TilePosition>))
						buildQueue[type.getID()] = new List<TilePosition>();

					//This is a list of buildings
					IEnumerable<Unit> buildings = Interface().GetAllBuildings().Where(currentBuilding => currentBuilding.getType()
						== type && currentBuilding.getTilePosition().opEquals(buildLocation));

					Unit currentBuildCommand = null;

                    // 
					if (buildings.Count() > 0)
						currentBuildCommand = buildings.First();

                    // build at the location
					builder.build(buildLocation, type);

                    // allow the sytem to catch up
					System.Threading.Thread.Sleep(100);
					building = builder.build(buildLocation, type);

					while (currentBuildCommand == null && timeout > 0)
					{
					    if (building)
						{
							buildQueue[type.getID()].Add(buildLocation);
							Console.Out.WriteLine("Builder Building " + type.getName());
							textCounter = 0;
							return building;
						}
						if (!building)
						{
							Console.Out.WriteLine("Builder Can't Build " + type.getName());
							return building;
						}
					}
					if (timeout <= 0)
					{
						building = builder.build(buildLocation, type);
						System.Threading.Thread.Sleep(300);
						if (building)
						{
							buildQueue[type.getID()].Add(buildLocation);
							Console.Out.WriteLine("Builder Building " + type.getName() + " At its Location");
							textCounter = 0;

							return building;
						}
					}
				}
				return false;
			}
			catch
			{
				return false;
			}
        }

		// Function to position buildings taking he unit type for the building size
		protected bool Position(UnitType type, int timeout = 10)
		{
			try
			{
                // can the AI afford the building
				if (CanBuildBuilding(type))
				{
					TilePosition buildPosition;

                    // get eh base location for building
					buildPosition = Interface().baseLocations[(int)Interface().currentBuildSite];

                    // if no builder selected then select one
					if (builder == null || builder.getHitPoints() <= 0)
					{
						builder = Interface().GetBuilder(buildPosition, false);
					}

                    // if the build location is further than dist and itterations, then move the builder
					double dist = DELTADISTANCE;
					if (buildLocation is TilePosition && buildPosition is TilePosition)
						dist = buildLocation.getDistance(buildPosition);
					if (buildLocation != null && dist > iterations)
					{
						move(new Position(buildLocation), builder);
						return true;
					}
					else
					{
						buildPosition = PossibleBuildLocation(buildPosition, builder, type);
                        // if build location is NULL then build at builders location
						if (buildPosition == null)
						{
							if (bwapi.Broodwar.canBuildHere(builder, new TilePosition(builder.getPosition()), type))
							{
								buildPosition = new TilePosition(builder.getPosition());
							}
						}
					}
					try
					{
						buildLocation = buildPosition;
					}
					catch
					{
						return false;
					}

					if (buildLocation is TilePosition)
					{
                        // set target to becom a new Position from the TilePosition"buildPosition"
						Position target = new Position(buildPosition);
                        // Move the builder into place
						builder.move(target, true);
						Console.Out.WriteLine("Builder Moving into Position");
                        // While the builder is to far from the build location move to location
						while (builder.getDistance(target) >= DELTADISTANCE && timeout > 0)
						{
							if (builder.move(target, true))
								break;
							System.Threading.Thread.Sleep(100);
							timeout--;
						}
                        // If the buildder takes too long to get into position a new builder is selected
						if (timeout <= 0)
						{
							Console.Out.WriteLine("Positioning Timed Out Replacing Builder");
							builder = Interface().GetBuilder(buildPosition, true);
							return false;
						}
						Console.Out.WriteLine("Builder in Position");
						return true;
					}
                    // If the builder cant get inot position and the position is NULL then attempt to build a pylon at the location
					Console.Out.WriteLine("buildLocation is NULL");
					builder.build(builder.getTilePosition(), bwapi.UnitTypes_Protoss_Pylon);
					return false;
				}
                // Stop the output box from filling up too much
				if (textCounter >= 1)
				{
					return false;
				}
				textCounter++;
				Console.Out.WriteLine("Can't Afford to Build");
				return false;
			}
			catch
			{
				return false;
			}
		}

		// Check if there is a pylon at the desirerd location
		private bool HasPylonAtLocation(int buildLocation)
		{
            // Check is buildLocation is valid
			if (Interface().baseLocations.ContainsKey(buildLocation) && Interface().baseLocations[buildLocation] is TilePosition)
			{
				Position location = new Position(Interface().baseLocations[buildLocation]);
                // go through all the buildings and select all the Pylons, then check their distance from the desired location
                // and if its within the given distance return true
				foreach (Unit unit in Interface().GetAllBuildings())
					if (unit.getType().getID() == bwapi.UnitTypes_Protoss_Pylon.getID() &&
						unit.getDistance(location) <= DELTADISTANCE)
						return true;
				return false;
			}
			return false;
		}

        //
        //Actions
        //
		[ExecutableAction("SelectNewBuilder")]
		public bool SelectNewBuilder()
		{
			if (needNewBuilder == true)
			{
				builder = Interface().GetBuilder(Interface().baseLocations[(int)Interface().currentBuildSite], true);
				needNewBuilder = false;
				return true;
			}
			return false;
		}

        // Action for finding a suitable loaction for the Protoss Assimiltor to harvest Vespin Gas
        [ExecutableAction("SelectAssimilatorLocation")]
        public bool SelectAssimilatorLocation()
        {
            // Enough resources available?
            if (!CanBuildBuilding(bwapi.UnitTypes_Protoss_Assimilator) || !Interface().baseLocations.ContainsKey((int)Interface().currentBuildSite))
                return false;

            TilePosition buildPosition = Interface().baseLocations[(int)Interface().currentBuildSite];
            // Are there any geysers available/visible?
            IEnumerable<Unit> geysers = Interface().GetGeysers().Where(geyser => geyser.getResources() > 0);
            if (geysers.Count() < 1)
                return false;

            // Sort by closest path for ground units from selected build base
            TilePosition closest = geysers
                .OrderBy(geyser => geyser.getDistance(new Position(buildPosition)))
                .First().getTilePosition();
            
            // If there is a close geyers we are done
            if (closest is TilePosition)
            {
                this.buildLocation = closest;
                builder = Interface().GetBuilder(Interface().baseLocations[(int)Interface().currentBuildSite], false);
                return true;
            }
            return false;
        }


        // Action to use the suitable location to build the protoss Assimilator
        [ExecutableAction("BuildAssimilator")]
        public bool BuildAssimilator()
        {
            //Check to see if the AI can afford the building
            if (CanBuildBuilding(bwapi.UnitTypes_Protoss_Assimilator) && buildLocation is TilePosition)
            {
                // Call the build function feeding in the unit ID
                return Build(bwapi.UnitTypes_Protoss_Assimilator);
            }
            return false;
        }


        // Action for finding a suitable loaction for the Protoss Forge
        [ExecutableAction("PositionForge")]
        public bool PositionForge()
        {
			return Position(bwapi.UnitTypes_Protoss_Forge);
        }


        // Action to use the suitable location to build the protoss Forge
        [ExecutableAction("BuildForge")]
        public bool BuildForge()
        {
            return Build(bwapi.UnitTypes_Protoss_Forge);
        }


        // Action for finding a suitable loaction for the Protoss Cybernetics Core
        [ExecutableAction("PositionCyberneticsCore")]
        public bool PositionCyberneticsCore()
        {
			return Position(bwapi.UnitTypes_Protoss_Cybernetics_Core);
        }


        // Action to use the suitable location to build the protoss Cybernetics Core
        [ExecutableAction("BuildCyberneticsCore")]
        public bool BuildCyberneticsCore()
        {
            return Build(bwapi.UnitTypes_Protoss_Cybernetics_Core);
        }


        // Action for finding a suitable loaction for the Protoss Nexus
        [ExecutableAction("PositionNexus")]
        public bool PositionNexus()
        {
            return Position(bwapi.UnitTypes_Protoss_Nexus);
        }


        // Action to use the suitable location to build the protoss Nexus
        [ExecutableAction("BuildNexus")]
        public bool BuildNexus()
        {
            return Build(bwapi.UnitTypes_Protoss_Nexus);
        }

        // Action for finding a suitable loaction for the Protoss Pylon
        [ExecutableAction("PositionPylon")]
        public bool PositionPylon()
        {
            return Position(bwapi.UnitTypes_Protoss_Pylon);
        }


        // Action to use the suitable location to build the protoss Pylon
        [ExecutableAction("BuildPylon")]
        public bool BuildPylon()
        {
            return Build(bwapi.UnitTypes_Protoss_Pylon);            
        }


        // Action for finding a suitable loaction for the Protoss Pylon that is used for defense
        [ExecutableAction("PositionChokePylon")]
        public bool PositionChokePylon()
        {
            return Position(bwapi.UnitTypes_Protoss_Pylon);
        }


        // Action to use the suitable location to build the protoss Pylon that is used for defense
        [ExecutableAction("BuildChokePylon")]
        public bool BuildChokePylon()
        {
            return Build(bwapi.UnitTypes_Protoss_Pylon);
        }


        // Action for finding a suitable loaction for the Protoss Gateway
        [ExecutableAction("PositionGateway")]
        public bool PositionGateway()
        {
            return Position(bwapi.UnitTypes_Protoss_Gateway);
        }


        // Action to use the suitable location to build the protoss GateWay
        [ExecutableAction("BuildGateway")]
        public bool BuildGateway()
        {
            return Build(bwapi.UnitTypes_Protoss_Gateway);
        }


		// Action for finding a suitable loaction for the Protoss Stargate
		[ExecutableAction("PositionStargate")]
		public bool PositionStargate()
		{
			return Position(bwapi.UnitTypes_Protoss_Stargate);
		}


		// Action to use the suitable location to build the protoss Stargate
		[ExecutableAction("BuildStargate")]
		public bool BuildStargate()
		{
			return Build(bwapi.UnitTypes_Protoss_Stargate);
		}

		// Action for finding a suitable loaction for the Protoss Fleetbeacon
		[ExecutableAction("PositionFleetbeacon")]
		public bool PositionFleetbeacon()
		{
			return Position(bwapi.UnitTypes_Protoss_Fleet_Beacon);
		}

		// Action to use the suitable location to build the protoss Fleetbeacon
		[ExecutableAction("BuildFleetbeacon")]
		public bool BuildFleetbeacon()
		{
			return Build(bwapi.UnitTypes_Protoss_Fleet_Beacon);
		}

        // Action for finding a suitable loaction for the protoss Citadel of Adun
		[ExecutableAction("PositionCitadel")]
		public bool PositionCitadel()
		{
			return Position(bwapi.UnitTypes_Protoss_Citadel_of_Adun);
		}

        // Action to use the suitable location to build the protoss Citadel of Adun
		[ExecutableAction("BuildCitadel")]
		public bool BuildCitadel()
		{
			return Build(bwapi.UnitTypes_Protoss_Citadel_of_Adun);
		}

		// Action for finding a suitable loaction for the Protoss Templar Archives
		[ExecutableAction("PositionArchives")]
		public bool PositionArchives()
		{
			return Position(bwapi.UnitTypes_Protoss_Templar_Archives);
		}

        // Action to use the suitable location to build the Protoss Templar Archives
		[ExecutableAction("BuildArchives")]
		public bool BuildArchives()
		{
			return Build(bwapi.UnitTypes_Protoss_Templar_Archives);
		}

		// Action for finding a suitable loaction for the Protoss Robotics Facility
		[ExecutableAction("PositionRoboticsFacility")]
		public bool PositionRoboticsFacility()
		{
			return Position(bwapi.UnitTypes_Protoss_Robotics_Facility);
		}

		// Action to use the suitable location to build the protoss Robotics Facility
		[ExecutableAction("BuildRoboticsFacility")]
		public bool BuildRoboticsFacility()
		{
			return Build(bwapi.UnitTypes_Protoss_Robotics_Facility);
		}

        // Action for finding a suitable loaction for the Protoss Observatory
		[ExecutableAction("PositionObservatory")]
		public bool PositionObservatory()
		{
			return Position(bwapi.UnitTypes_Protoss_Observatory);
		}

        // Action to use the suitable location to build the protoss Observatory
		[ExecutableAction("BuildObservatory")]
		public bool BuildObservatory()
		{
			return Build(bwapi.UnitTypes_Protoss_Observatory);
		}

        // Action for finding a suitable loaction for the Protoss Photon Cannon
        [ExecutableAction("PositionCannon")]
        public bool PositionCannon()
        {
            return Position(bwapi.UnitTypes_Protoss_Photon_Cannon);
        }

        // Action to use the suitable location to build the protoss Photon Cannon
        [ExecutableAction("BuildCannon")]
        public bool BuildCannon()
        {
            return Build(bwapi.UnitTypes_Protoss_Photon_Cannon);
        }

        // Action to tell the AI that this build order is finished
        [ExecutableAction("FinishedEighteenNexusOpening")]
        public bool FinishedEighteenNexusOpening()
        {
            needBuilding = false;

            return needBuilding;
        }

        //
        // SENSES
        //
        [ExecutableSense("NeedBuilding")]
        public bool NeedBuilding()
        {
            return needBuilding;
        }

        [ExecutableSense("BuildingDamaged")]
        public bool BuildingDamaged()
        {

            IEnumerable<Unit> buildings = Interface().GetAllBuildings().Where(building => building.isCompleted() && building.getHitPoints() < building.getType().maxHitPoints()).Where(building => building.getHitPoints() > 0);

            return (buildings.Count() > 0);
        }

        // Check for pylons at the choke
		[ExecutableSense("HasChokePylon")]
		public bool HasChokePylon()
		{
			return HasPylonAtLocation(4);
		}

        // Check for pylons at the naturals choke
		[ExecutableSense("HasNaturalChokePylon")]
		public bool HasNaturalChokePylon()
		{
			return HasPylonAtLocation(5);
		}

        // Check for pylons at the natural
		[ExecutableSense("HasNaturalPylon")]
		public bool HasNaturalPylon()
		{
			return HasPylonAtLocation(2);
		}

        // check for pylons at the enemy base
		[ExecutableSense("HasEnemyPylon")]
		public bool HasEnemyPylon()
		{
			return HasPylonAtLocation(5);
		}

        // Get a new builder
		[ExecutableSense("NeedNewBuilder")]
		public bool NeedNewBuilder()
		{
			return needNewBuilder;
		}

		//Returns if a pylon is being constructed
		[ExecutableSense("PylonConstructing")]
		public bool PylonConstructing()
		{
			IEnumerable<Unit> building = Interface().GetBuilding(bwapi.UnitTypes_Protoss_Pylon).Where(build => build.isBeingConstructed());
			if (building.Count() > 0)
				return true;
			else
				return false;
		}

        // Sense to return the number of protoss Nexus'
        [ExecutableSense("NexusCount")]
        public int NexusCount()
        {
			return countBuilding(bwapi.UnitTypes_Protoss_Nexus);
        }


        // Sense to return the number of protoss Forges
        [ExecutableSense("ForgeCount")]
        public int ForgeCount()
        {
			return countBuilding(bwapi.UnitTypes_Protoss_Forge);
        }


        // Sense to return the number of protoss Assimilators
        [ExecutableSense("AssimilatorCount")]
        public int AssimilatorCount()
        {
			return countBuilding(bwapi.UnitTypes_Protoss_Assimilator);
        }


        // Sense to return the number of protoss Cybernetics Cores
        [ExecutableSense("CyberneticsCoreCount")]
        public int CyberneticsCoreCount()
        {
			return countBuilding(bwapi.UnitTypes_Protoss_Cybernetics_Core);
        }


        // Sense to return the number of protoss Gateways
		[ExecutableSense("GatewayCount")]
		public int GatewayCount()
		{
			return countBuilding(bwapi.UnitTypes_Protoss_Gateway);
		}


        // Sense to return the number of protoss Pylon's
		[ExecutableSense("PylonCount")]
		public int PylonCount()
		{
			return countBuilding(bwapi.UnitTypes_Protoss_Pylon);
		}
		
        // Sense to return the number of protoss Cannon's
		[ExecutableSense("CannonCount")]
		public int CannonCount()
		{
			return countBuilding(bwapi.UnitTypes_Protoss_Photon_Cannon);
		}
		
        // Sense to return the number of protoss StarGates
		[ExecutableSense("StargateCount")]
		public int StargateCount()
		{
			return countBuilding(bwapi.UnitTypes_Protoss_Stargate);
		}
		
        // Sense to return the number of protoss Fleet Beacon's
		[ExecutableSense("FleetbeaconCount")]
		public int FleetbeaconCount()
		{
			return countBuilding(bwapi.UnitTypes_Protoss_Fleet_Beacon);
		}

		// Sense to return the number of protoss Citadel of Adun
		[ExecutableSense("CitadelCount")]
		public int CitadelCount()
		{
			return countBuilding(bwapi.UnitTypes_Protoss_Citadel_of_Adun);
		}

		// Sense to return the number of protoss Templar Archives
        [ExecutableSense("ArchivesCount")]
        public int ArchivesCount()
        {
			return countBuilding(bwapi.UnitTypes_Protoss_Templar_Archives);
        }

		// Sense to return the number of protoss Robotics Facilities
		[ExecutableSense("RoboticsFacilityCount")]
		public int RoboticsFacilityCount()
		{
			return countBuilding(bwapi.UnitTypes_Protoss_Robotics_Facility);
		}

        // Sense to return the number of protoss Observatories
		[ExecutableSense("ObservatoryCount")]
		public int ObservatoryCount()
		{
			return countBuilding(bwapi.UnitTypes_Protoss_Observatory);
		}

        // Select a unit for building a structure
        [ExecutableSense("HaveBuilder")]
        public bool HaveBuilder()
        {
            builder = UnitManager().GetProbe();

            return (builder is Unit) ? true : false;
        }

        // Sense to tell the AI whether they have a Nexus at their natural expansion
        [ExecutableSense("HaveNaturalNexus")]
        public bool HaveNaturalNExus()
        {
            TilePosition natural = Interface().baseLocations.ContainsKey((int)BuildSite.Natural) ? Interface().baseLocations[(int)BuildSite.Natural] : null;
            TilePosition start = Interface().baseLocations[(int)BuildSite.StartingLocation];

            // Natural not known
            if (natural == null)
                return false;

            // Arbitratry distance measure to determine if the Nexus is closer to the natural or the starting location
            double dist = new Position(natural).getDistance(new Position(start)) / 3;

            if (Interface().GetNexus().Where(nexus => nexus.getDistance(new Position(natural)) < dist).Count() > 0)
                return true;

            foreach (Unit unit in this.buildingInProgress.Keys)
                if (unit.getType().getID() == bwapi.UnitTypes_Protoss_Nexus.getID() &&
                    unit.getDistance(new Position(natural)) < dist)
                    return true;

            return false;
        }
    }
}
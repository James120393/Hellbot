using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using POSH.sys.annotations;
using SWIG.BWAPI;
using SWIG.BWTA;

namespace POSH_StarCraftBot.behaviours
{
    public class UnitControl : AStarCraftBehaviour
    {
        /// <summary>
        /// The int value key is identifying the location on the map by shifting the x corrdinate three digits to the left and adding the y value. 
        /// An example would be the position P(122,15) results in the key k=122015
        /// </summary>
        private Dictionary<int, List<Unit>> minedPatches;

        private bool forceReady = false;
		private bool stopZealotBuild = true;

        /// <summary>
        /// The int value key is identifying the location on the map by shifting the x corrdinate three digits to the left and adding the y value. 
        /// An example would be the position P(122,15) results in the key k=122015
        /// </summary>
        private Dictionary<int, List<Unit>> minedGas;
        
        /// <summary>
        /// The dict key is UnitType.getID() which is a numerical representation of the type The UnitType itself 
        /// would not work as a key due to a wrong/missing implementation of the hash
        /// </summary>
        private Dictionary<int, List<Unit>> morphingUnits;

        private Dictionary<int, List<Unit>> trainingUnits;


        public UnitControl(AgentBase agent)
            : base(agent, 
            new string[] {},
            new string[] {})
        {
            minedPatches = new Dictionary<int, List<Unit>>(); 
            minedGas = new Dictionary<int, List<Unit>>();
            morphingUnits = new Dictionary<int, List<Unit>>();
            trainingUnits = new Dictionary<int, List<Unit>>();
        }

        //
        // INTERNAL
        //
        private int ConvertTilePosition(TilePosition pos)
        {
            return (pos.xConst() * 1000) + pos.yConst();
        }

        // Check to see if any units are currently being trained
        protected int CheckForTrainingUnits(UnitType type)
        {
            if (!trainingUnits.ContainsKey(type.getID()))
                return 0;
            trainingUnits[type.getID()].RemoveAll(unit => !unit.isTraining());

            return trainingUnits[type.getID()].Count;
        }

		protected int CheckForDeadUnits(UnitType type)
		{
			int removeUnit = 0;
			foreach (Unit unit in Interface().GetAllUnits(true).Where(unit => unit.getType() == type && unit.getHitPoints() <= 0))
			{
				removeUnit--;
			}
			return removeUnit;
		}

        // Get any idle probes
        protected internal Unit GetProbe()
        {
            if (IdleProbes())
                return Interface().GetIdleProbes().ElementAt(0);
            //TODO:  here we could possibly take of the fact that we remove a busy probe from its current task which is not a good thing sometimes
            // this is especially the case if it is the last drone mining

            return (Interface().GetProbes(1).Count() > 0) ? Interface().GetProbes(1).ElementAt(0) : null;
        }

        // Get any idle probes
        protected internal Unit GetMineralProbe()
        {
            if (IdleProbes())
                return Interface().GetIdleProbes().ElementAt(0);
            //TODO:  here we could possibly take of the fact that we remove a busy probe from its current task which is not a good thing sometimes
            // this is especially the case if it is the last drone mining
            IEnumerable<Unit>  probes = Interface().GetProbes(5);
            probes.First().isGatheringMinerals();
            return (Interface().GetProbes(1).Count() > 0) ? Interface().GetProbes(1).ElementAt(0) : null;
        }

        // Function to send probes to gather minerals
        public bool ProbesToResource(IEnumerable<Unit> resources, Dictionary<int, List<Unit>> mined, int threshold, bool onlyIdle, int maxUnits)
        {
            IEnumerable<Unit> probes;
            int[] mineralTypes = { bwapi.UnitTypes_Resource_Mineral_Field.getID(), bwapi.UnitTypes_Resource_Mineral_Field_Type_2.getID(), bwapi.UnitTypes_Resource_Mineral_Field_Type_3.getID() };
            bool executed = false;
            if (onlyIdle)
                probes = Interface().GetIdleProbes();
            else
                probes = Interface().GetProbes().Where(probe => !Interface().IsBuilder(probe));

            if (probes.Count() < 1 || resources.Count() < 1 || probes == null)
                return executed;

            // Update all minded Patches by removing non harvesting probes or dead ones
            foreach (KeyValuePair<int, List<Unit>> patch in minedPatches)
            {
                patch.Value.RemoveAll(probe => (probe.getHitPoints() <= 0 || probe.getOrderTarget() == null || ConvertTilePosition(probe.getOrderTarget().getTilePosition()) != patch.Key));
            }

            foreach (Unit probe in probes.OrderBy(probe => probe.getResources()))
            {
                if (maxUnits < 1)
                    break;
				if (probe == null)
				{
					return false;
				}
                if (probe.getOrderTarget() is Unit && probe.getTarget() is Unit && resources.Contains(probe.getOrderTarget()) && probe.getTarget().getResources() > 0 &&
                    mined.ContainsKey(ConvertTilePosition(probe.getOrderTarget().getTilePosition())))
                {
                    continue;
                }

                IEnumerable<Unit> patchPositions = resources.
                    Where(patch => patch.hasPath(probe)).
                    OrderBy(patch => probe.getDistance(patch));
                Unit finalPatch = patchPositions.First();
                int positionValue;

                foreach (Unit position in patchPositions)
                {
                    positionValue = ConvertTilePosition(position.getTilePosition());
                    // A better distribution over resources would be beneficial 
                    if (!mined.ContainsKey(positionValue) || mined[positionValue].Count <= threshold)
                    {
                        finalPatch = position;
						break;
                    }

                }
                int secCounter = 10;
                while (!(probe.getTarget() is Unit && probe.getTarget().getID() == finalPatch.getID()) && !probe.isMoving() && secCounter-- > 0)
                {
                    executed = probe.gather(finalPatch, false);
                    maxUnits--;
                    System.Threading.Thread.Sleep(50);
                    // if (_debug_)
                    //Console.Out.WriteLine("Probe is gathering: " + executed);
                }

                positionValue = ConvertTilePosition(finalPatch.getTilePosition());
                if (!mined.ContainsKey(positionValue))
                {
                    mined.Add(positionValue, new List<Unit>());
                }

                mined[positionValue].Add(probe);
            }

            return true;
        }

		// Function to Train units
		protected bool TrainProbe(UnitType type, bool naturalBuild)
		{
            // Can afford to build
			if (CanTrainUnit(type))
			{
				int targetLocation = (int)BuildSite.StartingLocation;

				if (Interface().baseLocations.ContainsKey((int)Interface().currentBuildSite))
					targetLocation = (int)Interface().currentBuildSite;

                // Get all Nexus' available 
				IEnumerable<Unit> prodBuildings = Interface().GetNexus();

                // If there are none then return false
				if (prodBuildings.Count() <= 0)
					return false;

                // If the natural bool is true then only build at the natural expansion
				if (naturalBuild)
				{
                    // Return false if the build queue is 5
					if (prodBuildings.Last().getTrainingQueue().Count() < 5)
					{
						bool trainSuccess = prodBuildings.Last().train(type);
						if (trainSuccess)
						{
							Console.Out.WriteLine("Training Unit: " + type.getName() + " At Natural");
							return true;
						}
					}					
				}
                // Go thorugh each Nexus and train probes until build queue's are full
				else
				{
					foreach (Unit build in prodBuildings)
					{
						if (build.getTrainingQueue().Count() < 5)
						{
							bool trainSuccess = build.train(type);
							if (trainSuccess)
							{
								Console.Out.WriteLine("Training Unit: " + type.getName());
							}
						}
					}
				}
				return true;
			}
			return false;
		}

        // Function to Train units
		protected bool TrainUnit(UnitType type, UnitType building, bool single, int timeout = 50)
		{
            // Can afford to build
			if (CanTrainUnit(type))
			{
				int targetLocation = (int)BuildSite.StartingLocation;

				if (Interface().baseLocations.ContainsKey((int)Interface().currentBuildSite))
					targetLocation = (int)Interface().currentBuildSite;

                // Get all buildings of the same type as the input
				IEnumerable<Unit> prodBuildings = Interface().GetBuilding(building);
				if (prodBuildings.Count() <= 0)
					return false;

                // Go thorugh each buildiong and train the selected unit until build queue's are full
				foreach (Unit build in prodBuildings)
				{
                    // Return false if the build queue is 5
					if (build.getTrainingQueue().Count() < 1 && timeout > 0)
					{
						bool trainSuccess = build.train(type);
						if (trainSuccess)
						{
							Console.Out.WriteLine("Training Unit: " + type.getName());
                            // If true will only train one unit and will not fill up the build queue
							if (single)
								return true;
						}
					}
				}
			}
			return false;
		}

        //
        // ACTIONS
        //
        // Action to tell the AI that its forces are finished being trained
        [ExecutableAction("FinishedForce")]
        public bool FinishedForce()
        {
            forceReady = true;
            return true;
        }

        // Action to tell the AI that its forces are not finished building
        [ExecutableAction("NotFinishedForce")]
        public bool NotFinishedForce()
        {
            forceReady = false;
            return false;
        }

        //Action to tell the AI to Build a Protoss Probe
        [ExecutableAction("BuildProbe")]
        public bool BuildProbe()
        {
            return TrainProbe(bwapi.UnitTypes_Protoss_Probe, false);
        }

        //Action to tell the AI to Build a Protoss Probe at the natural expansion
        [ExecutableAction("BuildNaturalProbe")]
        public bool BuildNaturalProbe()
        {
            return TrainProbe(bwapi.UnitTypes_Protoss_Probe, true);
        }

        //Action to tell the AI to Build a Protoss Zealot
        [ExecutableAction("TrainZealot")]
        public bool TrainZealot()
        {
            return TrainUnit(bwapi.UnitTypes_Protoss_Zealot, bwapi.UnitTypes_Protoss_Gateway, false);
        }

        //Action to tell the AI to Never build Zealots
        [ExecutableAction("StopZealot")]
        public bool StopZealot()
        {
            stopZealotBuild = false;
            return true;
        }

        //Action to tell the AI to Build a Protoss Dragoon
        [ExecutableAction("TrainDragoon")]
        public bool TrainDragoon()
        {
            return TrainUnit(bwapi.UnitTypes_Protoss_Dragoon, bwapi.UnitTypes_Protoss_Gateway, false);
        }

        //Action to tell the AI to Build a single Protoss Dragoon
        [ExecutableAction("TrainSingleDragoon")]
        public bool TrainSingleDragoon()
        {
            return TrainUnit(bwapi.UnitTypes_Protoss_Dragoon, bwapi.UnitTypes_Protoss_Gateway, true);
        }

        //Action to tell the AI to Build a Protoss Corsair
        [ExecutableAction("TrainCorsair")]
        public bool TrainCorsair()
        {
            return TrainUnit(bwapi.UnitTypes_Protoss_Corsair, bwapi.UnitTypes_Protoss_Stargate, false);
        }

        //Action to tell the AI to Build a Protoss Carrier
        [ExecutableAction("TrainCarrier")]
        public bool TrainCarrier()
        {
            return TrainUnit(bwapi.UnitTypes_Protoss_Carrier, bwapi.UnitTypes_Protoss_Stargate, false);
        }

        //Action to tell the AI to Build a Protoss Dark Templar
        [ExecutableAction("TrainDarkTemplar")]
        public bool TrainDarkTemplar()
        {
            return TrainUnit(bwapi.UnitTypes_Protoss_Dark_Templar, bwapi.UnitTypes_Protoss_Gateway, false);
        }

        //Action to tell the AI to Build a Protoss Observer
        [ExecutableAction("TrainObserver")]
        public bool TrainObserver()
        {
            return TrainUnit(bwapi.UnitTypes_Protoss_Observer, bwapi.UnitTypes_Protoss_Robotics_Facility, false);
        }

        //Action to tell the AI to Never build Zealots
        [ExecutableSense("CanTrainZealot")]
        public bool CanTrainZealot()
        {
            return stopZealotBuild;
        }

        //Action to tell the AI to Assign Probes to gather minerals
        [ExecutableAction("AssignProbes")]
        public bool AssignProbes()
        {
            IEnumerable<Unit> mineralPatches = Interface().GetMineralPatches();
            return ProbesToResource(mineralPatches, minedPatches, 6, true, 1);
        }


        //Action to tell the AI to Assign Probes to gather Vespin Gas
        [ExecutableAction("AssignToGas")]
        public bool AssignToGas()
        {
            IEnumerable<Unit> assimilators = Interface().GetBuilding(bwapi.UnitTypes_Protoss_Assimilator);

            return ProbesToResource(assimilators, minedGas, 2, true, 1);
        } 

        //
        // SENSES
        //
        // Sense to tell the AI that their forces are ready
        [ExecutableSense("ForceReady")]
        public bool ForceReady()
        {
            return forceReady;
        }
        
        [ExecutableSense("CanAttack")]
        public bool CanAttack()
        {

            return Interface().GetAllUnits(false).Where(unit => !unit.isUnderAttack() && !unit.isAttacking()).Count() > 10 || forceReady;
        }

        // Sense to tell the AI how many Idle Probes it has
        [ExecutableSense("IdleProbes")]
        public bool IdleProbes()
        {
            return (Interface().GetIdleProbes().Count() > 0) ? true : false;
        }


        //Sense to tell the AI how many Probes it has
        [ExecutableSense("ProbeCount")]
        public int ProbeCount()
        {
            return Interface().ProbeCount() + CheckForTrainingUnits(bwapi.UnitTypes_Protoss_Probe);
        }


        //Sense to tell the AI how many Dragoons it has
        [ExecutableSense("DragoonCount")]
        public int DragoonCount()
        {
            return Interface().DragoonCount() + CheckForTrainingUnits(bwapi.UnitTypes_Protoss_Dragoon);
        }


        //Sense to tell the AI how many Zealots it has
        [ExecutableSense("ZealotCount")]
        public int ZealotCount()
        {
			int count = Interface().ZealotCount() + CheckForTrainingUnits(bwapi.UnitTypes_Protoss_Zealot) - CheckForDeadUnits(bwapi.UnitTypes_Protoss_Zealot);
			if (count <= 0)
				return 0;
 			else
				return count;
        }


        //Sense to tell the AI how many Dark Templars it has
        [ExecutableSense("DarkTemplarCount")]
        public int DarkTemplarCount()
        {
            return Interface().DarkTemplarCount() + CheckForTrainingUnits(bwapi.UnitTypes_Protoss_Dark_Templar);
        }


        //Sense to tell the AI how many Corsairs it has
        [ExecutableSense("CorsairCount")]
        public int CorsairCount()
        {
            return Interface().CorsairCount() + CheckForTrainingUnits(bwapi.UnitTypes_Protoss_Corsair);
        }


		//Sense to tell the AI how many Carriers it has
		[ExecutableSense("CarrierCount")]
		public int CarrierCount()
		{
			return Interface().CarrierCount() + CheckForTrainingUnits(bwapi.UnitTypes_Protoss_Carrier);
		}

		//Sense to tell the AI how many Observers it has
		[ExecutableSense("ObserverCount")]
		public int ObserverCount()
		{
			return Interface().ObserverCount() + CheckForTrainingUnits(bwapi.UnitTypes_Protoss_Observer);
		}

		//Sense to tell the AI how many carriers it has not including those training
		[ExecutableSense("CarrierCountNotTraining")]
		public int CarrierCountNotTraining()
		{
			return Interface().CorsairCount() - CheckForTrainingUnits(bwapi.UnitTypes_Protoss_Carrier);
		}

		//Action to tell the AI if it has any combat units
		[ExecutableSense("HasUnits")]
		public bool HasUnits()
		{
			try
			{
				if (Interface().GetAllUnits(false).Count() > 0)
					return true;
				else
					return false;
			}
			catch
			{
				return false;
			}
		}
       
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using POSH.sys.annotations;
using System.Threading;
using SWIG.BWAPI;

namespace POSH_StarCraftBot.behaviours
{
    public class ResourceControl : AStarCraftBehaviour
    {
        private bool finishedResearch;
		private bool needResearch = true;

        public ResourceControl(AgentBase agent)
            : base(agent, new string[] {}, new string[] {})
        {

        }
        //
        // INTERNAL
        //

		private bool HaveResearch(UpgradeType research)
		{
			return Interface().Self().getUpgradeLevel(research) > 0;
		}

        // Attempt to do research at the desired building
		private bool DoResearch(UpgradeType research, IEnumerable<Unit> building)
		{
			try
			{
				return building.Where(build => !build.isUpgrading() && build.getHitPoints() > 0).First().upgrade(research);
			}
			catch
			{
				return false;
			}
		}

        //
        // ACTIONS
        //

        // Build interceptors for each carrier, until its queue is full
		[ExecutableAction("BuildInterceptors")]
		public bool BuildInterceptors()
		{
			foreach (Unit carrier in Interface().GetCarrier())
			{
				for (int i = 0; i < 5; i++)
				{
					if (carrier.getTrainingQueue().Count() < 5)
					{
						carrier.train(bwapi.UnitTypes_Protoss_Interceptor);
					}
					else
					{
						break;
					}
				}
				continue;
			}		
			return true;
		}

        // Set the need research to false
		[ExecutableAction("DoNotNeedResearch")]
		public bool DoNotNeedResearch()
		{
			needResearch = false;
			return true;
		}

        //Action to tell AI to research the Protoss Dragoon Range upgrade
        [ExecutableAction("DragoonRangeUpgrade")]
        public bool DragoonRangeUpgrade()
        {
            return DoResearch(bwapi.UpgradeTypes_Singularity_Charge, Interface().GetCyberneticsCore());
        }

        //Action to tell AI to research the Protoss Shield upgrade
        [ExecutableAction("ShieldUpgrade")]
        public bool ShieldUpgrade()
        {
			return DoResearch(bwapi.UpgradeTypes_Protoss_Plasma_Shields, Interface().GetForge());
        }

		//Action to tell AI to research the Protoss Air Weapon upgrade
		[ExecutableAction("AirWepUpgrade")]
		public bool AirWepUpgrade()
		{
			return DoResearch(bwapi.UpgradeTypes_Protoss_Air_Weapons, Interface().GetCyberneticsCore());
		}

		//Action to tell AI to research the Protoss Observer Range upgrade
		[ExecutableAction("ObserverUpgrade")]
		public bool ObserverUpgrade()
		{
			return DoResearch(bwapi.UpgradeTypes_Sensor_Array, Interface().GetObservatory());
		}

		//Action to tell AI to research the Protoss Ground Weapon upgrade
		[ExecutableAction("GroundWepUpgrade")]
		public bool GroundWepUpgrade()
		{
			return DoResearch(bwapi.UpgradeTypes_Protoss_Ground_Weapons, Interface().GetForge());
		}

		//Action to tell AI to research the Protoss Carrier capacity upgrade
		[ExecutableAction("CarrierUpgrade")]
		public bool CarrierUpgrade()
		{
			return DoResearch(bwapi.UpgradeTypes_Carrier_Capacity, Interface().GetFleetbeacon());
		}

		//Action to tell AI to research the Protoss Legs "Makes Zealots Faster"
		[ExecutableAction("LegsUpgrade")]
		public bool LegsUpgrade()
		{
			return DoResearch(bwapi.UpgradeTypes_Leg_Enhancements, Interface().GetCitadel());
		}

        //
        // SENSES
        //

        // Check to see if any interceptors are needed within the Carrier
		[ExecutableSense("InterceptorsNeeded")]
		public bool InterceptorsNeeded()
		{
			IEnumerable<Unit> carrier = Interface().GetCarrier();
			foreach (Unit c in carrier)
			{
				if (c.getInterceptorCount() < 8)
				{
					if (c.getTrainingQueue().Count() >= 5)
					{
						continue;
					}
					return true;
				}
				continue;
			}
			return false;
		}

        // Check if all research needed is done
        [ExecutableSense("DoneResearch")]
        public bool DoneResearch()
        {
            return finishedResearch;
        }

        // Return the total supply
        [ExecutableSense("TotalSupply")]
        public int TotalSupply()
        {
            return Interface().TotalSupply();
        }

        // Return the current usage of supply
        [ExecutableSense("Supply")]
        public int SupplyCount()
        {
            return Interface().SupplyCount();
        }

        // Return the Available supply
        [ExecutableSense("AvailableSupply")]
        public int AvailableSupply()
        {
            return Interface().AvailableSupply();
        }

        // Return the amount of gas available
        [ExecutableSense("Gas")]
        public int Gas()
        {
            return Interface().GasCount();
        }

        // Return the amount of minerals available
        [ExecutableSense("Minerals")]
        public int Minerals()
        {
            return Interface().MineralCount();
        }

        //Sense to tell AI if they have the protoss Carrier capacity upgrade
		[ExecutableSense("HaveCarrier")]
		public bool HaveCarrier()
		{
			return (Interface().Self().getUpgradeLevel(bwapi.UpgradeTypes_Carrier_Capacity) > 0 || Interface().Self().isUpgrading(bwapi.UpgradeTypes_Carrier_Capacity));
		}
		
        //Sense to tell AI if they have the protoss Dragoon range upgrade
        [ExecutableSense("HaveDragoonRange")]
        public bool HaveDragoonRange()
        {
			return (Interface().Self().getUpgradeLevel(bwapi.UpgradeTypes_Singularity_Charge) > 0 || Interface().Self().isUpgrading(bwapi.UpgradeTypes_Singularity_Charge));
        }

        //Sense to tell AI if they have the protoss Shield upgrade
        [ExecutableSense("HaveShield")]
        public bool HaveShield()
        {
			return (Interface().Self().getUpgradeLevel(bwapi.UpgradeTypes_Protoss_Plasma_Shields) > 0 || Interface().Self().isUpgrading(bwapi.UpgradeTypes_Protoss_Plasma_Shields));
        }

		//Sense to tell AI if they have the protoss Ground Weapon upgrade
		[ExecutableSense("HaveGroundWep")]
		public bool HaveGroundWep()
		{
			return (Interface().Self().getUpgradeLevel(bwapi.UpgradeTypes_Protoss_Ground_Weapons) > 0 || Interface().Self().isUpgrading(bwapi.UpgradeTypes_Protoss_Ground_Weapons));
		}

		//Sense to tell AI if they have the protoss Observer Range upgrade
		[ExecutableSense("HaveOberverUpgrade")]
		public bool HaveOberverUpgrade()
		{
			return (Interface().Self().getUpgradeLevel(bwapi.UpgradeTypes_Sensor_Array) > 0 || Interface().Self().isUpgrading(bwapi.UpgradeTypes_Sensor_Array));
		}

		//Sense to tell AI if they have the protoss Air Weapon upgrade
		[ExecutableSense("HaveAirWep")]
		public bool HaveAirWep()
		{
			return (Interface().Self().getUpgradeLevel(bwapi.UpgradeTypes_Protoss_Air_Weapons) > 0 || Interface().Self().isUpgrading(bwapi.UpgradeTypes_Protoss_Air_Weapons));
		}

		//Sense to tell AI if they have the protoss Legs upgrade
		[ExecutableSense("HaveLegs")]
		public bool HaveLegs()
		{
			return (Interface().Self().getUpgradeLevel(bwapi.UpgradeTypes_Leg_Enhancements) > 0 || Interface().Self().isUpgrading(bwapi.UpgradeTypes_Leg_Enhancements));
		}

        // Are any of the buildings researching
		[ExecutableSense("IsResearching")]
		public bool IsResearching()
		{
			return (Interface().GetForge().Where(forge => forge.getHitPoints() > 0).First().isUpgrading() || Interface().GetCyberneticsCore().Where(core => core.getHitPoints() > 0).First().isUpgrading()
                || Interface().GetCitadel().Where(citadel => citadel.getHitPoints() > 0).First().isUpgrading() || Interface().GetObservatory().Where(obs => obs.getHitPoints() > 0).First().isUpgrading()
                || Interface().GetFleetbeacon().Where(fleet => fleet.getHitPoints() > 0).First().isUpgrading());
		}

        // Are any forges researching
		[ExecutableSense("IsForgeResearching")]
		public bool IsForgeResearching()
		{
			return (Interface().GetForge().Where(forge => forge.getHitPoints() > 0).First().isUpgrading());
		}

        // Are any Observatories researching
		[ExecutableSense("IsObservatoryResearching")]
		public bool IsObservatoryResearching()
		{
			return (Interface().GetObservatory().Where(observatory => observatory.getHitPoints() > 0).First().isUpgrading());
		}

        // Are any Cybernetics Core's researching
		[ExecutableSense("IsCoreResearching")]
		public bool IsCoreResearching()
		{
			return (Interface().GetCyberneticsCore().Where(core => core.getHitPoints() > 0).First().isUpgrading());
		}

        // Are any Fleet Beacons researching
        [ExecutableSense("IsFleetResearching")]
        public bool IsFleetResearching()
        {
            return (Interface().GetFleetbeacon().Where(core => core.getHitPoints() > 0).First().isUpgrading());
        }

        // Are any Citadel of Adun researching
        [ExecutableSense("IsCitadelResearching")]
        public bool IsCitadelResearching()
        {
            return (Interface().GetCitadel().Where(core => core.getHitPoints() > 0).First().isUpgrading());
        }

        // Deas the AI need any research
		[ExecutableSense("NeedResearch")]
		public bool NeedResearch()
		{
			return needResearch;
		}        
    }
}

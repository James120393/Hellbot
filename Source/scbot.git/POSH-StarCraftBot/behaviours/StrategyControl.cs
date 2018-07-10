using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using POSH.sys.annotations;
using System.Threading;
using SWIG.BWAPI;
using SWIG.BWTA;
using POSH_StarCraftBot.logic;

namespace POSH_StarCraftBot.behaviours
{

    public enum Strategy { EighteenNexusOpening = 0, TwoHatchMuta = 1, Zergling = 2 }

    public class StrategyControl : AStarCraftBehaviour
    {
        private Unit scout;
		private Unit probeScout;
        private Position lureCentroid;
        /// <summary>
        /// The scout counter contains the number of bases we already discovered moving from Startlocation towards the most distant ones.
        /// bases are retrieved using bwta.getBaselocations
        /// </summary>
        private int scoutCounter = 1;
        private Strategy currentStrategy;
        private bool startStrategy = true;
        private float alarm = 0.0f;
        private GamePhase phase;
        private bool buildArmy = false;
        private int maxBaseLocations;

		//public bool naturalHasBeenFound = false;
        public StrategyControl(AgentBase agent)
            : base(agent, new string[] { }, new string[] { })
        {

        }

		public Unit GetScout()
		{
			return scout;
		}

		//
        // INTERNAL
        //
        private bool SwitchBuildToBase(int location)
        {
            if (Interface().baseLocations.ContainsKey(location) && Interface().baseLocations[location] is TilePosition)
            {
                Interface().currentBuildSite = (BuildSite)location;
                return true;
            }

            return false;
        }

		// Send scout to scout
		private bool ScoutToBase(bool enemy)
		{
            // Select the start position
			TilePosition startLoc = Interface().baseLocations[(int)BuildSite.StartingLocation];

            // Pause the system for the gma eot catch up
			System.Threading.Thread.Sleep(500);

            // If there is no scout select one
			if (probeScout == null || probeScout.getHitPoints() <= 0)
				probeScout = Interface().GetProbes().First();

            // If the list is empty then return false
			if (Interface().basePositions.Count() < 1)
			{
				return false;
			}

            // set the target location
			Position target = Interface().basePositions.First().getPosition();

            // if its the enemy thats being scouted then set target to enemy base
			if (enemy)
				target = Interface().basePositions.Last().getPosition();

            // Random base location for scout to retreat to
			Position randTarget = Interface().basePositions.Skip(2).First().getPosition();

			if (!enemy)
			{
                // While the scout is not close enough to the location continue moving the scout
				while (probeScout.getDistance(target) >= DELTADISTANCE)
				{
					probeScout.move(target, false);
                    // Check if the scout is dead
					if (probeScout.getHitPoints() <= 0)
					{
						Console.Out.WriteLine("Probe scout dead");
						return false;
					}
                    // If the scout is not moving then move the scout
					if (!probeScout.isMoving() && probeScout.getHitPoints() > 0)
					{
						probeScout.move(target, false);
						Console.Out.WriteLine("Probe to Natural");
					}
					System.Threading.Thread.Sleep(50);
				}
                // once close enough, then move the scout to another base
				probeScout.move(randTarget, false);
				Console.Out.WriteLine("Probe at Natural");

                // Add the location to the disctionary
				Interface().baseLocations.Add(2, Interface().basePositions.First().getTilePosition());
				return true;
			}
			if (enemy)
			{
                // Check if the scout is dead
				if (probeScout.getHitPoints() <= 0)
				{
					Console.Out.WriteLine("Probe scout dead");
					return false;
				}

                // Go thorugh each base location and move the scout to each one.
				foreach (BaseLocation baseLoc in Interface().basePositions.Reverse())
				{
                    // if the scout is not moving then move the scout
					if (!probeScout.isMoving() && probeScout.getHitPoints() > 0)
					{
						probeScout.move(baseLoc.getPosition(), true);
						Console.Out.WriteLine("Probe Searching for Enemy Base");
					}

                    // When the scout is close enough to a base location check for enemy buildings
					if (probeScout.getDistance(baseLoc.getPosition()) <= DELTADISTANCE *2)
					{
						IEnumerable<Unit> enemyBuildings = bwapi.Broodwar.enemy().getUnits().Where(units => units.getHitPoints() > 0).Where(units => units.getType().isBuilding());
						
                        // If there are enemy buildings
						if (enemyBuildings != null)
						{
                            // Go through each building and if it is visible then add that base location to the dictionary
							foreach (Unit building in enemyBuildings)
							{
								try
								{
									Interface().baseLocations.Add(5, baseLoc.getTilePosition());
									Interface().forcePoints[ForceLocations.EnemyStart] = baseLoc.getTilePosition();
									Console.Out.WriteLine("Enemy Building Detected");
									return true;
								}
								catch
								{
									return false;
								}
							}
						}
					}
				}
				return true;
			}
			return false;
		}

        // Select the choke point for either the start location or the natural expansions location.
		private bool SelectChoke(bool startChoke)
		{
			// get the distance between start and natural 
			BuildSite site = Interface().currentBuildSite;
			TilePosition start = Interface().baseLocations[(int)BuildSite.StartingLocation];
			TilePosition targetChoke = null;
			Chokepoint chokepoint = null;

			if (!startChoke)
			{
				targetChoke = Interface().baseLocations[2];
				double distance = start.getDistance(targetChoke);

				// find some kind of measure to determine if the the closest choke to natural is not the once between choke and start but after the natural
				IEnumerable<Chokepoint> chokes = bwta.getChokepoints().Where(ck => bwta.getGroundDistance(new TilePosition(ck.getCenter()), start) > 0).OrderBy(choke => choke.getCenter().getDistance(new Position(targetChoke)));
				if (chokes == null)
				{
					return false;
				}
				foreach (Chokepoint ck in chokes)
				{

					if (bwta.getGroundDistance(new TilePosition(ck.getCenter()), targetChoke) < bwta.getGroundDistance(new TilePosition(ck.getCenter()), start))
					{
						chokepoint = ck;
						break;
					}
				}

			}
			else
			{
				targetChoke = start;
				chokepoint = bwta.getChokepoints().Where(ck => bwta.getGroundDistance(new TilePosition(ck.getCenter()), start) > 0).OrderBy(choke => choke.getCenter().getDistance(new Position(start))).First();
			}



			if (chokepoint == null)
				return false;

			//picking the right side of the choke to position forces
			Interface().forcePoints[ForceLocations.NaturalChoke] = (targetChoke.getDistance(new TilePosition(chokepoint.getSides().first)) < targetChoke.getDistance(new TilePosition(chokepoint.getSides().second))) ? new TilePosition(chokepoint.getSides().first) : new TilePosition(chokepoint.getSides().second);
			Interface().currentForcePoint = ForceLocations.NaturalChoke;

			if (!Interface().baseLocations.ContainsKey((int)BuildSite.Choke))
				Interface().baseLocations.Add(4, Interface().forcePoints[ForceLocations.NaturalChoke]);

			return true;

		}

        //
        // ACTIONS
        //
        // Select the natural expansion as a build location
        [ExecutableAction("SelectNatural")]
        public bool SelectNatural()
        {
            Interface().currentBuildSite = BuildSite.Natural;
            return SwitchBuildToBase((int)BuildSite.Natural);
        }

        // Select the start base as a build location
        [ExecutableAction("SelectStartBase")]
        public bool SelectStartBase()
        {
			Interface().currentBuildSite = BuildSite.StartingLocation;
            return SwitchBuildToBase((int)BuildSite.StartingLocation);
        }

        // Select the extension area as a build location
        [ExecutableAction("SelectExtension")]
        public bool SelectExtension()
        {
            return SwitchBuildToBase((int)BuildSite.Extension);
        }

        // Let the AI do nothing
        [ExecutableAction("Idle")]
        public bool Idle()
        {
            try
            {
                Thread.Sleep(50);
                return true;
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Idle Action Crashed");
                return false;
            }
        }

        // Send the scout to natural expansion
		[ExecutableAction("ProbeToNatural")]
		public bool ProbeToNatural()
		{
			return ScoutToBase(false);
		}

        // Send the scout to the enemy base
		[ExecutableAction("ScoutToEnemyBase")]
		public bool ScoutToEnemyBase()
		{
			return ScoutToBase(true);
		}

        // Select the choke for a build location
		[ExecutableAction("SelectChokeBuild")]
		public bool SelectChokeBuild()
		{
			SelectChoke(true);
			Interface().currentBuildSite = BuildSite.Choke;
			return SwitchBuildToBase((int)BuildSite.Choke);
		}

        // Select the natural expansions choke for build location
		[ExecutableAction("SelectNaturalChokeBuild")]
		public bool SelectNaturalChokeBuild()
		{
			SelectChoke(false);
			Interface().currentBuildSite = BuildSite.NaturalChoke;
			return SwitchBuildToBase((int)BuildSite.NaturalChoke);
		}

        // Select the choke point for forces to move to
		[ExecutableAction("SelectChokeForceLocation")]
		public bool SelectChokeForceLocation()
		{
			return SelectChoke(false);
		}

        // Select the enemies choke for a build location
		[ExecutableAction("SelectEnemyChokeBuild")]
		public bool SelectEnemyChokeBuild()
		{
			if (Interface().enemyBuildingChoke is TilePosition)
			{
				Interface().currentBuildSite = BuildSite.EnemyChoke;
				return true;
			}
			return false;
		}

        //
        // SENSES
        //
        // Returns if the enemy base has been found
		[ExecutableSense("EnemyBaseFound")]
		public bool EnemyBaseFound()
		{
			if (Interface().forcePoints.ContainsKey(ForceLocations.EnemyStart))//Interface().enemyBaseFound;
				return true;
			return false;
		}
		
        // Returns if the natural has been found
        [ExecutableSense("NaturalFound")]
        public bool NaturalFound()
        {
			if (Interface().baseLocations.ContainsKey(2))
				return true;
			return false;
        }

        /// <summary>
        /// Returns the enemy race once it is known. The options are: -1 for unknown, 0 for Zerg, 1 for Terran, 2 for Protoss
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("EnemyRace")]
        public int EnemyRace()
        {
            // currently we only expect 1-on-1 matches so there should be only one other player in the game
            int enemyRace = Interface().ActivePlayers.First().Value.getRace().getID();
            if (enemyRace == bwapi.Races_Unknown.getID())
            {
                Interface().enemyRace = Races.Unknown;
                return (int)Races.Unknown;
            }
            if (enemyRace == bwapi.Races_Zerg.getID())
            {
                Interface().enemyRace = Races.Zerg;
                return 0;
            } 
			if (enemyRace == bwapi.Races_Terran.getID())
            {
                Interface().enemyRace = Races.Terran;
                return 1;
            }
            if (enemyRace == bwapi.Races_Protoss.getID())
            {
                Interface().enemyRace = Races.Protoss;
                return 2;
            }


            return -1;
        }

        [ExecutableSense("GameRunning")]
        public bool GameRunning()
        {
            return Interface().GameRunning();
        }

        [ExecutableSense("FollowStrategy")]
        public int FollowStrategy()
        {
            return 1;
        }

        // See what the ration is for pobes gathering minerals to those gathering gas
        [ExecutableSense("ProbeAssimilatorRatio")]
        public float ProbeAssimilatorRatio()
        {
            // For every three probes gathering minerals, one probe should be gathering gas
			int ratio = Interface().GetProbes().Where(probe => probe.isGatheringMinerals()).Count() / 3;
			int gatheringGas = ratio - Interface().GetProbes().Where(probe => probe.isGatheringGas()).Count();
			if (gatheringGas >= 1)
				return 1;
			else
				return 0;
        }

        ///////////////////////////////////////////////////////////////Un-used Code Saved for later use, Please disregard all following code///////////////////////////////////////////////////////////////
        //
        // Action
        //
        [ExecutableAction("Pursue18NexusOpening")]
        public bool Pursue18NexusOpening()
        {
            currentStrategy = Strategy.EighteenNexusOpening;
            return (currentStrategy == Strategy.EighteenNexusOpening) ? true : false;
        }

        [ExecutableSense("ProbesLured")]
        public bool ProbesLured()
        {
            return Interface().GetProbes().Where(probe => probe.isUnderAttack()).Count() > 0;
        }


        [ExecutableSense("ProbeScoutAvailable")]
        public bool ProbeScoutAvailable()
        {
			return (probeScout is Unit && probeScout.getHitPoints() > 0 && !probeScout.isConstructing() && !probeScout.isRepairing()) ? true : false;
        }

        [ExecutableAction("ProbeScouting")]
        public bool ProbeScouting()
        {
            // no scout alive or selected
            if (probeScout == null || probeScout.getHitPoints() <= 0)
                SelectProbeScout();
            if (probeScout != null)
            {
                IEnumerable<BaseLocation> baseloc = Interface().basePositions;

                if (baseloc.Count() > maxBaseLocations)
                    maxBaseLocations = baseloc.Count();
                if (baseloc.Count() < maxBaseLocations && scoutCounter > baseloc.Count())
                    return false;
                // reached the last accessible base location away and turn back to base
                if (scoutCounter > maxBaseLocations)
                    scoutCounter = 1;


                if (probeScout.isUnderAttack())
                {
                    if (Interface().baseLocations.ContainsKey((int)BuildSite.Natural))
                        probeScout.move(new Position(Interface().baseLocations[(int)BuildSite.Natural]));
                    else if (Interface().baseLocations.ContainsKey((int)BuildSite.Extension))
                    {
                        probeScout.move(new Position(Interface().baseLocations[(int)BuildSite.Extension]));
                    }
                    return false;
                }


                // still scouting
                if (probeScout.isMoving())
                    return true;

                double distance = probeScout.getPosition().getDistance(
                    baseloc.OrderBy(loc => bwta.getGroundDistance(loc.getTilePosition(), Interface().baseLocations[(int)BuildSite.StartingLocation]))
                        .ElementAt(scoutCounter)
                        .getPosition()
                        );
                if (distance < DELTADISTANCE * 2)
                {
                    // close to another base location
                    if (!Interface().baseLocations.ContainsKey(scoutCounter))
                        Interface().baseLocations[scoutCounter] = new TilePosition(probeScout.getTargetPosition());
                    scoutCounter++;
                    IEnumerable<Unit> enemyBuildings = bwapi.Broodwar.enemy().getUnits().Where(units => units.getHitPoints() > 0).Where(units => units.getType().isBuilding());
                    if (enemyBuildings != null)
                    {
                        foreach (Unit building in enemyBuildings)
                        {
                            try
                            {
                                Interface().baseLocations.Add(5, baseloc.ElementAt(scoutCounter).getTilePosition());
                                Interface().forcePoints[ForceLocations.EnemyStart] = baseloc.ElementAt(scoutCounter).getTilePosition();
                                Console.Out.WriteLine("Enemy Building Detected");
                                return true;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    bool executed = false;
                    if (baseloc.Count() > scoutCounter)
                        executed = probeScout.move(baseloc.ElementAt(scoutCounter).getPosition());
                    // if (_debug_)
                    Console.Out.WriteLine("Probe is scouting: " + executed);
                    return executed;
                }
            }
            return true;
        }

        [ExecutableAction("NeedArmy")]
        public bool NeedArmy()
        {
            buildArmy = true;
            return buildArmy;
        }

        [ExecutableAction("RespondToLure")]
        public bool RespondToLure()
        {
            IEnumerable<Unit> probes = Interface().GetProbes().Where(probe => probe.isUnderAttack());

            if (probes.Count() > 1)
                lureCentroid = CombatControl.CalculateCentroidPosition(probes);
            foreach (Unit probe in probes)
            {
                if (probe.isCarryingMinerals())
                {
                    probe.gather(Interface().GetBuilding(bwapi.UnitTypes_Protoss_Assimilator).OrderBy(extr => extr.getDistance(probe)).First());
                }
                else if (probe.isCarryingGas())
                {
                    probe.gather(Interface().GetMineralPatches().OrderBy(extr => extr.getDistance(probe)).First());
                }
                else
                {
                    probe.move(new Position(Interface().baseLocations[(int)BuildSite.StartingLocation]));
                }
                alarm += 0.1f;

            }

            return alarm > 0 ? true : false;
        }


        [ExecutableAction("SelectLuredBase")]
        public bool SelectLuredBase()
        {
            int location = Interface().baseLocations.OrderBy(loc => new Position(loc.Value).getApproxDistance(lureCentroid)).First().Key;
            ForceLocations target;
            switch (location)
            {
                case (int)ForceLocations.OwnStart:
                    target = ForceLocations.OwnStart;
                    break;
                case (int)ForceLocations.Natural:
                    target = ForceLocations.Natural;
                    break;
                case (int)ForceLocations.Extension:
                    target = ForceLocations.Extension;
                    break;
                default:
                    target = ForceLocations.Natural;
                    break;

            }
            Interface().currentForcePoint = target;
            return true;


        }

        [ExecutableAction("CounterWithForce")]
        public bool CounterWithForce()
        {
            //TODO: this needs to be implemented
            return false;
        }

        [ExecutableAction("SelectProbeScout")]
        public bool SelectProbeScout()
        {
            if (probeScout != null && probeScout.getHitPoints() > 0)
                return true;

            Unit scout = null;
            IEnumerable<Unit> units = Interface().GetProbes().Where(probe =>
                probe.getHitPoints() > 0).OrderBy(probe => bwta.getGroundDistance(probe.getTilePosition(), Interface().baseLocations[1]));

            if (units.Count() > 0)
            {
                scout = units.Last();
            }
            probeScout = scout;

            return (probeScout is Unit && probeScout.getHitPoints() > 0) ? true : false;
        }

        //
        // Sense
        //
        [ExecutableSense("HaveScout")]
        public bool HaveScout()
        {
            if (scout == null || scout.getHitPoints() <= 0)
                return false;
            else
                return true;
        }


        [ExecutableSense("DoneExploring")]
        public bool DoneExploring()
        {
            if (Interface().baseLocations.Count() == bwta.getBaseLocations().Count())
                return true;

            return false;
        }


        [ExecutableSense("NeedProbeAtNatural")]
        public bool NeedProbeAtNatural()
        {
            if (Interface().baseLocations.ContainsKey((int)BuildSite.Natural))
                return false;
            if (scout == null || scout.getHitPoints() == 0)
                return true;

            TilePosition startLoc = Interface().baseLocations[(int)BuildSite.StartingLocation];

            IOrderedEnumerable<BaseLocation> pos = bwta.getBaseLocations()
                .Where(baseLoc => !baseLoc.getTilePosition().opEquals(startLoc) && bwta.getGroundDistance(startLoc, baseLoc.getTilePosition()) > 0)
                .OrderBy(baseLoc => bwta.getGroundDistance(startLoc, baseLoc.getTilePosition()));
            Console.Out.WriteLine("startLoc: " + startLoc.xConst() + " " + startLoc.yConst());
            //foreach(BaseLocation poi in pos)
            //Console.Out.WriteLine("Loc: " + poi.getTilePosition().xConst() + " " + poi.getTilePosition().yConst() + " dist: " + bwta.getGroundDistance(startLoc, poi.getTilePosition()));
            if (pos.Count() < 1)
                return true;

            double dist = scout.getDistance(pos.First().getPosition());
            Console.Out.WriteLine("distance from natural: " + dist);
            if (dist < DELTADISTANCE && !Interface().baseLocations.ContainsKey((int)BuildSite.Natural))
            {
                Interface().baseLocations[(int)BuildSite.Natural] = pos.First().getTilePosition();

                //if (_debug_)
                Console.Out.WriteLine("probe to natural: " + Interface().baseLocations[(int)BuildSite.Natural].xConst() + " " + Interface().baseLocations[(int)BuildSite.Natural].yConst());
                scout = null;
                return false;
            }

            return true;
        }


        [ExecutableSense("DoneExploringLocal")]
        public bool DoneExploringLocal()
        {
            if (Interface().baseLocations.ContainsKey((int)BuildSite.Natural))
            {
                return true;
            }
            return false;
        }


        [ExecutableSense("BuildArmy")]
        public bool BuildArmy()
        {
            return buildArmy;
        }


        [ExecutableSense("Alarm")]
        public float Alarm()
        {
            alarm = (alarm < 0.0f) ? 0.0f : alarm - 0.05f;

            return alarm;
        }

    }
}

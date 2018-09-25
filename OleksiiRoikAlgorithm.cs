using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robot.Common;


namespace OleksiiRoik.RobotChallenge
{

  

    public class OleksiiRoikAlgorithm : IRobotAlgorithm
    {

        Dictionary<int, RobotInfo> RobotsInfo;

        enum RobotState
        {
            Moving,
            Collecting,
            Weak, 
            Attacking

        }

        public OleksiiRoikAlgorithm()
        {
            RobotsInfo = new Dictionary<int, RobotInfo>();
         
            
        }

        //Допоміжні функції
        struct RobotInfo
        {
            public RobotState state;
            public Position interest;
            public int isDividing;



        }

       

        public bool IsCellFree(Position cell, Robot.Common.Robot movingRobot, IList<Robot.Common.Robot> robots)
        {
            foreach (var robot in robots)
            {
                    if (robot.Position == cell)
                        return false;
                
            }
            return true;
        }

        public bool isStantionNotInterested(Position a)
        {
            foreach (var robot in RobotsInfo)
            {
                
                if (robot.Value.interest == a)
                {
                    return false;
                }
              
            }

            return true;
        }

        public bool isRobotInStation(Robot.Common.Robot movingRobot,
            IList<Robot.Common.Robot> robots, Map map)
        {
            foreach (var station in map.Stations)
            {
                if (station.Position == movingRobot.Position)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsStationFree(EnergyStation station, Robot.Common.Robot movingRobot,
            IList<Robot.Common.Robot> robots)
        {
            return IsCellFree(station.Position, movingRobot, robots) && isStantionNotInterested(station.Position);
        }
        public Position FindNearestFreeStation(int robotToMoveIndex, Map map, IList<Robot.Common.Robot> robots)
        {
            EnergyStation nearest = null;
            Robot.Common.Robot movingRobot = robots[robotToMoveIndex];
            int minDistance = Int32.MaxValue;
        
           
            foreach (var station in map.Stations)
            {
                if (isStantionNotInterested(station.Position))
                {
            
                    int d = DistanceHelper.FindDistance(station.Position, movingRobot.Position);
                    if (d < minDistance)
                    {

                        if (RobotsInfo.Count == 0)
                        {
                            minDistance = d;
                            nearest = station;
                        }
                        else
                        {
                            foreach (var RobotI in RobotsInfo)
                            {
                                if (RobotI.Value.interest == station.Position && RobotI.Key != robotToMoveIndex)
                                {
                                    continue;
                                }
                                else
                                {
                                    minDistance = d;
                                    nearest = station;

                                }
                            }
                        }
                     
                        
                    }
                }
            }
            
            return nearest == null ? null : nearest.Position;
        }

        

        public double EnergyToMove(Position a, Position b)
        {
            return Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2);
        }

        public bool isEnoughEnergyToMove(Position a, Position b, int energy)
        {
            if (energy < EnergyToMove(a,b))
            {
                return false;
            }

            return true;
        }

        public bool isEnemyOnStation(Position pos, int robotToMoveIndex,
            IList<Robot.Common.Robot> robots)
        {
            var enemies = robots.Where(r => r.Owner != robots[robotToMoveIndex].Owner);
            foreach (var enemy in enemies)
            {
                if (enemy.Position == pos)
                {
                    return true;
                }
            }

            return false;

        }

        public bool RobotInitialize(IList<Robot.Common.Robot> robots, int robotToMoveIndex, Map map)
        {
            Robot.Common.Robot movingRobot = robots[robotToMoveIndex];
            RobotInfo newRobotInfo;
            Position stationPosition = FindNearestFreeStation(robotToMoveIndex, map, robots);
            newRobotInfo.interest = stationPosition;
            var enemies = robots.Where(r => r.Owner != robots[robotToMoveIndex].Owner);
            var allies = robots.Where(r => r.Owner == robots[robotToMoveIndex].Owner);
            //foreach (var enem in enemies)
            //{
            //    if (DistanceHelper.FindDistance(enem.Position, robots[robotToMoveIndex].Position) < 100 && enem.Energy > robots[robotToMoveIndex].Energy && allies.Count() > 10)
            //    {
            //        newRobotInfo.interest = enem.Position;
            //        newRobotInfo.state = RobotState.Attacking;
            //    }
            //}
            if (EnergyToMove(movingRobot.Position, stationPosition) > 400)
            {
                newRobotInfo.state = RobotState.Weak;
            }
            else if (isRobotInStation(movingRobot, robots, map))
            {
                newRobotInfo.interest = movingRobot.Position;
                newRobotInfo.state = RobotState.Collecting;
            }

            else
            {
              
                newRobotInfo.state = RobotState.Moving;
            }

            newRobotInfo.isDividing = 0;
            RobotsInfo.Add(robotToMoveIndex,newRobotInfo);
            return true;
        }

        public bool isEnemyNear(IList<Robot.Common.Robot> robots, int robotToMoveIndex, Map map)
        {
            var enemies = robots.Where(r => r.Owner != robots[robotToMoveIndex].Owner);
            foreach (var enem in enemies)
            {
                if (DistanceHelper.FindDistance(enem.Position, robots[robotToMoveIndex].Position) < 100 && enem.Energy > robots[robotToMoveIndex].Energy)
                {
                    return true;
                }
            }

            return false;
        }
        public bool isCollectReady(IList<Robot.Common.Robot> robots, int robotToMoveIndex, Map map)
        {
            int d = DistanceHelper.FindDistance(RobotsInfo[robotToMoveIndex].interest, robots[robotToMoveIndex].Position);
            if (d <= 2)
            {
                return true;
            }

            return false;
        }

        public RobotCommand DoStep(IList<Robot.Common.Robot> robots, int robotToMoveIndex, Map map)
        {
            
            if (!RobotsInfo.ContainsKey(robotToMoveIndex))
            {
                RobotInitialize(robots, robotToMoveIndex, map);
            }
            var movingRobInfo = RobotsInfo[robotToMoveIndex];
            Robot.Common.Robot movingRobot = robots[robotToMoveIndex];
            //if (RobotsInfo[robotToMoveIndex].state == RobotState.Attacking)
            //{
            //    if (!isEnemyNear(robots, robotToMoveIndex, map))
            //    {
            //        RobotInfo temp = RobotsInfo[robotToMoveIndex];
            //        if (isCollectReady(robots, robotToMoveIndex, map))
            //        {
            //            temp.state = RobotState.Collecting;
            //        }
            //        else
            //        {
            //            temp.state = RobotState.Moving;
            //        }

            //        temp.interest = movingRobot.Position;
            //        RobotsInfo[robotToMoveIndex] = temp;
            //    }
            //    else
            //    {


            //        return new MoveCommand() { NewPosition = RobotsInfo[robotToMoveIndex].interest };
            //    }
            //}
             if (movingRobInfo.state == RobotState.Moving)
            {
                if (isCollectReady(robots,robotToMoveIndex,map))
                {
                    RobotInfo temp = RobotsInfo[robotToMoveIndex];
                    temp.state = RobotState.Collecting;
                    RobotsInfo[robotToMoveIndex] = temp;
                    
                }

                if (isEnoughEnergyToMove(movingRobot.Position, movingRobInfo.interest, movingRobot.Energy))
                {
                    return new MoveCommand() {NewPosition = movingRobInfo.interest};
                }
                else
                {
                    
                    int Step = 0;

                    Position stationPosition = movingRobInfo.interest;
                    if (isEnemyOnStation(stationPosition, robotToMoveIndex, robots))
                    {
                        Step = 2;
                    }
                    else
                    {
                        Step = 4;
                    }
                    Position newPosition = movingRobot.Position;
                     if (stationPosition.X > movingRobot.Position.X)
                         {
                             if(Math.Abs(stationPosition.X - movingRobot.Position.X) < 4)
                             {
                                 newPosition.X = stationPosition.X;
                             }
                         else if (stationPosition.Y == movingRobot.Position.Y)
                        {
                                    newPosition.X += Step;
                        }
                        else
                        {
                            newPosition.X += movingRobot.Energy / 35 + 1;
                        }
                    }
                    else if (stationPosition.X < movingRobot.Position.X)
                    {
                        if (Math.Abs(stationPosition.X - movingRobot.Position.X) < 4)
                        {
                            newPosition.X = stationPosition.X;
                        }
                        else if (stationPosition.Y == movingRobot.Position.Y)
                        {
                            newPosition.X -= Step;
                        }
                        else
                        {
                            newPosition.X -= movingRobot.Energy / 35 + 1;
                        }

                    }
                    if (stationPosition.Y > movingRobot.Position.Y)
                    {
                        if (Math.Abs(stationPosition.Y - movingRobot.Position.Y) < 4)
                        {
                            newPosition.Y = stationPosition.Y;
                        }
                        else if (stationPosition.X == movingRobot.Position.X)
                        {
                            newPosition.Y += Step;
                        }
                        else
                        {
                            newPosition.Y += movingRobot.Energy / 35 + 1;
                        }

                    }
                    else if (stationPosition.Y < movingRobot.Position.Y)
                    {
                        if (Math.Abs(stationPosition.Y - movingRobot.Position.Y) < 4)
                        {
                            newPosition.Y = stationPosition.Y;
                        }
                        else if (stationPosition.X == movingRobot.Position.X)
                        {
                            newPosition.Y -= Step;
                        }
                        else
                        {
                            newPosition.Y -= movingRobot.Energy/35 + 1;
                        }

                    }

                    if (!isEnoughEnergyToMove(movingRobot.Position, newPosition, (movingRobot.Energy)))
                    {
                        RobotInfo temp = RobotsInfo[robotToMoveIndex];
                        temp.state = RobotState.Weak;
                        temp.interest = movingRobot.Position;
                        RobotsInfo[robotToMoveIndex] = temp;
                        return new MoveCommand() { NewPosition = movingRobot.Position };
                    }

                    return new MoveCommand() { NewPosition = newPosition };
                }
            } 
            else if (movingRobInfo.state == RobotState.Collecting)
            {
                if (movingRobot.Energy >= 420)
                {
                    Position newPosition = FindNearestFreeStation(robotToMoveIndex, map, robots);
                    int EnergyToNewRobot = 100;
                    if (isEnemyOnStation(newPosition, robotToMoveIndex, robots))
                    {
                        EnergyToNewRobot = 200;
                    }
                    if (isEnoughEnergyToMove(movingRobot.Position, newPosition, 420)  && isStantionNotInterested(newPosition))
                    {
                        //RobotInfo temp = RobotsInfo[robotToMoveIndex];
                        //temp.state = RobotState.Moving;
                        //temp.interest = newPosition;
                        //temp.isDividing++;
                        //RobotsInfo[robotToMoveIndex] = temp;
                        return new CreateNewRobotCommand(){NewRobotEnergy = EnergyToNewRobot};
                    }
                  



                }
                return new CollectEnergyCommand();
            }
            else if (RobotsInfo[robotToMoveIndex].state == RobotState.Weak)
            {
                if (isCollectReady(robots, robotToMoveIndex, map))
                {
                    return new CollectEnergyCommand();
                }
                else
                {
                    return new MoveCommand(){NewPosition = movingRobot.Position };

                }
            }
           




                return null;
            
        }

        public string Author {
            get { return "Roik Oleksii"; }
        }
        public string Description {
            get { return "Robit AI Algorithm"; }
        }
    }
}

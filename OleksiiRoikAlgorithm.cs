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

        public int roundNumber;
        public Dictionary<int, RobotInfo> RobotsInfo;

        //Перелічуваний тип станів робота
        public enum RobotState
        {
            Moving,
            Collecting,
            Weak, 
            Attacking

        }

        public OleksiiRoikAlgorithm()
        {
            RobotsInfo = new Dictionary<int, RobotInfo>();
            Logger.OnLogRound += Logger_OnLogRound;
            roundNumber  = 0;


        }

        private void Logger_OnLogRound(object sender, LogRoundEventArgs e)
        {
           roundNumber++;
        }

        //Структруа, що зберігає інформацію про роботів потрібну для роботи алгоритму
        public struct RobotInfo
        {
            public RobotState state;
            public Position interest;
            public int isDividing;



        }

       
        //Перевірка чи на клітинці не знаходяться роботи
        public bool IsCellFree(Position cell, Robot.Common.Robot movingRobot, IList<Robot.Common.Robot> robots)
        {
            foreach (var robot in robots)
            {
                    if (robot.Position == cell)
                        return false;
                
            }
            return true;
        }

        //Перевірка  чи станцією не зацікавлений інший робот
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

        //Перевірка чи робот знаходиться на станції
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

        //Перевірка чи станція вільна
        public bool IsStationFree(EnergyStation station, Robot.Common.Robot movingRobot,
            IList<Robot.Common.Robot> robots)
        {
            return IsCellFree(station.Position, movingRobot, robots) && isStantionNotInterested(station.Position);
        }

        //Знайти найближчу станцію
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
                    if (isEnemyOnStation(station.Position, robotToMoveIndex, robots))
                    {
                        d *= 2;
                    }

                    if (d < minDistance)
                    {

                        if (RobotsInfo.Count == 0)
                        {
                            minDistance = d;
                            nearest = station;
                        }
                        else
                        {
                        
                                    minDistance = d;
                                    nearest = station;

                                
                            
                        }
                     
                        
                    }
                }
            }
            
            return nearest == null ? null : nearest.Position;
        }

        

        //Обчислення енергії для миттєвого руху до позиції
        public double EnergyToMove(Position a, Position b)
        {
            return Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2);
        }

        //Перевірка чи роботу вистачає енергії для руху до позиції
        public bool isEnoughEnergyToMove(Position a, Position b, int energy, int robotToMoveIndex,
            IList<Robot.Common.Robot> robots)
        {
            int potentialEnergy = energy;
            if (isEnemyOnStation(b,robotToMoveIndex,robots))
            {
                potentialEnergy += 30;
            }
            if (potentialEnergy >= EnergyToMove(a,b))
            {
                return true;
            }

            return false;
        }

        //Перевірка чи ворог знаходиться на станціЇ
        public bool isEnemyOnStation(Position pos, int robotToMoveIndex,
            IList<Robot.Common.Robot> robots)
        {
            var enemies = robots.Where(r => r.Owner.Name != robots[robotToMoveIndex].Owner.Name);
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
            if (EnergyToMove(movingRobot.Position, stationPosition) > 520)
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

            var myRobots = robots.Where(d => d.Owner.Name == robots[robotToMoveIndex].Owner.Name);
            int robotsCount = myRobots.Count();
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
                    return new CollectEnergyCommand();

                }

                if (isEnoughEnergyToMove(movingRobot.Position, movingRobInfo.interest, movingRobot.Energy,robotToMoveIndex,robots))
                {
                    return new MoveCommand() {NewPosition = movingRobInfo.interest};
                }
                else
                {
                    
                    int Step = 0;

                    Position stationPosition = movingRobInfo.interest;
                    if (isEnemyOnStation(stationPosition, robotToMoveIndex, robots))
                    {
                        Step = 4;
                    }
                    else
                    {
                        Step = 5;
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
                                    newPosition.X += Step + 2;
                        }
                        else
                        {
                            newPosition.X += Step;
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
                            newPosition.X -= Step + 2;
                        }
                        else
                        {
                            newPosition.X -= Step;
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
                            newPosition.Y += Step + 2;
                        }
                        else
                        {
                            newPosition.Y += Step;
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
                            newPosition.Y -= Step + 2;
                        }
                        else
                        {
                            newPosition.Y -= Step;
                        }

                    }

                    if (!isEnoughEnergyToMove(movingRobot.Position, newPosition, (movingRobot.Energy), robotToMoveIndex, robots))
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
            if (movingRobInfo.state == RobotState.Collecting)
            {
                if (robots[robotToMoveIndex].Position != RobotsInfo[robotToMoveIndex].interest)
                {
                    return new MoveCommand() {NewPosition = RobotsInfo[robotToMoveIndex].interest};
                }
             
               
                    Position newPosition = FindNearestFreeStation(robotToMoveIndex, map, robots);
                    int EnergyToNewRobot = (int)EnergyToMove(movingRobot.Position, newPosition) + 40;
                    if (isEnemyOnStation(newPosition, robotToMoveIndex, robots))
                    {
                        EnergyToNewRobot += 30;
                    }
                if (EnergyToNewRobot >= 500)
                {
                    EnergyToNewRobot /= 2;
                    EnergyToNewRobot += 30;
                }
                if (movingRobot.Energy >= (EnergyToNewRobot + 200) && isStantionNotInterested(newPosition))
                    {
                        if (robotsCount >= 72)
                        {
                            return new CollectEnergyCommand();
                        }

                        if (EnergyToNewRobot < movingRobot.Energy + 250)
                        {
                           if (EnergyToNewRobot >= 700)
                            {
                                return new CollectEnergyCommand();
                            }

                            return new CreateNewRobotCommand() {NewRobotEnergy = EnergyToNewRobot};
                        }
                        //RobotInfo temp = RobotsInfo[robotToMoveIndex];
                        //temp.state = RobotState.Moving;
                        //temp.interest = newPosition;
                        //temp.isDividing++;
                        //RobotsInfo[robotToMoveIndex] = temp;
                        //if (EnergyToNewRobot > 150)
                        //{
                        //    EnergyToNewRobot = 150;
                        //}


                    }



                return new CollectEnergyCommand();
            }
             
            if (RobotsInfo[robotToMoveIndex].state == RobotState.Weak)
            {
                if (isCollectReady(robots, robotToMoveIndex, map))
                {
                    return new CollectEnergyCommand();
                }
                else if(robots[robotToMoveIndex].Energy > 10)
                {
                    Position stationPosition = FindNearestFreeStation(robotToMoveIndex, map, robots);
                    if (isEnoughEnergyToMove(movingRobot.Position, stationPosition, (movingRobot.Energy)*3, robotToMoveIndex, robots))
                    {
                        RobotInfo temp = RobotsInfo[robotToMoveIndex];
                        temp.state = RobotState.Moving;
                        temp.interest = stationPosition;
                        RobotsInfo[robotToMoveIndex] = temp;
                        return new MoveCommand() { NewPosition = stationPosition};
                    }

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

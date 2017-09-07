using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ForcesGame
{
    class GameState
    {
        private long levelLength;
        public int numPoints {get; set;}
        private int goalPoints;
        public bool started { get; set; }
        public bool won { get; set; }
        public bool timeExpired { get; set; }
        public bool levelLoaded { get; set; }
        private long start;

        public int goalX { get; set; }
        public int goalY { get; set;  }
        public int goalRadius { get; set; }

        public GameState()
        {
            this.started = false;
            won = false;
            timeExpired = false;
            numPoints = 0;
            levelLoaded = false;
        }

        public void startGame(TimeSpan startTime)
        {
            this.start = startTime.Ticks;
            started = true;
            won = false;
            timeExpired = false;
        }

        public void setLength(int mins)
        {
            this.levelLength = (new TimeSpan(0, mins, 0)).Ticks;
        }

        public TimeSpan getTimeLeft(TimeSpan currentTime)
        {
            long timePassed = currentTime.Ticks - start;
            return TimeSpan.FromTicks(levelLength - timePassed);
        }

        public void setGoal(int x, int y, int radius, int goalPoints)
        {
            this.goalPoints = goalPoints;
            this.goalX = x;
            this.goalY = y;
            this.goalRadius = radius;
        }

        public string goalPointsString()
        {
            return numPoints.ToString() + "/" + goalPoints.ToString();
        }

        public void checkTime(TimeSpan currentTime)
        {
            if (getTimeLeft(currentTime).Ticks < (new TimeSpan(0, 0, 0).Ticks))
            {
                timeExpired = true;
                started = false;
            }
        }

        public bool checkParticle(Particle p)
        {
            int distance = (int)Math.Sqrt(Math.Pow(p.X - goalX, 2) + Math.Pow(p.Y - goalY, 2));
            //if ((p.X > goalX - goalRadius && p.X < goalX + goalRadius) && (p.Y > goalY - goalRadius && p.Y < goalY + goalRadius))
            if(distance < goalRadius)
            {
                if (numPoints < goalPoints)
                {
                    numPoints++;
                }
                else
                {
                    this.won = true;
                    this.started = false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

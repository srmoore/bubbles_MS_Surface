using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ForcesGame
{
    class Particle
    {
        private int divider = 100;
        private int xPos;
        public int X
        {
            get { return xPos; }
        }

        private int yPos;
        public int Y
        {
            get { return yPos; }
        }

        public int xVelocity { get; set; }
        public int yVelocity { get; set; }

        private int decelerationRate = 0; // pixels per update?

        private int _lifeSpan = 0;
        public int lifeSpan { 
            get { return _lifeSpan; } 
            set { this._lifeSpan = value; }
        }

        public void timeStep() { _lifeSpan++; }

        public void setPos(int x, int y)
        {
            xPos = x;
            yPos = y;
        }

        public void setVelocity(int xV, int yV)
        {
            xVelocity = xV;
            yVelocity = yV;
        }

        public bool isStopped()
        {
            if (xVelocity == 0 && yVelocity == 0)
                return true;
            else
                return false;
        }
        public int[] nextPosition()
        {
            int[] pos = new int[2];
            pos[0] = xPos + xVelocity / divider;
            pos[1] = yPos + yVelocity / divider;
            return pos;
        }

        public void update()
        {
            xPos = xPos + xVelocity / divider;
            yPos = yPos + yVelocity / divider;
            _lifeSpan++;

            if (xVelocity > 0)
            {
                xVelocity = xVelocity - decelerationRate;
                if (xVelocity < 0)
                    xVelocity = 0;
            }
            else
            {
                xVelocity = xVelocity + decelerationRate;
                if (xVelocity > 0)
                    xVelocity = 0;
            }

            if (yVelocity > 0)
            {
                yVelocity = yVelocity - decelerationRate;
                if (yVelocity < 0)
                    yVelocity = 0;
            }
            else
            {
                yVelocity = yVelocity + decelerationRate;
                if (yVelocity > 0)
                    yVelocity = 0;
            }
        }

    }
}

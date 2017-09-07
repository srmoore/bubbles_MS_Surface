using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ForcesGame
{
    class Effector
    {
        public int xPos { get; set; }
        public int yPos { get; set; }
        public int contactX { get; set; }
        public int contactY { get; set; }
        public int gridWidthX { get; set; } // number of grid squares to effect on the x axis ( times 2 )
        public int gridWidthY { get; set; }

        public double energyX { get; set; }
        public double energyY { get; set; }

        public int scaleX { get; set; }
        public int scaleY { get; set; }

        private int lastX = -99999;
        private int lastY = -99999;
        private int maxEnergy = 800;

        public int getMaxEnergy()
        {
            return maxEnergy;
        }

        /*public void move(int newX, int newY)
        {
            if(lastX == -99999)
                lastX = xPos;
            if (lastY == -99999)
                lastY = yPos;
            int moveX = newX - lastX;
            int moveY = newY - lastY;
            double xScale = ((double)moveX / ((double)scaleX) * maxEnergy);
            double yScale = ((double)moveY / ((double)scaleY) * maxEnergy);
            
            lastX = newX;
            lastY = newY;
            energyX = energyX + xScale;
            if (energyX > maxEnergy)
                energyX = maxEnergy;
            if (energyX < -1 * maxEnergy)
                energyX = -1 * maxEnergy;
            energyY = energyY + yScale;
            if (energyY > maxEnergy)
                energyY = maxEnergy;
            if (energyY < -1 * maxEnergy)
                energyY = -1 * maxEnergy;
        }*/
        public void move(int newX, int newY)
        {
            this.contactY = newY;
            this.contactX = newX;
            int moveX = (newX - xPos);
            int moveY = (newY - yPos);
            double xScale = ((double)moveX / ((double)scaleX)) * maxEnergy;
            double yScale = ((double)moveY / ((double)scaleY)) * maxEnergy;
            energyX = xScale;
            if (energyX > maxEnergy)
                energyX = maxEnergy;
            if (energyX < -1 * maxEnergy)
                energyX = -1 * maxEnergy;
            energyY = yScale;
            if (energyY > maxEnergy)
                energyY = maxEnergy;
            if (energyY < -1 * maxEnergy)
                energyY = -1 * maxEnergy;
        }
    }
}

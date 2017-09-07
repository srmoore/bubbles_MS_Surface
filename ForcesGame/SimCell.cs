using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ForcesGame
{
    class SimCell
    {
        public bool isWall { get; set; }
        private int maxEnergy = 100;
        private double _energyX;
        public double energyX
        {
            get { return _energyX; }
            set
            {
                if (value > this.maxEnergy)
                {
                    this._energyX = this.maxEnergy;
                }
                else if (value < this.maxEnergy * -1)
                {
                    this._energyX = -1 * this.maxEnergy;
                }
                else
                {
                    this._energyX = value;
                }
            }
        }
        private double _energyY;
        public double energyY
        {
            get { return _energyY; }
            set
            {
                if (value > maxEnergy)
                    _energyY = maxEnergy;
                else if (value < maxEnergy * -1)
                    _energyY = -1 * maxEnergy;
                else
                    _energyY = value;
            }
        }
        public int xPos {get; set;} 
        public int yPos {get; set;}

        public int getMaxEnergy() { return this.maxEnergy; }
        public void setMaxEnergy(int newValue) { this.maxEnergy = newValue; }

        public SimCell(int maxEnergy)
        {
            this.maxEnergy = maxEnergy;
        }

        public SimCell()
        {
            this.maxEnergy = 40;
        }
    }
}

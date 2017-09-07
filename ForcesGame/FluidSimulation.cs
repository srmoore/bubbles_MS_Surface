using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ForcesGame
{
    class FluidSimulation
    {
        int numCellsX;
        int numCellsY;
        int cellPixelsX;
        int cellPixelsY;
        SimCell[] simCells;
        Texture2D gridTexture;
        public Texture2D arrowTexture { get; set; }
        SpriteBatch gridBatch;
        //SpriteBatch arrowBatch;
        
        // simulation params
        double decayAmount = .6;
        int maxEnergy = 100;
        double tangentDecay = .7;


        public FluidSimulation(int numX, int numY, int cellX, int cellY, GraphicsDevice graphics){
            // set the size of the simulation
            this.numCellsX = numX;
            this.numCellsY = numY;
            this.cellPixelsX = cellX;
            this.cellPixelsY = cellY;

            // initialize the simulation cells
            simCells = new SimCell[numX * numY];
            for (int y = 0; y < numCellsY; y++)
            {
                for (int x = 0; x < numCellsX; x++)
                {
                    SimCell currentCell = new SimCell();
                    currentCell.setMaxEnergy(maxEnergy);
                    currentCell.xPos = x * cellPixelsX;
                    currentCell.yPos = y * cellPixelsY;
                    currentCell.energyX = 0;// x - (numCellsX / 2);
                    currentCell.energyY = 0;// y - numCellsY / 2;
                    simCells[x + (y * numCellsX)] = currentCell;
                }
            }

            gridBatch = new SpriteBatch(graphics);
            //arrowBatch = new SpriteBatch(graphics);

            // create the two textures
            Color[] gridArray = new Color[cellPixelsX * cellPixelsY];
            for (int y = 0; y < cellPixelsY; y++)
            {
                for (int x = 0; x < cellPixelsX; x++)
                {
                    if (x == 0 || y == 0 || x == cellPixelsX - 1 || y == cellPixelsY - 1)
                        gridArray[x + (y * cellPixelsX)] = Color.White;
                    else
                        gridArray[x + (y * cellPixelsX)] = Color.Black;
                }
            }
            gridTexture = new Texture2D(graphics, cellPixelsX, cellPixelsY, 0, TextureUsage.None, SurfaceFormat.Color);
            gridTexture.SetData<Color>(gridArray);
        }

        public void setWall(int x, int y)
        {
            if (getCell(x, y) > 0 && getCell(x, y) < simCells.Length)
                simCells[getCell(x,y)].isWall = true;
        }

        public void clearWalls()
        {
            for (int i = 0; i < simCells.Length; i++)
                simCells[i].isWall = false;
        }

        public void Draw()
        {
            gridBatch.Begin(SpriteBlendMode.AlphaBlend);
            Color drawColor;
            for (int i = 0; i < simCells.Length; i++)
            {

                if (simCells[i].isWall)
                    drawColor = Color.Tomato;
                else
                    drawColor = new Color(10, 10, 10); // Color.Gray;
                gridBatch.Draw(gridTexture, new Vector2(simCells[i].xPos, simCells[i].yPos), null, drawColor, 0, Vector2.Zero, 1, SpriteEffects.None,1);
                float rotation = (float)Math.Atan2(simCells[i].energyY, simCells[i].energyX);
                if (simCells[i].energyX != 0 || simCells[i].energyY != 0)
                {
                    double scale = Math.Sqrt(Math.Pow(simCells[i].energyX, 2) + Math.Pow(simCells[i].energyY, 2)) / Math.Sqrt(Math.Pow(simCells[i].getMaxEnergy(), 2) + Math.Pow(simCells[i].getMaxEnergy(), 2));
                    if (scale > 0)
                        scale = scale + .1;
                    if (scale > 1)
                        scale = 1;
                     drawColor = new Color((byte)(Color.LightGreen.R * scale),(byte)(Color.LightGreen.G * scale),(byte)(Color.LightGreen.B * scale));
                    gridBatch.Draw(arrowTexture, new Vector2(simCells[i].xPos + this.cellPixelsX / 2, simCells[i].yPos + this.cellPixelsY / 2), null, drawColor, rotation, new Vector2(arrowTexture.Width / 2, arrowTexture.Height / 2), ((cellPixelsX * .5f) / arrowTexture.Width), SpriteEffects.None, 0.9f);
                }
            }
            gridBatch.End();

            //            arrowBatch.End();
        }

        private int getSimCellNum(int xPos, int yPos)
        {
            int remainder;
            int cellX = 0;
            if(xPos > 0 && xPos < numCellsX * cellPixelsX)
                cellX = Math.DivRem(xPos, cellPixelsX, out remainder);
            int cellY = 0;
            if(yPos > 0 && yPos < numCellsY * cellPixelsY)
                cellY = Math.DivRem(yPos, cellPixelsY, out remainder);
            return cellX + (cellY * numCellsX);
        }

        private int getCell(int x, int y)
        {
            return x + y * numCellsX;
        }

        public void Update(){ //int mouseX, int mouseY){

            SimCell[] newCells = new SimCell[simCells.Length];


            // Decay energy
            for (int i = 0; i < simCells.Length; i++)
            {
                newCells[i] = new SimCell(simCells[i].getMaxEnergy());
                newCells[i].xPos = simCells[i].xPos;
                newCells[i].yPos = simCells[i].yPos;
                newCells[i].isWall = simCells[i].isWall;
                //newCells[i].energyX = simCells[i].energyX * decayAmount;
                //if (newCells[i].energyX < 1 && newCells[i].energyX > -1)
                    newCells[i].energyX = 0;
                //newCells[i].energyY = simCells[i].energyY * decayAmount;
                //if (newCells[i].energyY < 1 && newCells[i].energyY > -1)
                    newCells[i].energyY = 0;
            }

            for (int yCell = 0; yCell < numCellsY; yCell++)
            {
                for (int xCell = 0; xCell < numCellsX; xCell++)
                {
                    // skip cells that have no energy and are not walls
                    if ((simCells[xCell + yCell * numCellsX].energyX != 0 ||
                        simCells[xCell + yCell * numCellsX].energyY != 0) && !simCells[xCell + yCell * numCellsX].isWall)
                    {
                        // propagate energy
                        double energyX = simCells[xCell + yCell * numCellsX].energyX;
                        double energyY = simCells[xCell + yCell * numCellsX].energyY;

                        // find out vector length for energy.

                        // handle one direction first
                        if (energyX == 0)
                        {
                            if (energyY < 0 && yCell != 0)
                            {
                                if (newCells[getCell(xCell, yCell - 1)].isWall)
                                {
                                    
                                    if (xCell - 1 >= 0)
                                    {
                                        if (!newCells[getCell(xCell - 1, yCell - 1)].isWall) {
                                            newCells[getCell(xCell - 1, yCell - 1)].energyX = energyY * .25; // should be negative
                                            newCells[getCell(xCell - 1, yCell - 1)].energyY = energyY * .25; // should be negative
                                        }
                                        else if (!newCells[getCell(xCell - 1, yCell)].isWall)
                                        {
                                            newCells[getCell(xCell - 1, yCell)].energyX = energyY * .5; // should be negative
                                        }
                                    }
                                    if (xCell + 1 < numCellsX)
                                    {
                                        if (!newCells[getCell(xCell + 1, yCell - 1)].isWall)
                                        {
                                            newCells[getCell(xCell + 1, yCell - 1)].energyX = energyY * -.25; // should be positive
                                            newCells[getCell(xCell + 1, yCell - 1)].energyY = energyY * .25; // should be negative
                                        }
                                        else if (!newCells[getCell(xCell + 1, yCell)].isWall)
                                        {
                                            newCells[getCell(xCell + 1, yCell)].energyX = energyY * -.5; // should be positive
                                        }
                                    }
                                }
                                else
                                {
                                    newCells[xCell + (yCell - 1) * numCellsX].energyY += energyY * decayAmount;
                                }
                            }
                            if (energyY > 0 && yCell != numCellsY - 1)
                            {
                                if (newCells[getCell(xCell, yCell + 1)].isWall)
                                {

                                    if (xCell - 1 >= 0)
                                    {
                                        if (!newCells[getCell(xCell - 1, yCell + 1)].isWall)
                                        {
                                            newCells[getCell(xCell - 1, yCell + 1)].energyX = energyY * -.25; // should be negative
                                            newCells[getCell(xCell - 1, yCell + 1)].energyY = energyY * .25; // should be postive
                                        }
                                        else if (!newCells[getCell(xCell - 1, yCell)].isWall)
                                        {
                                            newCells[getCell(xCell - 1, yCell)].energyX = energyY * -.5; // should be negative
                                        }
                                    }
                                    if (xCell + 1 < numCellsX)
                                    {
                                        if (!newCells[getCell(xCell + 1, yCell + 1)].isWall)
                                        {
                                            newCells[getCell(xCell + 1, yCell + 1)].energyX = energyY * .25; // should be positive
                                            newCells[getCell(xCell + 1, yCell + 1)].energyY = energyY * .25; // should be positive
                                        }
                                        else if (!newCells[getCell(xCell + 1, yCell)].isWall)
                                        {
                                            newCells[getCell(xCell + 1, yCell)].energyX = energyY * .5; // should be positive
                                        }
                                    }
                                }
                                else
                                {
                                    newCells[xCell + (yCell + 1) * numCellsX].energyY += energyY * decayAmount;
                                }
                            }
                        }
                        else if (energyY == 0)
                        {
                            if (energyX < 0 && xCell != 0)
                            {
                                if (newCells[getCell(xCell - 1, yCell)].isWall)
                                {

                                    if (yCell - 1 >= 0)
                                    {
                                        if (!newCells[getCell(xCell - 1, yCell - 1)].isWall)
                                        {
                                            newCells[getCell(xCell - 1, yCell - 1)].energyX = energyX * .25; // should be negative
                                            newCells[getCell(xCell - 1, yCell - 1)].energyY = energyX * .25; // should be negative
                                        }
                                        else if (!newCells[getCell(xCell, yCell - 1)].isWall)
                                        {
                                            newCells[getCell(xCell, yCell - 1)].energyY = energyX * .5; // should be negative
                                        }
                                    }
                                    if (yCell + 1 < numCellsY)
                                    {
                                        if (!newCells[getCell(xCell - 1, yCell + 1)].isWall)
                                        {
                                            newCells[getCell(xCell - 1, yCell + 1)].energyX = energyX * .25; // should be negative
                                            newCells[getCell(xCell - 1, yCell + 1)].energyY = energyX * -.25; // should be positive
                                        }
                                        else if (!newCells[getCell(xCell, yCell + 1)].isWall)
                                        {
                                            newCells[getCell(xCell, yCell + 1)].energyY = energyX * -.5; // should be positive
                                        }
                                    }
                                }
                                else
                                {
                                    newCells[xCell - 1 + yCell * numCellsX].energyX += energyX * decayAmount;
                                }
                            }
                            if (energyX > 0 && xCell != numCellsX - 1)
                            {
                                if (newCells[getCell(xCell + 1, yCell)].isWall)
                                {

                                    if (yCell - 1 >= 0)
                                    {
                                        if (!newCells[getCell(xCell + 1, yCell - 1)].isWall)
                                        {
                                            newCells[getCell(xCell + 1, yCell - 1)].energyX = energyX * .25; // should be positive
                                            newCells[getCell(xCell + 1, yCell - 1)].energyY = energyX * -.25; // should be negative
                                        }
                                        else if (!newCells[getCell(xCell, yCell - 1)].isWall)
                                        {
                                            newCells[getCell(xCell, yCell - 1)].energyY = energyX * -.5; // should be negative
                                        }
                                    }
                                    if (yCell + 1 < numCellsY)
                                    {
                                        if (!newCells[getCell(xCell + 1, yCell + 1)].isWall)
                                        {
                                            newCells[getCell(xCell + 1, yCell + 1)].energyX = energyX * .25; // should be positive
                                            newCells[getCell(xCell + 1, yCell + 1)].energyY = energyX * .25; // should be positive
                                        }
                                        else if (!newCells[getCell(xCell, yCell + 1)].isWall)
                                        {
                                            newCells[getCell(xCell, yCell + 1)].energyY = energyX * .5; // should be positive
                                        }
                                    }
                                }
                                else
                                {
                                    newCells[xCell + 1 + yCell * numCellsX].energyX += energyX * decayAmount;
                                }
                            }
                        }
                        else // handle if it isn't just in one direction. Walls in this case work them selves out as long as you don't add energy to them. 
                        {
                            float rotation = (float)Math.Atan2(energyY, energyX);

                            // Postive Y energy is down.. so when the rotation is in quadrant 1 or 2, have to check if you are at the bottom of the grid. otherwise the top.
                            if (rotation < Math.PI/2 && rotation > 0) // quadrent 1
                            {
                                if(xCell != numCellsX -1 && !newCells[getCell(xCell + 1, yCell)].isWall)
                                    newCells[xCell + 1 + yCell * numCellsX].energyX += energyX * decayAmount * tangentDecay;
                                if (yCell != numCellsY - 1 && !newCells[getCell(xCell, yCell + 1)].isWall)
                                    newCells[xCell + (yCell + 1) * numCellsX].energyY += energyY * decayAmount * tangentDecay;

                                if (xCell != numCellsX - 1 && yCell != numCellsY - 1 && !newCells[getCell(xCell + 1, yCell + 1)].isWall)
                                {
                                    newCells[xCell + 1 + (yCell + 1) * numCellsX].energyY += energyY * decayAmount;
                                    newCells[xCell + 1 + (yCell + 1) * numCellsX].energyX += energyX * decayAmount;
                                }
                            }
                            else if (rotation < Math.PI && rotation > Math.PI / 2) // quadrant 2
                            {
                                if(xCell != 0 && !newCells[getCell(xCell - 1, yCell)].isWall)
                                    newCells[xCell - 1 + yCell * numCellsX].energyX += energyX * decayAmount * tangentDecay;
                                if (yCell != numCellsY - 1 && !newCells[getCell(xCell,yCell+1)].isWall)
                                    newCells[xCell + (yCell + 1) * numCellsX].energyY += energyY * decayAmount * tangentDecay;

                                if (xCell != 0 && yCell != numCellsY - 1 && !newCells[getCell(xCell - 1, yCell + 1)].isWall)
                                {
                                    newCells[xCell - 1 + (yCell + 1) * numCellsX].energyY += energyY * decayAmount;
                                    newCells[xCell - 1 + (yCell + 1) * numCellsX].energyX += energyX * decayAmount;
                                }
                            }
                            //else if (rotation < 3 * Math.PI / 2 && rotation > Math.PI) // quadrant 3
                            else if (rotation < -1 * (Math.PI / 2) && rotation > -1 * Math.PI)
                            {
                                if (xCell != 0 && !newCells[getCell(xCell - 1, yCell)].isWall)
                                    newCells[xCell - 1 + yCell * numCellsX].energyX += energyX * decayAmount * tangentDecay;
                                if (yCell != 0 && !newCells[getCell(xCell, yCell - 1)].isWall)
                                    newCells[xCell + (yCell - 1) * numCellsX].energyY += energyY * decayAmount * tangentDecay;

                                if (xCell != 0 && yCell != 0 && !newCells[getCell(xCell - 1, yCell - 1)].isWall)
                                {
                                    newCells[xCell - 1 + (yCell - 1) * numCellsX].energyY += energyY * decayAmount;
                                    newCells[xCell - 1 + (yCell - 1) * numCellsX].energyX += energyX * decayAmount;
                                }
                            }
                            //else if (rotation < 2 * Math.PI && rotation > 3 * Math.PI / 2 ) // quadrant 4
                            else if (rotation < 0 && rotation > -1 * ( Math.PI / 2 ) )
                            {
                                if (xCell != numCellsX - 1 && !newCells[getCell(xCell + 1, yCell)].isWall)
                                    newCells[xCell + 1 + yCell * numCellsX].energyX += energyX * decayAmount * tangentDecay;
                                if (yCell != 0 && !newCells[getCell(xCell, yCell - 1)].isWall)
                                    newCells[xCell + (yCell - 1) * numCellsX].energyY += energyY * decayAmount * tangentDecay;

                                if (xCell != numCellsX - 1 && yCell != 0 && !newCells[getCell(xCell + 1, yCell - 1)].isWall)
                                {
                                    newCells[xCell + 1 + (yCell - 1) * numCellsX].energyY += energyY * decayAmount;
                                    newCells[xCell + 1 + (yCell - 1) * numCellsX].energyX += energyX * decayAmount;
                                }
                            }
                        }
                        
                    }// end if any energry != 0
               }
            }
                
            // copy energies back.
            for (int i = 0; i < newCells.Length; i++)
            {
                simCells[i].energyX = newCells[i].energyX;
                simCells[i].energyY = newCells[i].energyY;
            }
        }

        internal void applyEffectors(IDictionary<int, Effector> effectors)
        {
            foreach (Effector eff in effectors.Values)
            {
                int cellNum = getSimCellNum(eff.xPos, eff.yPos);
                if (!simCells[cellNum].isWall)
                {
                    simCells[cellNum].energyX = eff.energyX;
                    simCells[cellNum].energyY = eff.energyY;
                }
            }
        }

        internal void UpdateParticles(List<Particle> stream)
        {
            foreach (Particle p in stream)
            {
                int cellNum = getSimCellNum(p.X, p.Y);
                p.setVelocity((int)(p.xVelocity + simCells[cellNum].energyX), (int)(p.yVelocity + simCells[cellNum].energyY));
                int[] nextPos = p.nextPosition();
                if (nextPos[0] > 0 && nextPos[0] < numCellsX * cellPixelsX && nextPos[1] > 0 && nextPos[1] < numCellsY * cellPixelsY)
                {
                    cellNum = getSimCellNum(nextPos[0], nextPos[1]);
                    if (simCells[cellNum].isWall)
                        p.setVelocity(0, 0);
                }
            }
        }
    }
}

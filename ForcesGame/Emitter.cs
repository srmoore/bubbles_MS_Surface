using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ForcesGame
{
    class Emitter
    {
        private int xPos;
        private int yPos;
        private int xVelocity = 0;
        private int yVelocity = 0;
        private int radius = 0;
        private long ticksPerParticle = 0;
        Random r = new Random();
        long lastEmitCount = 0;
        long lastEmitCheck = 0;

        public void setParticlesPerSecond(int pps) 
        {
            long oneSec = new TimeSpan(0,0,1).Ticks;
            ticksPerParticle = oneSec / pps;
        }

        public void move(int x, int y)
        {
            xVelocity = x - xPos;
            yVelocity = y - yPos;
            xPos = x;
            yPos = y;
        }

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

        public void setRadius(int rad)
        {
            radius = rad;
        }

        internal void emit(List<Particle> parts, TimeSpan currentTime){
            lastEmitCheck += currentTime.Ticks;
            long ticksPassed = lastEmitCheck - lastEmitCount;
            if(ticksPassed > ticksPerParticle) {
                int numPartsToEmit = (int)(ticksPassed / ticksPerParticle);
                for (int i = 0; i < numPartsToEmit; i++)
                {
                    Particle part = new Particle();
                    //part.setVelocity(xVelocity + r.Next((int)(xVelocity * .25), (int)(xVelocity * 1.25)), yVelocity + r.Next((int)(yVelocity * .25), (int)(yVelocity * 1.25)));
                    part.setVelocity(xVelocity, yVelocity);
                    int pX = xPos + r.Next(-radius, radius);
                    int pY = yPos + r.Next(-radius, radius);
                    part.setPos(pX, pY);
                    parts.Add(part);
                }
                lastEmitCount += ticksPerParticle * numPartsToEmit;
            }
        }

        internal void emit(List<Particle> parts, int p)
        {
            for (int i = 0; i < p; i++)
            {
                Particle part = new Particle();
                //part.setVelocity(xVelocity + r.Next((int)(xVelocity * .25), (int)(xVelocity * 1.25)), yVelocity + r.Next((int)(yVelocity * .25), (int)(yVelocity * 1.25)));
                part.setVelocity(xVelocity, yVelocity);
                int pX = xPos + r.Next(-radius, radius);
                int pY = yPos + r.Next(-radius, radius);
                part.setPos(pX, pY);
                parts.Add(part);
            }
        }
    }
}

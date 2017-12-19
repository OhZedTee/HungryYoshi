using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HungryYoshi.Models.Particle_System
{
    public class ParticleEngine
    {
        //Engine Properties
        private Random random;
        public Vector2 EmitterLocation { get; set; }
        private List<Particle> particles;
        private List<Texture2D> textures;
        public Color PresetColor { get; set; }
        public bool SpecificColor { get; set; }
        public bool Fountain { get; set; }
        public bool Generate {get; set;}
        public float Angle { get; set; }


        Vector2 angleRange;

        public ParticleEngine(List<Texture2D> textures, Vector2 location)
        {
            EmitterLocation = location;
            this.textures = textures;
            this.particles = new List<Particle>();
            random = new Random();
        }

        /// <summary>
        /// Create new particles based on predetermined properties and the use of random generators
        /// </summary>
        /// <returns></returns>
        private Particle GenerateNewParticle()
        {
            //Determine the easy statistics
            Texture2D texture = textures[random.Next(textures.Count)];
            Vector2 position = EmitterLocation;
            Vector2 velocity;
            float angularVelocity;
            float size;
            int time;

            //Create a range for degrees
            angleRange.X = MathHelper.ToDegrees(Angle) - 15;
            angleRange.Y = MathHelper.ToDegrees(Angle) + 15;

            //Pick a randpom angle betweeen the angle ranges to be as the permanent angle
            float adjustedAngle = MathHelper.ToRadians(random.Next((int)angleRange.X, (int)angleRange.Y));

            //Calculate other variables using random generators. 
            if (!Fountain)
            {
                velocity = new Vector2(
                        1f * (float)(random.NextDouble() * 2 - 1),
                        1f * (float)(random.NextDouble() * 2 - 1));
                angularVelocity = 0.1f * (float)(random.NextDouble() * 2 - 1);
                size = (float)random.NextDouble();
                time = 20 + random.Next(40);

            }

            else
            {
                float scalarVel = 2 * (float)(random.NextDouble());
                velocity.X = scalarVel * (float)Math.Cos(adjustedAngle);
                velocity.Y = scalarVel * (float)Math.Sin(adjustedAngle);
                angularVelocity = 0;
                size = (float)random.NextDouble() * 0.3f;
                time = 10 + random.Next(20);
            }

            //If a specific color was not choosen,
            if (!SpecificColor)
            {
                //Create a random color
                PresetColor = new Color(
                        (float)random.NextDouble(),
                        (float)random.NextDouble(),
                        (float)random.NextDouble());
            }

            //Create the new particles
            return new Particle(texture, position, velocity, MathHelper.ToDegrees(adjustedAngle), angularVelocity, PresetColor, size, time);
        }

        /// <summary>
        /// Update the number of particles created at every update.
        /// </summary>
        public void Update()
        {
            //Generate new particles by adding a specific amount every update
            if (Generate)
            {
                int total = 2;

                if (Fountain)
                {
                    total = 1;
                }

                for (int i = 0; i < total; i++)
                {
                    particles.Add(GenerateNewParticle());
                }
            }

            //Remove particles if they are alive but their time to live is at 0
            for (int particle = 0; particle < particles.Count; particle++)
            {
                particles[particle].Update();
                if (particles[particle].Time <= 0)
                {
                    particles.RemoveAt(particle);
                    particle--;
                }
            }
        }

        /// <summary>
        /// Draw the particles onto the screen from the location provided
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            for (int index = 0; index < particles.Count; index++)
            {
                //Draw the particle
                particles[index].Draw(spriteBatch);
            }
        }
    }
}
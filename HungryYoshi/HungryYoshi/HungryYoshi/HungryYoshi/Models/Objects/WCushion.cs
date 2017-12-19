using HungryYoshi.Models.Particle_System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HungryYoshi.Models.Objects
{
    class WCushion : MapTile
    {
        //Whoopee Cushion Properties
        public float Angle { get; set; }
        private Vector2 acceleration;
        private Vector2 fieldOfView;

        //Objects
        private ParticleEngine particleEngine;
        private Player player;

        public WCushion(Vector2 pos, ContentManager content, float angle, Player player)
            : base(pos, content, "Whoopee Cushion")
        {
            Angle = angle;
            this.player = player;

            //Set the angle to the positive equivalent if it's negative
            if (angle < 0)
            {
                //CHECK mod, not add (-4000 degrees)
                Angle = 360 + Angle;
            }

            fieldOfView = new Vector2(Angle + 15, Angle - 15);

            //Set the acceleration based on angle
            acceleration.X = 15f / (float)Driver.REFRESH_RATE;
            if (Angle < 90 || Angle == 360)
            {
                acceleration.Y = acceleration.X * (float)Math.Tan(MathHelper.ToRadians(Angle));
                acceleration.Y = -acceleration.Y;
            }
            else if (Angle >= 90 && Angle < 180)
            {
                acceleration.Y = acceleration.X * (float)Math.Tan(MathHelper.ToRadians(Angle - 90));
            }
            else if (Angle >= 180 && Angle < 270)
            {
                acceleration.Y = acceleration.X * (float)Math.Tan(MathHelper.ToRadians(Angle - 180));
                acceleration.X = -acceleration.X;
            }
            else if (Angle >= 270 && Angle < 360)
            {
                acceleration.X = -acceleration.X;
                acceleration.Y = acceleration.X * (float)Math.Tan(MathHelper.ToRadians(Angle - 270));
            }

            //Rotate the corners of the tile
            for (int i = 0; i < base.GetCorners.Length; ++i)
            {
                //Create pivot point and move the corners back by the pivot
                Vector2 pivot = new Vector2(base.GetSprite.GetBounds.X, base.GetSprite.GetBounds.Y);
                base.SetCorner(i, new Vector2(base.GetCorner(i).X - pivot.X, base.GetCorner(i).Y));
                base.SetCorner(i, new Vector2(base.GetCorner(i).X, base.GetCorner(i).Y - pivot.Y));

                //Rotate the corners
                Vector2 newCorner;
                newCorner.X = (float)(base.GetCorner(i).X * Math.Cos(MathHelper.ToRadians(angle)) - base.GetCorner(i).Y * Math.Sin(MathHelper.ToRadians(angle)));
                newCorner.Y = (float)(base.GetCorner(i).X * Math.Sin(MathHelper.ToRadians(angle)) + base.GetCorner(i).Y * Math.Cos(MathHelper.ToRadians(angle)));

                //Reset the corners to the new rotated location and add the pivot back
                base.SetCorner(i, new Vector2(newCorner.X + pivot.X, base.GetCorner(i).Y));
                base.SetCorner(i, new Vector2(base.GetCorner(i).X, newCorner.Y + pivot.Y));
            }

            //Set the particle engine properties
            List<Texture2D> textures = new List<Texture2D>();
            textures.Add(content.Load<Texture2D>("Smoke"));
            particleEngine = new ParticleEngine(textures, base.GetSprite.GetPosition);
            particleEngine.EmitterLocation = new Vector2((base.GetCorner((int)MapTile.Corners.topRight).X + base.GetCorner((int)MapTile.Corners.bottomRight).X) * 0.5f, (base.GetCorner((int)MapTile.Corners.topRight).Y + base.GetCorner((int)MapTile.Corners.bottomRight).Y) * 0.5f);
            particleEngine.SpecificColor = true;
            particleEngine.PresetColor = Color.White;
            particleEngine.Fountain = true;
            particleEngine.Generate = true;
            particleEngine.Angle = MathHelper.ToRadians(angle);

        }

        /// <summary>
        /// Property to set the player
        /// </summary>
        public Player SetPlayer
        {
            set { player = value; }
        }

        /// <summary>
        /// Update the particle engine and the update the player if its in the airfield
        /// </summary>
        public override void Update()
        {
            //Update the engine
            particleEngine.Update();

            //Check for collision of player and airfield
            if (CheckCollision(player.GetSprite.GetPosition, base.GetSprite.GetPosition))
            {
                //Accelerate the player if in a bubble
                if (player.InBubble)
                {
                    player.Trajectory += acceleration;
                }
            }

            base.Update();
        }

        /// <summary>
        /// Checks if the player is within the Whoopee Cushion airfield
        /// </summary>
        /// <param name="a">A rectangle representing the player's boundaries</param>
        /// <param name="b">An array of points representing the rectangular airfield</param>
        /// <returns>A boolean value representing a collision</returns>
        public bool CheckCollision(Vector2 loc, Vector2 dest)
        {
            //Calculate the player's current angle from the whoopee cushion
            Vector2 displacement = loc - dest;
            float angle = MathHelper.ToDegrees((float)Math.Atan2(displacement.Y, displacement.X));

            //Make sure it's a positive ange
            if (angle < 0)
            {
                angle = 360 + angle;
            }

            //Make sure the angle randomly generated in within the range
            if (angle < fieldOfView.X && angle > fieldOfView.Y)
            {
                //Calculate the distance squared two objects
                float distance = Vector2.DistanceSquared(dest, loc);

                //If the player is less than 150 pixels away rom the cushion, there is a colision
                if (distance < Math.Pow(150, 2))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Draw the Whoopee Cushion to the screen
        /// </summary>
        /// <param name="sb">The SpriteBatch that is used to draw the textures</param>
        public override void Draw(SpriteBatch sb)
        {
            //Call draw rotation to draw the sprite with an angle
            DrawRotation(sb, Angle);
            particleEngine.Draw(sb);
        }

        /// <summary>
        /// Draw the tile to the screen with a rotation angle
        /// </summary>
        /// <param name="sb">The SpriteBatch used to draw the textures</param>
        /// <param name="angle">The angle at which the tile is rotated</param>
        public override void DrawRotation(SpriteBatch sb, float angle)
        {
            base.GetSprite.DrawRotation(sb, Color.White, angle, new Vector2(base.GetSprite.GetBounds.Width * 0.5f, base.GetSprite.GetBounds.Height - 16));
        }
    }
}
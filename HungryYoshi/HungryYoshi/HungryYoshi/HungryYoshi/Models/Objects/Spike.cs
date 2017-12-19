using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HungryYoshi.Models.Objects
{
    class Spike: MapTile
    {
        //Spike properties
        public float Angle { get; set; }
        public bool HitSide {get; set;}
        public bool HitSpike {get; set;}

        //Midpoint between 2 corners where the player can die
        Vector2 spikeMidPoint;

        public Spike(Vector2 pos, ContentManager content, float angle) : base(pos, content, "Spike")
        {
            Angle = angle;

            //Create midpoint based on preset corners
            spikeMidPoint.X = (base.GetCorner((int)MapTile.Corners.topLeft).X + base.GetCorner((int)MapTile.Corners.topRight).X) * 0.5f;
            spikeMidPoint.Y = (base.GetCorner((int)MapTile.Corners.topLeft).Y + base.GetCorner((int)MapTile.Corners.topRight).Y) * 0.5f;

            //Rotate the corners of the tile
            for (int i = 0; i < base.GetCorners.Length; ++i)
            {
                //Create pivot point and move the points back by the pivot
                Vector2 pivot = new Vector2(base.GetSprite.GetBounds.X, base.GetSprite.GetBounds.Y);
                base.SetCorner(i, new Vector2(base.GetCorner(i).X - pivot.X, base.GetCorner(i).Y));
                base.SetCorner(i, new Vector2(base.GetCorner(i).X, base.GetCorner(i).Y - pivot.Y));

                //Rotate the points
                Vector2 newCorner;
                newCorner.X = (float)(base.GetCorner(i).X * Math.Cos(MathHelper.ToRadians(angle)) - base.GetCorner(i).Y * Math.Sin(MathHelper.ToRadians(angle)));
                newCorner.Y = (float)(base.GetCorner(i).X * Math.Sin(MathHelper.ToRadians(angle)) + base.GetCorner(i).Y * Math.Cos(MathHelper.ToRadians(angle)));

                //Reset the points to the new rotated location and add the pivot back
                base.SetCorner(i, new Vector2(newCorner.X + pivot.X, base.GetCorner(i).Y));
                base.SetCorner(i, new Vector2(base.GetCorner(i).X, newCorner.Y + pivot.Y));
            }
            //Rotate the mdpoints of the tile
            //Create pivot point and move the midpoint back by the pivot
            Vector2 origin = new Vector2(base.GetSprite.GetBounds.X, base.GetSprite.GetBounds.Y);
            spikeMidPoint.X = spikeMidPoint.X - origin.X;
            spikeMidPoint.Y = spikeMidPoint.Y - origin.Y;

            //Rotate the midpoint
            Vector2 rotatedMidPoint;
            rotatedMidPoint.X = (float)(spikeMidPoint.X * Math.Cos(MathHelper.ToRadians(angle)) - spikeMidPoint.Y * Math.Sin(MathHelper.ToRadians(angle)));
            rotatedMidPoint.Y = (float)(spikeMidPoint.X * Math.Sin(MathHelper.ToRadians(angle)) + spikeMidPoint.Y * Math.Cos(MathHelper.ToRadians(angle)));

            //Reset the midpoint to the new rotated location and add the pivot back
            spikeMidPoint.X = rotatedMidPoint.X + origin.X;
            spikeMidPoint.Y = rotatedMidPoint.Y + origin.Y;
        }

        /// <summary>
        /// Property to get the midpoint position
        /// </summary>
        public Vector2 GetMidpoint
        {
            get { return spikeMidPoint; }
        }

        /// <summary>
        /// Changes the player's trajectory based on what side of the spike was hit. If it hits the side, it stops its X movement and
        /// only gravity applies on the Y. If it hits the top, it dies.
        /// </summary>
        /// <param name="player">An object which represent the player in the game</param>
        public void ChangePlayerTraj(Player player)
        {
            //Check to see if the player hit the side of the spike or not
            if (!HitSide)
            {
                //If the spike has not been hit yet
                if (!HitSpike)
                {
                    //Reset the player
                    Player.gameState = Player.States.reset;
                    HitSpike = true;
                }
            }
            else
            {
                //If the spike has not been hit yet
                if (!HitSpike)
                {
                    //Set its velocity to 0 so that only gravity affects the player. (Player wont have X movement any longer)
                    player.Velocity = 0;

                    //Reset the player location and shoot it
                    player.GetSprite.SetPosition("Y", player.GetSprite.GetPosition.Y + 2);
                    Player.gameState = Player.States.shooting;
                    HitSpike = true;
                }
            }
        }

        /// <summary>
        /// Draw the spike to the screen
        /// </summary>
        /// <param name="sb">The SpriteBatch that is used to draw the textures</param>
        public override void Draw(SpriteBatch sb)
        {
            //Call draw rotation to draw the sprite with an angle
            DrawRotation(sb, Angle);
        }

        /// <summary>
        /// Draw the tile to the screen with a rotation angle
        /// </summary>
        /// <param name="sb">The SpriteBatch used to draw the textures</param>
        /// <param name="angle">The angle at which the tile is rotated</param>
        public override void DrawRotation(SpriteBatch sb, float angle)
        {
            base.DrawRotation(sb, angle);
        }
    }
}
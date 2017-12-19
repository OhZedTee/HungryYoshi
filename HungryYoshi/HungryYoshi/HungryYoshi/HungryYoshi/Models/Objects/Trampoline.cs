using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HungryYoshi.Models.Objects
{
    class Trampoline : MapTile
    {
        //Properties
        public float Angle { get; set; }
        private const float SLOWDOWN = 10 / Driver.REFRESH_RATE;
        public bool HitSide {get; set;}
        public bool HitTramp {get; set;}

        //Midpoint between 2 corners where the player can jump from
        Vector2 trampMidPoint;
        
        public Trampoline(Vector2 pos, ContentManager content, float angle):base(pos, content, "Trampoline")
        {
            Angle = angle;
            
            //Reset the corners already made by the maptiles to fit the trampoline
            base.SetCorner((int)MapTile.Corners.topLeft, new Vector2(base.GetSprite.GetBounds.Left - (base.GetSprite.GetBounds.Width / 4) , base.GetSprite.GetBounds.Top - (base.GetSprite.GetBounds.Height / 3)));
            base.SetCorner((int)MapTile.Corners.topRight, new Vector2(base.GetSprite.GetBounds.Right - (base.GetSprite.GetBounds.Width * 0.5f + base.GetSprite.GetBounds.Width / 4), base.GetSprite.GetBounds.Top - (base.GetSprite.GetBounds.Height / 3)));
            base.SetCorner((int)MapTile.Corners.bottomLeft, new Vector2(base.GetSprite.GetBounds.Left - (base.GetSprite.GetBounds.Width / 4), base.GetSprite.GetBounds.Bottom - (base.GetSprite.GetBounds.Height * 0.5f + base.GetSprite.GetBounds.Height / 5)));
            base.SetCorner((int)MapTile.Corners.bottomRight, new Vector2(base.GetSprite.GetBounds.Right - (base.GetSprite.GetBounds.Width * 0.5f + base.GetSprite.GetBounds.Width / 4), base.GetSprite.GetBounds.Bottom - (base.GetSprite.GetBounds.Height * 0.5f + base.GetSprite.GetBounds.Height / 5)));
            trampMidPoint.X = (base.GetCorner((int)MapTile.Corners.topLeft).X + base.GetCorner((int)MapTile.Corners.topRight).X) * 0.5f;
            trampMidPoint.Y = (base.GetCorner((int)MapTile.Corners.topLeft).Y + base.GetCorner((int)MapTile.Corners.topRight).Y) * 0.5f;

            //Rotate corners of the tile
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
            //Rotate midpoint of the tile
            //Create pivot point and move the midpoint back by the pivot
            Vector2 origin = new Vector2(base.GetSprite.GetBounds.X, base.GetSprite.GetBounds.Y);
            trampMidPoint.X = trampMidPoint.X - origin.X;
            trampMidPoint.Y = trampMidPoint.Y - origin.Y;

            //Rotate the midpoint
            Vector2 rotatedMidPoint;
            rotatedMidPoint.X = (float)(trampMidPoint.X * Math.Cos(MathHelper.ToRadians(angle)) - trampMidPoint.Y * Math.Sin(MathHelper.ToRadians(angle)));
            rotatedMidPoint.Y = (float)(trampMidPoint.X * Math.Sin(MathHelper.ToRadians(angle)) + trampMidPoint.Y * Math.Cos(MathHelper.ToRadians(angle)));

            //Reset the midpoint to the new rotated location and add the pivot back
            trampMidPoint.X = rotatedMidPoint.X + origin.X;
            trampMidPoint.Y = rotatedMidPoint.Y + origin.Y;
        }

        /// <summary>
        /// Property to get the midpoint position
        /// </summary>
        public Vector2 GetMidpoint
        {
            get { return trampMidPoint; }
        }

        /// <summary>
        /// Changes the player's trajectory based on what side of the trampoline was hit. If it hits the side, it stops its X movement and
        /// only gravity applies on the Y. If it hits the top, it bounces back in the opposite direction.
        /// </summary>
        /// <param name="player">An object which represent the player in the game</param>
        public void ChangePlayerTraj(Player player)
        {
            //Check to see if the player hit the side of the trampoline or not
            if (!HitSide)
            {
                //If the trampoline has not been hit yet
                if (!HitTramp)
                {
                    //Bounce the player off in the opposite direction
                    player.Direction *= -1;
                    player.Velocity -= SLOWDOWN;
                    player.Angle = player.CalcAngle(base.GetSprite.GetPosition, player.GetSprite.GetPosition);
                    Player.gameState = Player.States.shooting;
                    HitTramp = true;
                }
            }
            else if(!HitTramp)
            {
                //Set its velocity to 0 so that only gravity affects the player. (Player wont have X movement any longer)
                player.Velocity = 0;

                //Reset the player location and shoot it
                player.GetSprite.SetPosition("Y", player.GetSprite.GetPosition.Y + 2);
                Player.gameState = Player.States.shooting;
                HitTramp = true;
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
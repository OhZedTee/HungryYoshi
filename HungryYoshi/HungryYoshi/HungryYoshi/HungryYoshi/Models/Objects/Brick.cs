using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HungryYoshi.Models.Objects
{
    class Brick : MapTile
    {
        //Midpoint between every corner
        Vector2[] brickMidPoints = new Vector2[4];

        public Brick(Vector2 pos, ContentManager content) : base(pos, content, "Brick")
        {
            //Create midpoints based on the corners
            brickMidPoints[0].X = (base.GetCorner((int)MapTile.Corners.topLeft).X + base.GetCorner((int)MapTile.Corners.topRight).X) * 0.5f + (base.GetSprite.GetBounds.Width * 0.5f);
            brickMidPoints[0].Y = (base.GetCorner((int)MapTile.Corners.topLeft).Y + base.GetCorner((int)MapTile.Corners.topRight).Y) * 0.5f + (base.GetSprite.GetBounds.Height * 0.5f);
            brickMidPoints[1].X = (base.GetCorner((int)MapTile.Corners.topRight).X + base.GetCorner((int)MapTile.Corners.bottomRight).X) * 0.5f + (base.GetSprite.GetBounds.Width * 0.5f);
            brickMidPoints[1].Y = (base.GetCorner((int)MapTile.Corners.topRight).Y + base.GetCorner((int)MapTile.Corners.bottomRight).Y) * 0.5f + (base.GetSprite.GetBounds.Width * 0.5f);
            brickMidPoints[2].X = (base.GetCorner((int)MapTile.Corners.bottomRight).X + base.GetCorner((int)MapTile.Corners.bottomLeft).X) * 0.5f + (base.GetSprite.GetBounds.Width * 0.5f);
            brickMidPoints[2].Y = (base.GetCorner((int)MapTile.Corners.bottomRight).Y + base.GetCorner((int)MapTile.Corners.bottomLeft).Y) * 0.5f + (base.GetSprite.GetBounds.Width * 0.5f);
            brickMidPoints[3].X = (base.GetCorner((int)MapTile.Corners.bottomLeft).X + base.GetCorner((int)MapTile.Corners.topLeft).X) * 0.5f + (base.GetSprite.GetBounds.Width * 0.5f);
            brickMidPoints[3].Y = (base.GetCorner((int)MapTile.Corners.bottomLeft).Y + base.GetCorner((int)MapTile.Corners.topLeft).Y) * 0.5f + (base.GetSprite.GetBounds.Width * 0.5f);
        }

        /// <summary>
        /// Property to get the midpoint positions
        /// </summary>
        public Vector2[] GetMidpoints
        {
            get { return brickMidPoints; }
        }

        public void ChangePlayerTraj(string face, Player player)
        {
            //Change the player's trajectory and location based on the side of the brick that was hit
            if(face == "top")
            {
                //Set the player ontop of the brick
                player.GetSprite.SetPosition("Y", brickMidPoints[0].Y - (player.GetSprite.GetBounds.Height * 0.5f) + 4);

                //Only remove the player's Y trajectory if he is not in a bubble
                if (!player.InBubble)
                {
                    player.Trajectory = new Vector2(player.Trajectory.X, 0);
                }
            }

            else if (face == "bottom")
            {
                //Make the player bounce off of the brick
                player.HitTop = true;
                player.CancelGravity = false;
                player.Trajectory = new Vector2(player.Trajectory.X, 0);
            }

            else if (face == "left")
            {
                //Remove the player's X trajectory and displace the player to the left of the brick
                player.GetSprite.SetPosition("X", brickMidPoints[3].X - (player.GetSprite.GetBounds.Width * 0.5f) + 4);
                player.CancelGravity = false;
                player.Trajectory = new Vector2(0, player.Trajectory.Y);
            }

            else if (face == "right")
            {
                //Remove the player's X trajectory and dispace the player to the right of the brick
                player.GetSprite.SetPosition("X", brickMidPoints[1].X + (base.GetSprite.GetBounds.Width * 0.5f) - 4);
                player.CancelGravity = false;
                player.Trajectory = new Vector2(0, player.Trajectory.Y);
            }
        }

        /// <summary>
        /// Draw the tile to the screen
        /// </summary>
        /// <param name="sb">The SpriteBatch used to draw the textures</param>
        public override void Draw(SpriteBatch sb)
        {
            //Draw the maptile (brick) to the screen
            base.Draw(sb);
        }
    }
}
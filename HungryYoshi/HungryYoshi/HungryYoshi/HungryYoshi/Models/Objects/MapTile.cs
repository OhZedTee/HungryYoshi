using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HungryYoshi.Models.Objects
{
    class MapTile
    {
        //Objects
        Sprite sprite;

        //Properties
        int radius;

        //Collision Detection Properties
        Vector2[] corners = new Vector2[4];
        public enum Corners
        {
            topLeft,
            topRight,
            bottomLeft,
            bottomRight
        };

        public MapTile(Vector2 pos, ContentManager content, string tileType)
        {
            //Create the maptile sprite
            Texture2D texture = content.Load<Texture2D>(tileType);
            sprite = new Sprite(new Rectangle(Convert.ToInt32(pos.X), Convert.ToInt32(pos.Y), texture.Width, texture.Height), texture);            

            //Set largest radius possible on the tile
            if (sprite.GetBounds.Width > sprite.GetBounds.Height)
            {
                radius = sprite.GetBounds.Width + 2;
            }
            else
            {
                radius = sprite.GetBounds.Height + 2;
            }

            //Calculate the corner position of each tile
            corners[(int)Corners.topLeft] = new Vector2(sprite.GetBounds.Left - (sprite.GetBounds.Width * 0.5f), sprite.GetBounds.Top - (sprite.GetBounds.Height * 0.5f));
            corners[(int)Corners.topRight] = new Vector2(sprite.GetBounds.Right - (sprite.GetBounds.Width * 0.5f), sprite.GetBounds.Top - (sprite.GetBounds.Height * 0.5f));
            corners[(int)Corners.bottomLeft] = new Vector2(sprite.GetBounds.Left - (sprite.GetBounds.Width * 0.5f), sprite.GetBounds.Bottom - (sprite.GetBounds.Height * 0.5f));
            corners[(int)Corners.bottomRight] = new Vector2(sprite.GetBounds.Right - (sprite.GetBounds.Width * 0.5f), sprite.GetBounds.Bottom - (sprite.GetBounds.Height * 0.5f));            
        }

        /// <summary>
        /// Property to retrieve the sprite of the maptile
        /// </summary>
        public Sprite GetSprite
        {
            get { return sprite; }
        }

        /// <summary>
        /// Property to get the corners of the maptile
        /// </summary>
        public Vector2[] GetCorners
        {
            get { return corners; }
        }

        /// <summary>
        /// Changes the position of a specific corner
        /// </summary>
        /// <param name="index">An integer value representing the corner to be changed</param>
        /// <param name="value">The new Vector2 position of the corner</param>
        public void SetCorner(int index, Vector2 value)
        {
            corners[index] = value;
        }

        /// <summary>
        /// Retrieve the position of a specific corner
        /// </summary>
        /// <param name="index">An integer value representing the corner whose position is needed</param>
        /// <returns>Returns the position of the corner specified</returns>
        public Vector2 GetCorner(int index)
        {
            return corners[index];
        }

        /// <summary>
        /// Check to see if the tile is close the player: If two circles surrounding each object intersect.
        /// </summary>
        /// <param name="player">The player object that is being checked for a collision with</param>
        /// <returns>Returns a boolean, whether or not the circles collide</returns>
        public bool CircleCollision(Player player)
        {
            //Calculate the distance squared and the total radius squared between the two objects
            float distance = Vector2.DistanceSquared(sprite.GetPosition, player.GetSprite.GetPosition);
            double totalRadius = Math.Pow(radius,2) + Math.Pow(player.GetRadius, 2);

            //Check to see for a collision
            if (distance < totalRadius)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Call the updates in each child class
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// Draw the tile to the screen
        /// </summary>
        /// <param name="sb">The SpriteBatch used to draw the textures</param>
        public virtual void Draw(SpriteBatch sb)
        {
            sprite.Draw(sb);
        }

        /// <summary>
        /// Draw the tile to the screen with a rotation angle
        /// </summary>
        /// <param name="sb">The SpriteBatch used to draw the textures</param>
        /// <param name="angle">The angle at which the tile is rotated</param>
        public virtual void DrawRotation(SpriteBatch sb, float angle)
        {
            sprite.DrawRotation(sb, Color.White, angle, new Vector2(sprite.GetBounds.Width * 0.5f, sprite.GetBounds.Height * 0.5f));
        }
    }
}
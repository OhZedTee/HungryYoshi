using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HungryYoshi.Models
{
    class Sprite
    {
        //Location of Sprite
        private Rectangle bounds;
        private Vector2 pos;

        //Texture
        private Texture2D texture;

        //Stored angle for drawing sprite with rotation
        private float prevAngle = 0;

        public Sprite(Rectangle bounds, Texture2D texture)
        {
            this.bounds = bounds;
            pos = new Vector2(bounds.X, bounds.Y);
            this.texture = texture;
        }

        /// <summary>
        /// Property to get the boundary of the sprite
        /// </summary>
        public Rectangle GetBounds
        {
            get { return bounds; }
        }

        /// <summary>
        /// Property to get the position of the sprite
        /// </summary>
        public Vector2 GetPosition
        {
            get { return pos; }
        }

        /// <summary>
        /// Sets or Retrieves the texture of the sprite
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        /// <summary>
        /// Procedure that increments the position of the sprite and updates the boundary based on the direction given
        /// </summary>
        /// <param name="direction">The axis coordinate which is to be modified</param>
        /// <param name="value">The value at which the direction is set to</param>
        public void SetPosition(string direction, double value)
        {
            //The direction to be modified on the coordinate grid
            if (direction == "X")
            {
                //Set the position to the value
                pos.X = (float)value;
                bounds.X = Convert.ToInt32(pos.X);
            }
            else if (direction == "Y")
            {
                //Set the position to the value
                pos.Y = (float)value;
                bounds.Y = Convert.ToInt32(pos.Y);
            }
        }

        /// <summary>
        /// Procedure to set the position of the sprite to a new position
        /// </summary>
        /// <param name="newPos">A vector2 position that the sprite is moved to</param>
        public void SetPosition(Vector2 newPos)
        {
                    //Set the position to the new position
                    pos = newPos;
                    bounds.X = Convert.ToInt32(pos.X);
                    bounds.Y = Convert.ToInt32(pos.Y);
        }

        /// <summary>
        /// Procedure that changes the boundary of the sprite and the position of the sprite
        /// </summary>
        /// <param name="property">The property of the rectangle which is to be modified</param>
        /// <param name="value">The value at which the boundary is set to</param>
        public void SetBounds(string property, int value)
        {
            //The property to be modified on the coordinate grid
            if (property == "X")
            {
                //Set the position to the value
                bounds.X = value;
                pos.X = value;
            }
            else if (property == "width")
            {
                //Set the new width
                bounds.Width = value;
            }
            else if (property == "height")
            {
                //Set the new height
                bounds.Height = value;
            }
            else
            {
                //Set the position to the value
                bounds.Y = value;
                pos.Y = value;
            }
        }

        /// <summary>
        /// Draw the sprite with no special presets
        /// </summary>
        /// <param name="sb">SpriteBatch used to draw textures</param>
        public void Draw(SpriteBatch sb)
        {
            //Draw the sprite normally
            sb.Draw(texture, bounds, Color.White);
        }

        /// <summary>
        /// Draw the sprite with a given color and transparency
        /// </summary>
        /// <param name="sb">SpriteBatch used to draw textures</param>
        /// <param name="color">Color that the sprite will be drawn with</param>
        /// <param name="opacity">Transparency of Image</param>
        public void Draw(SpriteBatch sb, Color color, float opacity)
        {
            //Draw the sprite with the given color and transparency
            sb.Draw(texture, bounds, color * opacity);
        }

        /// <summary>
        /// Draw the sprite with a rotation
        /// </summary>
        /// <param name="sb">SpriteBatch used to draw textures</param>
        /// <param name="color">Color that the sprite will be drawn with</param>
        /// <param name="dest">The destination of the sprite</param>
        /// <param name="loc">The current location of the sprite</param>
        /// <param name="pivot">Center of texture that becomes the rotation pivot point</param>
        /// <param name="isStretched">Whether or not the image to be drawn needs to be stretched towards its destination</param>
        public float DrawRotation(SpriteBatch sb, Color color, Vector2 dest, Vector2 loc, Vector2 pivot, bool isStretched)
        {
            //Calculate the displacement between the location and the destination to calculate an angle
            Vector2 displacement = dest - loc;

            //If the sprite is not at its destination
            if (displacement.X != 0 && displacement.Y != 0)
            {
                //Calculate the angle in radians that the texture should be rotated in depending on the location of the sprite and destination
                float angle = (float)Math.Atan2(displacement.Y, displacement.X);
                prevAngle = angle;
            }

            //Create the stretch factor and stretch the image if required
            float stretch = 1;
            if (isStretched)
            {
                stretch = displacement.Length() / texture.Width;
            }

            //Draw the sprite with the color, stretch factor and rotation angle in radians
            sb.Draw(texture, pos, null, color, prevAngle, pivot, new Vector2(stretch, 1), SpriteEffects.None, 0f);

            return prevAngle;
        }

        /// <summary>
        /// Draw the sprite with a rotation
        /// <param name="sb">SpriteBatch used to draw textures</param>
        /// <param name="color">Color that the sprite will be drawn with</param>
        /// <param name="angle">Angle (In degrees) that the sprite is rotated on</param>
        /// <param name="pivot">Center of texture that becomes the rotation pivot point</param>
        public void DrawRotation(SpriteBatch sb, Color color, float angle, Vector2 pivot)
        {
            //Change the angle to radians for drawing
            angle = MathHelper.ToRadians(angle);
            //Draw the sprite with the color and rotation angle in radians
            sb.Draw(texture, pos , null, color, angle, pivot, 1f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draw text to the screen
        /// </summary>
        /// <param name="sb">SpriteBatch used to draw textures</param>
        /// <param name="output">The text that will be displayed to the screen</param>
        /// <param name="font"> The font used in drawing the string</param>
        /// <param name="pos">The location of the string on the world</param>
        /// <param name="color">Color that the sprite will be drawn with</param>
        /// <param name="position">Location of the text drawn to the screen</param>
        public void DrawString(SpriteBatch sb, string output, SpriteFont font, Color color, Vector2 position)
        {
            //Draw the text to the screen with the color, font, and its position
            sb.DrawString(font, output, position, color);
        }
    }
}
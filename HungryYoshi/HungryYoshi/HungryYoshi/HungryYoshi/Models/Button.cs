using HungryYoshi.Controller.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HungryYoshi.Models
{
    class Button
    {
        //Draw Properties
        public string Text { get; set; }
        SpriteFont font;
        Sprite sprite;

        //Click Properties
        bool isClicked;
        int timer = 0;
        Pointer pointer;

        public Button(Vector2 pos, string text, SpriteFont font, ContentManager content, Pointer pointer)
        {
            Text = text;
            this.font = font;
            this.pointer = pointer;
            Texture2D texture = content.Load<Texture2D>("Button");
            Vector2 length = font.MeasureString(text);
            sprite = new Sprite(new Rectangle((int)pos.X, (int)pos.Y, (int)length.X + 20, (int)length.Y + 10), texture);             
        }

        /// <summary>
        /// Property to check if the button was clicked
        /// </summary>
        public bool IsClicked
        {
            get { return isClicked; }
            set { isClicked = value; }
        }

        /// <summary>
        /// Property to retrieve the button's sprite
        /// </summary>
        public Sprite GetSprite
        {
            get { return sprite; }
        }

        /// <summary>
        /// Check to see if the button was clicked by either controller
        /// </summary>
        public void CheckClick()
        {
            //If the kinect is connected:
            if (pointer.KinectController)
            {
                //Increment the timer if the pointer is inside the button, otherwise reset the timer
                if (pointer.GetSprite.GetBounds.X >= sprite.GetBounds.X - 5 && pointer.GetSprite.GetBounds.X <= sprite.GetBounds.X + sprite.GetBounds.Width + 5)
                {
                    if (pointer.GetSprite.GetBounds.Y >= sprite.GetBounds.Y - 5 && pointer.GetSprite.GetBounds.Y <= sprite.GetBounds.Y + sprite.GetBounds.Height + 5)
                    {
                        ++timer;
                    }
                }
                else
                {
                    timer = 0;
                }

                //Click the button if 3 seconds have passed.
                if (timer == 3 * Driver.REFRESH_RATE)
                {
                    isClicked = true;
                }
            }
            else
            {
                //Check to see if the mouse was clicked inside the button and store the result
                if (pointer.IsLeftClicked)
                {
                    if (pointer.GetSprite.GetBounds.X >= sprite.GetBounds.X && pointer.GetSprite.GetBounds.X <= sprite.GetBounds.X + sprite.GetBounds.Width)
                    {
                        if (pointer.GetSprite.GetBounds.Y >= sprite.GetBounds.Y && pointer.GetSprite.GetBounds.Y <= sprite.GetBounds.Y + sprite.GetBounds.Height)
                        {
                            isClicked = true;
                            pointer.IsLeftClicked = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw the button and the text inside to the screen
        /// </summary>
        /// <param name="sb">The SpriteBatch that is used to draw the textures</param>
        public void Draw(SpriteBatch sb)
        {
            sprite.Draw(sb);
            DrawString(sb);
        }

        /// <summary>
        /// Draw the shaded in pointer over the regular one
        /// </summary>
        /// <param name="sb">The spritebatch that is used to draw the textures</param>
        public void DrawClick(SpriteBatch sb)
        {
            //Only draw if using the Kinect
            if (pointer.KinectController)
            {
                pointer.GetClick.Draw(sb, Color.Red, (float)timer / (3 * Driver.REFRESH_RATE));
            }
        }

        /// <summary>
        /// Draw the text inside the button to the screen
        /// </summary>
        /// <param name="sb">The SpriteBatch used to draw the textures</param>
        private void DrawString(SpriteBatch sb)
        {
            sprite.DrawString(sb, Text, font, Color.DarkGoldenrod, new Vector2(sprite.GetPosition.X + 10, sprite.GetPosition.Y + 5));
        }
    }
}
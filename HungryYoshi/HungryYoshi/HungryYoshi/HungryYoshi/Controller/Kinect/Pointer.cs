using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HungryYoshi.Models;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;
using HungryYoshi.Controller.Computer;

namespace HungryYoshi.Controller.Kinect
{
    class Pointer
    {
        //Objects
        Sprite sprite;
        Sprite clicking;
        KinectManager kinect;
        MouseController mouse;

        //Properties
        int timer = 0;
        int clickTimer = 0;
        Vector2 prevPos;

        public Pointer(ContentManager content, KinectManager kinect, MouseController mouse)
        {
            this.kinect = kinect;
            this.mouse = mouse;
            Texture2D texture = content.Load<Texture2D>("Pointer");

            sprite = new Sprite(new Rectangle(-32, -32, Convert.ToInt32(texture.Width * 1.2), Convert.ToInt32(texture.Height * 1.2)), texture);
            clicking = new Sprite(sprite.GetBounds, sprite.Texture);
        }

        /// <summary>
        /// Property to get the sprite of the pointer
        /// </summary>
        public Sprite GetSprite
        {
            get { return sprite; }
        }

        /// <summary>
        /// Property to get the sprite of the overlaying pointer
        /// </summary>
        public Sprite GetClick
        {
            get { return clicking; }
        }

        /// <summary>
        /// Property to get the current game controller
        /// </summary>
        public bool KinectController
        {
            get { return kinect.KinectController; }
        }

        /// <summary>
        /// Property to get or set whether the left mouse button was clicked
        /// </summary>
        public bool IsLeftClicked
        {
            get { return mouse.IsLeftClicked; }
            set { mouse.IsLeftClicked = value; }
        }

        /// <summary>
        /// Updates the pointer location
        /// </summary>
        public void Update()
        {
            //If the game is not being played
            if (World.worldState != World.States.play)
            {
                //If there is a player currently recognized by the kinect or if the game is not in setup
                if (Player.gameState != Player.States.noplayer || Player.gameState != Player.States.setup)
                {
                    //Create variable to hold pointer location
                    Vector2 curPos;

                    //Set the pointer location to the current controller
                    if (kinect.KinectController)
                    {
                        SkeletonPoint rightHand = kinect.ScaledHand("right").Position;
                        curPos = new Vector2(rightHand.X, rightHand.Y);
                    }
                    else
                    {
                        curPos = mouse.CheckInput();
                    }

                    //Check to see if the pointer is currently being used
                    if (Math.Abs(curPos.X - prevPos.X) > 10 && Math.Abs(curPos.Y - prevPos.Y) > 10)
                    {
                        prevPos = curPos;
                        clickTimer = 0;

                        if (kinect.KinectController)
                        {
                            IsLeftClicked = false;
                        }
                    }
                    else
                    {
                        ++timer;
                        ++clickTimer;
                    }

                    if (clickTimer >= 3 * Driver.REFRESH_RATE)
                    {
                        IsLeftClicked = true;
                        clickTimer = 0;
                    }

                        sprite.SetPosition(curPos);
                        clicking.SetPosition(sprite.GetPosition);
                }
            }
        }

        /// <summary>
        /// Draw the pointer to the screen
        /// </summary>
        /// <param name="sb">SpriteBatch used to draw textures</param>
        public void Draw(SpriteBatch sb)
        {
            //Draw the pointer's sprite
            sprite.Draw(sb);
        }
    }
}
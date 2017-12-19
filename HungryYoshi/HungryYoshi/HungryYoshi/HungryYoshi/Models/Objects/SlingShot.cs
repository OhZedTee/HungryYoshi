using HungryYoshi.Controller.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HungryYoshi.Models.Objects
{
    class SlingShot
    {
        //Slingshot Sprite objects
        Texture2D slingShot;
        Sprite sShotSprite;

        //Rope Sprite Objects
        Texture2D rope;
        Sprite ropeSprite;
        Vector2 playerPos;

        //Kinect
        KinectManager kinect;

        //Timer
        private float timer = 0;

        public SlingShot(ContentManager content, KinectManager kinect, int gameHeight)
        {
            slingShot = content.Load<Texture2D>("Slingshot");
            rope = content.Load<Texture2D>("Rope");
            
            sShotSprite = new Sprite(new Rectangle(320, gameHeight - 288, slingShot.Width, 252), slingShot);
            ropeSprite = new Sprite(new Rectangle(sShotSprite.GetBounds.Center.X, sShotSprite.GetBounds.Y + 10, rope.Width, rope.Height), rope);

            this.kinect = kinect;           
        }

        /// <summary>
        /// Property to retrieve the position of the slingshot
        /// </summary>
        public Vector2 GetPosition
        {
            get { return sShotSprite.GetPosition; }
        }

        /// <summary>
        /// Property to get the Slingshot sprite
        /// </summary>
        public Sprite GetSprite
        {
            get { return sShotSprite; }
        }

        /// <summary>
        /// Property to get and set the players' position
        /// </summary>
        public Vector2 PlayerPos
        {
            get { return playerPos; }
            set { playerPos = value; }
        }

        /// <summary>
        /// Draws the slingshot and the rope until the player is flying for a specific time
        /// </summary>
        /// <param name="sb">The SpriteBatch that is used to draw the textures</param>
        public void Draw(SpriteBatch sb)
        {
            //Draw the slingshot
            sShotSprite.Draw(sb);

            //If the player hasn't been shot or if it has and only recently
            if (Player.gameState == Player.States.setup || (Player.gameState == Player.States.flying && timer < 0.3 * Driver.REFRESH_RATE))
            {
                //Increment the timer if the player has been shot
                if (Player.gameState != Player.States.setup)
                {
                    ++timer;
                }

                //Draw the rope
                Vector2 pivot = new Vector2(0, rope.Height * 0.5f);
                ropeSprite.DrawRotation(sb, Color.White, playerPos, ropeSprite.GetPosition, pivot, true);
            }
        }
    }
}
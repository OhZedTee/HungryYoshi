using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HungryYoshi.Models.Objects
{
    class Bubble : MapTile
    {
        //Properties
        public bool FollowPlayer { get; set; } //Whether or not the bubble should follow the player
        public Vector2 PlayerPos { get; set; } //The player's position

        public Bubble(Vector2 pos, ContentManager content): base(pos, content, "Bubble")
        {
        }

        public override void Update()
        {
            //Move the bubble to the players position if it is following the player
            if (FollowPlayer)
            {
                base.GetSprite.SetPosition(PlayerPos);
            }

            base.Update();
        }

        /// <summary>
        /// Draw the tile to the screen
        /// </summary>
        /// <param name="sb">SpriteBatch used to draw textures</param>
        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }
    }
}
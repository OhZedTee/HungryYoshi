using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HungryYoshi.Models.Objects
{
    class Apple : MapTile
    {
        //Property to check if the player completed the objective
        public bool LevelComplete { get; set; }
                   
        public Apple(Vector2 pos, ContentManager content) : base(pos, content, "Apple")
        {
            //Reset the apple corners to the proper location
            base.SetCorner((int)Corners.topLeft, new Vector2(base.GetSprite.GetBounds.Left, base.GetSprite.GetBounds.Top));
            base.SetCorner((int)Corners.topRight,new Vector2(base.GetSprite.GetBounds.Right, base.GetSprite.GetBounds.Top));
            base.SetCorner((int)Corners.bottomLeft, new Vector2(base.GetSprite.GetBounds.Left, base.GetSprite.GetBounds.Bottom));
            base.SetCorner((int)Corners.bottomRight, new Vector2(base.GetSprite.GetBounds.Right, base.GetSprite.GetBounds.Bottom));
        }

        public override void Update()
        {
            //Change the state of the game on completion
            if (LevelComplete)
            {
                World.worldState = World.States.finished;
                World.GameOutcome = "LEVEL COMPLETE";
            }

            base.Update();
        }

        /// <summary>
        /// Draw the tile to the screen
        /// </summary>
        /// <param name="sb">The SpriteBatch used to draw the textures</param>
        public override void Draw(SpriteBatch sb)
        {
            //Draw the apple to the screen
            base.Draw(sb);
        }
    }
}
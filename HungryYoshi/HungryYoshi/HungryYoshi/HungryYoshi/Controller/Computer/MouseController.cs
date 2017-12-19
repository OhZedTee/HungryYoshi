using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using HungryYoshi.Models;

namespace HungryYoshi.Controller.Computer
{
    class MouseController
    {
        //Stores the current and previous mouse states respectively
        private MouseState mouseState;
        private MouseState lastMouseState;
        
        //Mouse button properties
        private bool isLeftClick = false;

        /// <summary>
        /// Property to get if the left mouse button was clicked
        /// </summary>
        public bool IsLeftClicked
        {
            get { return isLeftClick; }
            set { isLeftClick = value; }
        }

        /// <summary>
        /// Checks if the mouse has moved or been clicked
        /// </summary>
        /// <returns>Returns the mouse's position</returns>
        public Vector2 CheckInput()
        {
            //Resets the mouse state
            lastMouseState = mouseState;

            //Fetches the new mouse state
            mouseState = Mouse.GetState();

            //Checks if the left button of the mouse has been clicked
            if (lastMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed)
            {
                isLeftClick = true;
            }
            else
            {
                isLeftClick = false;
            }
            //Return mouse position
            return new Vector2(mouseState.X, mouseState.Y);
        }       
    }
}
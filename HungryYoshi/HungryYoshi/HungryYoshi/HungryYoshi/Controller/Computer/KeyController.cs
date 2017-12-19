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


namespace HungryYoshi.Controller.Computer
{
    class KeyController
    {

        //Stores the current and previous states of the keyboard respectively
        KeyboardState kb;
        KeyboardState kbPrev;

        //Property that stores the last command
        public string KeyCommand { get; set; }

        /// <summary>
        /// Checks to see if a new key was pressed on the keyboard and store the key pressed in the keyboard property
        /// </summary>
        /// <returns>A boolean value representing whether or no a new key was pressed</returns>
        public bool CheckInput()
        {
            //Stores the previous keyboard state and gets the new state
            kbPrev = kb;
            kb = Keyboard.GetState();

            //Checks if a specific key was pressed and stores the command
            if (kb.IsKeyDown(Keys.Escape) && !kbPrev.IsKeyDown(Keys.Escape))
            {
                //Stores the "Escape" command
                KeyCommand = "Escape";
                return true;
            }
            else if (kb.IsKeyDown(Keys.Enter) && !kbPrev.IsKeyDown(Keys.Enter))
            {
                //Stores the "Enter" command
                KeyCommand = "Enter";
                return true;
            }
            else if (kb.IsKeyDown(Keys.Space) && !kbPrev.IsKeyDown(Keys.Space))
            {
                //Stores the "space" command
                KeyCommand = " ";
                return true;

            }
            else if (kb.IsKeyDown(Keys.R) && !kbPrev.IsKeyDown(Keys.R))
            {
                //Stores the "R" command
                KeyCommand = "R";
                return true;

            }
            else if (kb.IsKeyDown(Keys.B) && !kbPrev.IsKeyDown(Keys.B))
            {
                //Stores the "B" command
                KeyCommand = "B";
                return true;
            }
            else if (kb.IsKeyDown(Keys.P) && !kbPrev.IsKeyDown(Keys.P))
            {
                //Stores the "P" command
                KeyCommand = "P";
                return true;
            }
            else if (kb.IsKeyDown(Keys.D) && !kbPrev.IsKeyDown(Keys.D))
            {
                //Stores the "D" command
                KeyCommand = "D";
                return true;
            }
            else if (kb.IsKeyDown(Keys.Z) && !kbPrev.IsKeyDown(Keys.Z))
            {
                //Stores the "Z" command
                KeyCommand = "Z";
                return true;
            }
            else if (kb.IsKeyDown(Keys.Up) && !kbPrev.IsKeyDown(Keys.Up))
            {
                //Stores the "Up" command
                KeyCommand = "Up";
                return true;
            }
            else if (kb.IsKeyDown(Keys.Down) && !kbPrev.IsKeyDown(Keys.Down))
            {
                //Stores the "Down" command
                KeyCommand = "Down";
                return true;
            }

            //Reset the current command (no new key was clicked)
            KeyCommand = "";
            return false;
        }
    }
}
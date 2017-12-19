using HungryYoshi.Controller.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HungryYoshi.Models
{
    class Menu
    {
        //Menu objects
        Button[] options = new Button[4];
        Button[] levels;
        Button back;
        Pointer pointer;
        World world;

        //Meun properties
        string[] availableLevels;
        string topic;
        SpriteFont font;
        int screenWidth;
        int screenHeight;

        //State of the Level Designer
        public static States menuState = States.start;
        private static States prevState;
        public enum States : byte
        {
            start,
            play,
            levels,
            credits,
            designer,
            exit
        };

        public Menu(ContentManager content, Pointer pointer, int screenWidth, int screenHeight, World world)
        {
            //Save the sprite and load the font
            this.pointer = pointer;
            font = content.Load<SpriteFont>("buttonFont");
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.world = world;

            //Grab the location of all the tiles
            availableLevels = Directory.GetFiles(@"Levels\");

            //Cut out all the unneeded parts of the file name (.txt and path)
            for (int i = 0; i < availableLevels.Length; ++i)
            {
                string[] s = availableLevels[i].Split('.');
                availableLevels[i] = s[0].Substring(7);
            }

            //Show the user the levels available and let them choose one
            levels = new Button[availableLevels.Length];
            for (int i = 0; i < levels.Length; ++i)
            {
                //Get the text from all the available files in the directory and measure the size
                string text = availableLevels[i];
                Vector2 size = font.MeasureString(text);

                //Create the buttons for each level
                if (i == 0)
                {
                    levels[i] = new Button(new Vector2((screenWidth * 0.5f) - (size.X * 0.5f), size.Y + 50), text, font, content, pointer);
                }
                else
                {
                    levels[i] = new Button(new Vector2((screenWidth * 0.5f) - (size.X * 0.5f), levels[i - 1].GetSprite.GetBounds.Y + size.Y + 50), text, font, content, pointer);
                }
            }

            //Create the buttons for the menu options
            string output = "PLAY";
            Vector2 textSize = font.MeasureString(output);
            options[0] = new Button(new Vector2((screenWidth * 0.5f) - (textSize.X * 0.5f), textSize.Y + 50), output, font, content, pointer);
            output = "DESIGNER";
            textSize = font.MeasureString(output);
            options[1] = new Button(new Vector2((screenWidth * 0.5f) - (textSize.X * 0.5f), options[0].GetSprite.GetBounds.Y + textSize.Y + 50), output, font, content, pointer);
            output = "CREDITS";
            textSize = font.MeasureString(output);
            options[2] = new Button(new Vector2((screenWidth * 0.5f) - (textSize.X * 0.5f), options[1].GetSprite.GetBounds.Y + textSize.Y + 50), output, font, content, pointer);
            output = "EXIT";
            textSize = font.MeasureString(output);
            options[3] = new Button(new Vector2((screenWidth * 0.5f) - (textSize.X * 0.5f), options[2].GetSprite.GetBounds.Y + textSize.Y + 50), output, font, content, pointer);
            output = "BACK";
            textSize = font.MeasureString(output);
            back = new Button(new Vector2(10, 10), output, font, content, pointer);

            //Create the game topic
            topic = "Hungry Yoshi";
        }

        /// <summary>
        /// Check to see if any of the buttons were clicked
        /// </summary>
        public void CheckClick()
        {
            //Go through every object and checked if they are clipped
            for (int i = 0; i < options.Length; ++i)
            {
                //Check for a click
                options[i].CheckClick();

                //Check for a link
                if (options[i].IsClicked)
                {
                    if (options[i].Text == "PLAY")
                    {
                        prevState = menuState;
                        menuState = States.play;
                    }
                    else if (options[i].Text == "DESIGNER")
                    {
                        prevState = menuState;
                        menuState = States.designer;
                    }
                    else if (options[i].Text == "CREDITS")
                    {
                        prevState = menuState;
                        menuState = States.credits;
                    }
                    else if (options[i].Text == "EXIT")
                    {
                        prevState = menuState;
                        menuState = States.exit;
                    }

                    options[i].IsClicked = false;
                    pointer.IsLeftClicked = false;
                    break;
                }
            }
        }

        /// <summary>
        /// Update data from each of the states
        /// </summary>
        public void Update()
        {
            switch (menuState)
            {
                    //If the player is in the play state, change his/her state to the levels state
                case States.play:
                    menuState = States.levels;
                    break;

                    //Show the user the levels and let the user choose the level they want to play
                case States.levels:
                    topic = "Choose A Level:";
                    PickLevel();
                    ReturnClick();
                    break;

                    //Let's the user make is own computer
                case States.designer:
                    World.worldState = World.States.designer;
                    ReturnClick();
                    break;

                    ///Exist the gate when neded
                case States.exit:
                    Environment.Exit(0);
                    break;

                    //Show the credits onto the screej
                case States.credits:
                    topic = "Credits";
                    ReturnClick();
                    break;
            }
        }

        /// <summary>
        /// Check which level was clicked
        /// </summary>
        private void PickLevel()
        {
            for (int i = 0; i < levels.Length; ++i)
            {
                //Check if there is any other psition while on internet
                levels[i].CheckClick();

                //If the button was clicked
                if (levels[i].IsClicked)
                {
                    //Create the level with the text of the level button + .txt
                    world.Level = levels[i].Text;
                    world.LoadWorld();
                    World.worldState = World.States.play;
                    levels[i].IsClicked = false;
                    pointer.IsLeftClicked = false;
                    break;
                }
            }
        }

        /// <summary>
        /// Draw the back button and check to see if it is clicked
        /// </summary>
        public void ReturnClick()
        {
            //Check the button for a prevous trajectory
            back.CheckClick();

            //If the player is not shooting any button
            if (back.IsClicked)
            {
                //If the designer is not being used, strore the previous bkajjgs
                if (menuState != States.designer)
                {
                    menuState = prevState;
                }
                else
                {
                    menuState = States.start;
                    World.worldState = World.States.menu;
                }
                back.IsClicked = false;
                pointer.IsLeftClicked = false;
            }
        }

        /// <summary>
        /// Draw the sprites to the screen
        /// </summary>
        /// <param name="sb">The spritebatch that is used to draw the textures</param>

        public void Draw(SpriteBatch sb)
        {
            switch (menuState)
            {
                    //Draw the start state
                case States.start:

                    //Draw the buttons
                    for (int i = 0; i < options.Length; ++i)
                    {
                        options[i].Draw(sb);
                    }
                    break;

                    //Draw the levek state
                case States.levels:

                    for (int i = 0; i < levels.Length; ++i)
                    {
                        //Draw the buttons
                        levels[i].Draw(sb);

                    }
                    break;

                    //Create the credits Chem
                case States.credits:
                    string name = "\nYoshi Moshi";
                    Vector2 nameSize = font.MeasureString(name);
                    string text = "             Producer: Ori Talmor \n      Project Manager: Ori Talmor \n        Programmers: Ori Talmor \n     Game Designers: Ori Talmor \n                 A Big Thanks To:   \n\n                       Trevor Lane\n \nFor Making Me Do This Horrendous Project";
                    Vector2 size = font.MeasureString(text);
                    sb.DrawString(font, name, new Vector2((screenWidth * 0.5f) - (nameSize.X * 0.5f), nameSize.Y + 10), Color.DarkSeaGreen);
                    sb.DrawString(font, text, new Vector2((screenWidth * 0.5f) - (size.X / 4) - 60, size.Y - nameSize.Y), Color.DarkSlateGray);
                    break;
            }

            Vector2 textSize = font.MeasureString(topic);
            sb.DrawString(font, topic, new Vector2((screenWidth * 0.5f) - (textSize.X * 0.5f), textSize.Y + 10), Color.Green);

            DrawBack(sb);

        }
        /// <summary>
        /// Draw the pointers overlaying the original pointers wth an opacity
        /// </summary>
        /// <param name="sb">The spritebatch that is used to draw the textures</param>
        public void DrawHover(SpriteBatch sb)
        {
            switch (menuState)
            {
                    //Check if the player is in the start portian of the menu and draw the pointer for it
                case States.start:

                    for (int i = 0; i < options.Length; ++i)
                    {
                        options[i].DrawClick(sb);
                    }
                    break;

                    //Check if the player is in the levels portian of the menu and draw the pointer for it
                case States.levels:

                    for (int i = 0; i < levels.Length; ++i)
                    {
                        levels[i].DrawClick(sb);
                    }
                    break;
            }
        }

        /// <summary>
        /// Draw the back button  on the world
        /// </summary>
        /// <param name="sb">The spritebatch that is used to draw the textures</param>
        public void DrawBack(SpriteBatch sb)
        {
            //Draw the back button as long as the menu is not the first screen
            if (menuState != States.start)
            {
                back.Draw(sb);
                back.DrawClick(sb);
            }
        }
    }
}

using HungryYoshi.Controller.Computer;
using HungryYoshi.Controller.Kinect;
using HungryYoshi.Models.Objects;
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
    class Designer
    {
        //Objects
        Button[] operationButtons = new Button[8];
        Pointer pointer;
        KinectManager kinect;
        List<MapTile> listTiles = new List<MapTile>();
        KeyController kb = new KeyController();
        Button clickedButton = null;

        //Properties
        private const int TILESIZE = 32;
        bool isRotating = false;
        MapTile rotatingTile = null;
        string buttonText;

        //State of the Level Designer
        public static States designerState = States.start;
        public enum States : byte
        {
            start,
            brick,
            trampoline,
            spike,
            wCushion,
            bubble,
            apple,
            remove,
            save
        };


        public Designer(KinectManager kinect, ContentManager content, Pointer pointer, int screenWidth, int screenHeight)
        {
            //Save the sprite and load the font
            this.pointer = pointer;
            SpriteFont font = content.Load<SpriteFont>("buttonFont");

            //Initialize the kinect
            this.kinect = kinect;

            //Create the buttons for the tile objects
            string output = "Brick";
            Vector2 textSize = font.MeasureString(output);
            operationButtons[0] = new Button(new Vector2(10, screenHeight - textSize.Y - 20), output, font, content, pointer);
            output = "Trampoline";
            textSize = font.MeasureString(output);
            operationButtons[1] = new Button(new Vector2(operationButtons[0].GetSprite.GetBounds.X + operationButtons[0].GetSprite.GetBounds.Width + 10, screenHeight - textSize.Y - 20), output, font, content, pointer);
            output = "Spike";
            textSize = font.MeasureString(output);
            operationButtons[2] = new Button(new Vector2(operationButtons[1].GetSprite.GetBounds.X + operationButtons[1].GetSprite.GetBounds.Width + 10, screenHeight - textSize.Y - 20), output, font, content, pointer);
            output = "Whoopee Cushion";
            textSize = font.MeasureString(output);
            operationButtons[3] = new Button(new Vector2(operationButtons[2].GetSprite.GetBounds.X + operationButtons[2].GetSprite.GetBounds.Width + 10, screenHeight - textSize.Y - 20), output, font, content, pointer);
            output = "Bubble";
            textSize = font.MeasureString(output);
            operationButtons[4] = new Button(new Vector2(operationButtons[3].GetSprite.GetBounds.X + operationButtons[3].GetSprite.GetBounds.Width + 10, screenHeight - textSize.Y - 20), output, font, content, pointer);
            output = "Apple";
            textSize = font.MeasureString(output);
            operationButtons[5] = new Button(new Vector2(operationButtons[4].GetSprite.GetBounds.X + operationButtons[4].GetSprite.GetBounds.Width + 10, screenHeight - textSize.Y - 20), output, font, content, pointer);
            output = "Remove";
            textSize = font.MeasureString(output);
            operationButtons[6] = new Button(new Vector2(operationButtons[5].GetSprite.GetBounds.X + operationButtons[5].GetSprite.GetBounds.Width + 10, screenHeight - textSize.Y - 20), output, font, content, pointer);
            output = "Save";
            textSize = font.MeasureString(output);
            operationButtons[7] = new Button(new Vector2(operationButtons[6].GetSprite.GetBounds.X + operationButtons[6].GetSprite.GetBounds.Width + 10, screenHeight - textSize.Y - 20), output, font, content, pointer);
        }

        /// <summary>
        /// Check to see if any of the buttons were clicked
        /// </summary>
        public void CheckClick()
        {
            //Check to see if any of the buttons were clicked
            for (int i = 0; i < operationButtons.Length; ++i)
            {
                operationButtons[i].CheckClick();
                
                //If this button is not already selected
                if (clickedButton == null || operationButtons[i] != clickedButton)
                {
                    //Make sure the button is actually clicked
                    if (operationButtons[i].IsClicked)
                    {
                        //Perform specific operations/change data based on specific button
                        //Change the states of the designer
                        if (operationButtons[i].Text == "Brick")
                        {
                            designerState = States.brick;
                        }
                        else if (operationButtons[i].Text == "Trampoline")
                        {
                            designerState = States.trampoline;
                        }
                        else if (operationButtons[i].Text == "Spike")
                        {
                            designerState = States.spike;
                        }
                        else if (operationButtons[i].Text == "Whoopee Cushion")
                        {
                            designerState = States.wCushion;
                        }
                        else if (operationButtons[i].Text == "Bubble")
                        {
                            designerState = States.bubble;
                        }
                        else if (operationButtons[i].Text == "Apple")
                        {
                            designerState = States.apple;
                        }
                        else if (operationButtons[i].Text == "Remove")
                        {
                            designerState = States.remove;
                        }
                        else if (operationButtons[i].Text == "Save")
                        {
                            //Save the game and go back to the menu
                            designerState = States.save;
                            Save();
                            World.worldState = World.States.menu;
                        }

                        //Store the last button that was clicked
                        clickedButton = operationButtons[i];

                        //Make the button not clicked anymore
                        operationButtons[i].IsClicked = false;
                        pointer.IsLeftClicked = false;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Update the tiles on the screen based on the location were the pointer was clicked
        /// </summary>
        /// <param name="content"></param>
        public void Update(ContentManager content)
        {
            //If the player is clicking to place an object
            if (pointer.IsLeftClicked || kinect.PlaceObject)
            {
                //Find the absolute location of the tile being addded
                Vector2 tilePos = pointer.GetSprite.GetPosition;
                tilePos.X = tilePos.X / TILESIZE;
                tilePos.Y = tilePos.Y / TILESIZE;
                tilePos.X = (float)Math.Truncate(tilePos.X);
                tilePos.Y = (float)Math.Truncate(tilePos.Y);
                tilePos = tilePos * TILESIZE;

                //Check if a tile can be placed at that location
                bool canPlaceTile = true;
                for (int i = 0; i < listTiles.Count; ++i)
                {
                    //If there is a tile at the choosen location
                    if (listTiles[i].GetSprite.GetPosition == tilePos)
                    {
                        //Remove the tile if the player is trying to remove it
                        if (designerState == States.remove)
                        {
                            listTiles.RemoveAt(i);
                            break;
                        }
                        else
                        {
                            //Set the tile to rotate mode if the player is trying to rotate the tile
                            if (listTiles[i] is Spike || listTiles[i] is Trampoline || listTiles[i] is WCushion)
                            {
                                isRotating = true;
                                rotatingTile = listTiles[i];
                                break;
                            }
                        }

                        //Otherwise, a tile cannot be placed at that location
                        canPlaceTile = false;
                        break;
                    }
                }

                //Set the kinect to not be placing an object for future updates
                kinect.PlaceObject = false;

                //If the player isn't rotating the tile and one can be placed
                if (!isRotating && canPlaceTile)
                {
                    //Add the tile based on the state of the designer
                    switch (designerState)
                    {
                        case States.brick:
                            listTiles.Add(new Brick(tilePos, content));
                            break;
                        case States.trampoline:
                            listTiles.Add(new Trampoline(tilePos, content, 0));
                            break;
                        case States.spike:
                            listTiles.Add(new Spike(tilePos, content, 0));
                            break;
                        case States.wCushion:
                            listTiles.Add(new WCushion(tilePos, content, 0, null));
                            break;
                        case States.bubble:
                            listTiles.Add(new Bubble(tilePos, content));
                            break;
                        case States.apple:
                            listTiles.Add(new Apple(tilePos, content));
                            break;
                    }
                }
            }
            else
            {
                //Rotate the player with the kinect controller if required
                if (kinect.KinectController)
                {
                    //If the player is rotating the tile
                    if (isRotating && rotatingTile != null)
                    {
                        //Change the text of the button to say rotate                        
                        if (clickedButton.Text != "Rotate")
                        {
                            buttonText = clickedButton.Text;
                        }
                        clickedButton.Text = "Rotate";

                        //If the kinect is rotating a tile:
                        if (kinect.Rotating)
                        {
                            //Increment the angle of the object
                            if (rotatingTile is Spike)
                            {
                                ((Spike)rotatingTile).Angle += 2;
                            }
                            else if (rotatingTile is Trampoline)
                            {
                                ((Trampoline)rotatingTile).Angle += 2;
                            }
                            else if (rotatingTile is WCushion)
                            {
                                ((WCushion)rotatingTile).Angle += 2;
                            }
                        }
                        //If the mouse is rotating a tile
                        else
                        {
                            //Return to the regular state of the designer
                            isRotating = false;
                            rotatingTile = null;

                            //Update the text in the button
                            if (buttonText != null)
                            {
                                clickedButton.Text = buttonText;
                            }
                        }
                    }
                }
                else
                {
                    //If the player is rotating the tile
                    if (isRotating && rotatingTile != null)
                    {
                        //Change the text of the button to say rotate
                        if (clickedButton.Text != "Rotate")
                        {
                            buttonText = clickedButton.Text;
                        }
                        clickedButton.Text = "Rotate";

                        //Check for player input
                        if (kb.CheckInput())
                        {
                            //If the player pressed the up or down button and the tile is a spike, trampoline, or a whoopee cushion,
                            //rotate the tile in the direction
                            if (kb.KeyCommand == "Up")
                            {
                                if (rotatingTile is Spike)
                                {
                                    ((Spike)rotatingTile).Angle += 5;
                                }
                                else if (rotatingTile is Trampoline)
                                {
                                    ((Trampoline)rotatingTile).Angle += 5;
                                }
                                else if (rotatingTile is WCushion)
                                {
                                    ((WCushion)rotatingTile).Angle += 5;
                                }
                            }
                            else if (kb.KeyCommand == "Down")
                            {
                                if (rotatingTile is Spike)
                                {
                                    ((Spike)rotatingTile).Angle -= 5;
                                }
                                else if (rotatingTile is Trampoline)
                                {
                                    ((Trampoline)rotatingTile).Angle -= 5;
                                }
                                else if (rotatingTile is WCushion)
                                {
                                    ((WCushion)rotatingTile).Angle -= 5;
                                }
                            }
                            //Check for the player finishing to rotate the tile
                            else if (kb.KeyCommand == "Enter")
                            {
                                //Return to the regular state of the designer
                                isRotating = false;
                                rotatingTile = null;
                                if (buttonText != null)
                                {
                                    clickedButton.Text = buttonText;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save the map created into a textfile
        /// </summary>
        public void Save()
        {
            //Get number of rows of tiles and the amount of tiles on each row
            int y = 0;
            int x = 0;
            for (int i = 0; i < listTiles.Count; ++i)
            {
                //Get the current row and coloumn of the tile
                int row = listTiles[i].GetSprite.GetBounds.Y / TILESIZE;
                int coloumn = listTiles[i].GetSprite.GetBounds.X / TILESIZE;

                //Store the largest row and coloumn
                if (row > y)
                {
                    y = row;
                }

                if (coloumn > x)
                {
                    x = coloumn;
                }
            }
            
            //Increment the row and coloumn so that all the tiles can be added in their proper element
            ++x;
            ++y;

            //Create a grid layout of all the possible tiles and set them to blank tiles
            Vector2[,] mappedTiles = new Vector2[x, y];
            for (int i = 0; i < (mappedTiles.Length / x); ++i)
            {
                for (int k = 0; k < (mappedTiles.Length / y); ++k)
                {
                    mappedTiles[k, i] = Vector2.Zero;
                }
            }

            //For every tile on the screen
            for (int i = 0; i < listTiles.Count; ++i)
            {
                //Get the specific row and coloumn of the tile on the grid
                int coloumn = listTiles[i].GetSprite.GetBounds.X / TILESIZE;
                int row = listTiles[i].GetSprite.GetBounds.Y / TILESIZE;

                //For every tile type:
                if (listTiles[i] is Trampoline)
                {
                    //Get the positive equivalent angle of the tile
                    if (((Trampoline)listTiles[i]).Angle < 0)
                    {
                        ((Trampoline)listTiles[i]).Angle = 360 + ((Trampoline)listTiles[i]).Angle;
                    }

                    //Add the tile into the grid with the angle
                    mappedTiles[coloumn, row] = new Vector2(1, ((Trampoline)listTiles[i]).Angle);
                }
                else if (listTiles[i] is Bubble)
                {
                    //Add the tile into the grid without an angle
                    mappedTiles[coloumn, row] = new Vector2(2, -1);
                }
                else if (listTiles[i] is Spike)
                {
                    //Get the positive equivalent angle of the tile
                    if (((Spike)listTiles[i]).Angle < 0)
                    {
                        ((Spike)listTiles[i]).Angle = 360 + ((Spike)listTiles[i]).Angle;
                    }

                    //Add the tile into the grid with the angle
                    mappedTiles[coloumn, row] = new Vector2(3, ((Spike)listTiles[i]).Angle);
                }
                else if (listTiles[i] is WCushion)
                {
                    //Get the positive equivalent angle of the tile
                    if (((WCushion)listTiles[i]).Angle < 0)
                    {
                        ((WCushion)listTiles[i]).Angle = 360 + ((WCushion)listTiles[i]).Angle;
                    }

                    //Add the tile into the grid with the angle
                    mappedTiles[coloumn, row] = new Vector2(4, ((WCushion)listTiles[i]).Angle);
                }
                else if (listTiles[i] is Apple)
                {
                    //Add the tile into the grid without an angle
                    mappedTiles[coloumn, row] = new Vector2(5, -1);
                }
                else if (listTiles[i] is Brick)
                {
                    //Add the tle into the grid without an angle
                    mappedTiles[coloumn, row] = new Vector2(6, -1);
                }
            }

            //Create an array which stores the individual rows of the grid
            string[] tiles = new string[y];
            int curHeight = 0;

            //Stores the tiles from the grid into their respective row
            for (int k = 0; k < y; ++k)
            {
                for (int i = 0; i < x; ++i)
                {
                    //For every tile add it into the respective row based on the values in the grid index
                    if (mappedTiles[i,k] == Vector2.Zero)
                    {
                        //Add a 0 for a blank tile
                        tiles[curHeight] += "0,";
                    }
                    
                    //If Y value in the grid index shows no angle
                    else if (mappedTiles[i, k].Y < 0)
                    {
                        //Add the specific tile with no angle
                        tiles[curHeight] += mappedTiles[i, k].X.ToString() + ",";
                    }
                       
                    //If the Y value in the grid index shows an angle
                    else if (mappedTiles[i, k].Y > 0)
                    {
                        //Add the specific tile with its index
                        tiles[curHeight] += mappedTiles[i, k].X.ToString() + "," + mappedTiles[i, k].Y.ToString() + ",";
                    }
                }

                //Move onto the next row of tiles
                ++curHeight;
            }

            //Create the new map with the given tiles
            Map map = new Map(tiles);

            //Create the map's name based on the current amount of maps with the same name
            string[] fileNames = Directory.GetFiles(@"Levels\");

            //Cut out all the unneeded parts of the file name (.txt and path)
            for (int i = 0; i < fileNames.Length; ++i)
            {
                string[] s = fileNames[i].Split('.');
                fileNames[i] = s[0].Substring(7);
            }

            //Record the number of custom maps already created
            int numCustoms = 0;
            for (int i = 0; i < fileNames.Length; ++i)
            {
                if (fileNames[i].Length > 6)
                {
                    if (fileNames[i].Substring(0, 6) == "Custom")
                    {
                        ++numCustoms;
                    }
                }
            }

            //Store the map with the next in line name (ie. Custom1, Custom2, Custon 3...)
            ++numCustoms;
            map.SaveLevel(@"Levels\\Custom" + numCustoms + ".txt");
        }

        /// <summary>
        /// Draw the buttons and Tiles to the screen
        /// </summary>
        /// <param name="sb">The SpriteBatch used to draw the textures</param>
        public void Draw(SpriteBatch sb)
        {
            //Draw all the buttons
            for (int i = 0; i < operationButtons.Length; ++i)
            {
                operationButtons[i].Draw(sb);
            }

            //Draw all the tiles on the screen
            for (int i = 0; i < listTiles.Count; ++i)
            {
                listTiles[i].Draw(sb);
            }
        }

        /// <summary>
        /// Draw the shaded in pointer to the screen.
        /// </summary>
        /// <param name="sb"SpriteBatch>The SpriteBatch used to draw the textures<</param>
        public void DrawHover(SpriteBatch sb)
        {
            //Draw all the buttons
            for (int i = 0; i < operationButtons.Length; ++i)
            {
                operationButtons[i].DrawClick(sb);
            }
        }
    }
}
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
    class Map
    {
        //Information on tiles
        List<MapTile> listTiles = new List<MapTile>();
        string[] tiles;

        //Objects
        Player player;
        SlingShot slingshot;

        //Properties
        int gameHeight = 0;
        int gameWidth = 0;

        //Constant tile size
        public const int TILESIZE = 32;

        public Map()
        {
        }

        public Map(string[] tiles)
        {
            this.tiles = tiles;
        }

        /// <summary>
        /// Property to get the dimensions of the game
        /// </summary>
        public int[] GameDimensions
        {
            get { return new int[2] { gameHeight, gameWidth }; }
        }

        /// <summary>
        /// Property to get the slingshot used in the game
        /// </summary>
        public SlingShot SetSlingShot
        {
            set { slingshot = value; }
        }

        /// <summary>
        /// Property to get the player object in the game
        /// </summary>
        public Player SetPlayer
        {
            set { player = value; }
        }

        /// <summary>
        /// Update each bubble's properties
        /// </summary>
        public void BubbleUpdate()
        {
            //F0r all the bubble tiles in the game, store the player's position
            for (int i = 0; i < listTiles.Count; ++i)
            {
                if (listTiles[i] is Bubble)
                {
                    ((Bubble)listTiles[i]).PlayerPos = new Vector2(player.GetSprite.GetPosition.X - (player.GetSprite.GetBounds.Width * 0.5f), player.GetSprite.GetPosition.Y - (player.GetSprite.GetBounds.Height * 0.5f));
                }
            }
        }

        #region Collision Detection

        /// <summary>
        /// Make sure the player is in the screen during setup
        /// </summary>
        public void SetupBoundary()
        {
            //Update the player's position based on what border they crossed
            if (player.GetSprite.GetPosition.X < 16)
            {
                player.GetSprite.SetPosition("X", 16);
            }
            else if (player.GetSprite.GetPosition.Y < 16)
            {
                player.GetSprite.SetPosition("Y", 16);
            }
            else if (player.GetSprite.GetPosition.Y > (slingshot.GetSprite.GetBounds.Y + slingshot.GetSprite.GetBounds.Height) - player.GetSprite.GetBounds.Height)
            {
                player.GetSprite.SetPosition("Y", (slingshot.GetSprite.GetBounds.Y + slingshot.GetSprite.GetBounds.Height) - player.GetSprite.GetBounds.Height);
            }
            else if (player.GetSprite.GetPosition.X > slingshot.GetSprite.GetBounds.X - player.GetSprite.GetBounds.Width)
            {
                player.GetSprite.SetPosition("X", slingshot.GetSprite.GetBounds.X - player.GetSprite.GetBounds.Width);
            }
        }

        /// <summary>
        /// Check if a collision occured and preform tasks specific to the collided object type.
        /// </summary>
        public void CheckPossibleCollision()
        {
            //Check every tile in the game:
            for (int i = 0; i < listTiles.Count; ++i)
            {
                //If an abstract circle around the tile is intersecting with the player:
                if (listTiles[i].CircleCollision(player))
                {
                    //Check which tile has a possible collision
                    if (listTiles[i] is Trampoline)
                    {
                        //Preform actual collision detection between the tile and player to make sure
                        Trampoline tempTramp = (Trampoline)listTiles[i];
                        if (CalcRectangleSides(player.GetCorners, tempTramp.GetCorners))
                        {
                            //Check which side of the trampoline player hit.
                            Vector2[] testPoints = new Vector2[3] { tempTramp.GetMidpoint, tempTramp.GetCorners[(int)MapTile.Corners.topLeft], tempTramp.GetCorners[(int)MapTile.Corners.topRight] };
                            for (int k = 0; k < testPoints.Length; ++k)
                            {
                                //Check to see if the collision being checked is with the midpoint
                                bool isMidpoint = false;
                                if (testPoints[k] == tempTramp.GetMidpoint)
                                {
                                    isMidpoint = true;
                                }

                                //Preform point to rectangle collision with the player to check for a collision on the specific side
                                tempTramp.HitSide = !SurfaceCollision(player.GetSprite.GetBounds, testPoints[k], isMidpoint);

                                //If there is a collision with the specific side break out of the loop
                                if (!tempTramp.HitSide)
                                {
                                    break;
                                }
                            }

                            //Change the players' trajectory based on the side that was hit
                            tempTramp.ChangePlayerTraj(player);
                        }
                    }
                    if (listTiles[i] is Spike)
                    {
                        //Perform actual collision detection between the tile and player to make sure
                        Spike tempSpike = (Spike)listTiles[i];
                        if (CalcRectangleSides(player.GetCorners, tempSpike.GetCorners))
                        {
                            //Check which side of the trampoline player hit.
                            Vector2[] testPoints = new Vector2[3] { tempSpike.GetMidpoint, tempSpike.GetCorners[(int)MapTile.Corners.topLeft], tempSpike.GetCorners[(int)MapTile.Corners.topRight] };
                            for (int k = 0; k < testPoints.Length; ++k)
                            {
                                //Check to see if the collision being checked is with the midpoint
                                bool isMidpoint = false;
                                if (testPoints[k] == tempSpike.GetMidpoint)
                                {
                                    isMidpoint = true;
                                }

                                //Perform point to rectangle collision with the player to check for a collision on the specific side
                                tempSpike.HitSide = !SurfaceCollision(player.GetSprite.GetBounds, testPoints[k], isMidpoint);

                                //If there is a collision with the specific side break out of the loop
                                if (!tempSpike.HitSide)
                                {
                                    break;
                                }
                            }

                            //Change player trajectory (or possibly kill him) based on the side that was hit
                            tempSpike.ChangePlayerTraj(player);
                        }
                    }
                    if (listTiles[i] is Bubble)
                    {
                        if (!player.InBubble && !player.PoppedBubble)
                        {
                            //Preform actual collision detection between the tile and player to make sure
                            if (CalcRectangleSides(player.GetCorners, listTiles[i].GetCorners))
                            {
                                //Change the properties of both the player and the bubble
                                player.InBubble = true;
                                ((Bubble)listTiles[i]).FollowPlayer = true;
                            }
                        }

                        //Change the acceleraion of the player based on the velocity
                        if (player.PoppedBubble && !player.InBubble)
                        {
                            player.PoppedBubble = false;
                        }
                        else if (player.PoppedBubble && player.InBubble)
                        {
                            listTiles.RemoveAt(i);
                            player.PoppedBubble = false;
                            player.InBubble = false;
                        }
                    }
                    if (listTiles[i] is Apple)
                    {
                        //Preform actual collision detection between the tile and player to make sure
                        if (CalcRectangleSides(player.GetCorners, listTiles[i].GetCorners))
                        {
                            //Change the properties of the apple to make the level complete
                            ((Apple)listTiles[i]).LevelComplete = true;
                        }
                    }
                    if (listTiles[i] is Brick)
                    {
                        //Preform actual collision detection between the tile and player to make sure
                        Brick tempBrick = (Brick)listTiles[i];

                        //player .GetSprite.SetPosition(player.GetSprite.GetPosition + (player.Trajectory * 2));
                        //player.CalcCorners();

                        if (CalcRectangleSides(player.GetCorners, tempBrick.GetCorners))
                        {
                            //Check which side of the brick player hit.
                            Vector2[] testPoints = new Vector2[4] 
                            { 
                                tempBrick.GetMidpoints[1],
                                tempBrick.GetMidpoints[3],
                                tempBrick.GetMidpoints[0],
                                tempBrick.GetMidpoints[2],
                            };

                            for (int k = 0; k < testPoints.Length; ++k)
                            {
                                //Check to see if the player collided with any of the midpoints of the brick
                                if (SurfaceCollision(player.GetSprite.GetBounds, testPoints[k], true))
                                {
                                    //Stop the creation of particles
                                    player.NoParticles = true;

                                    //Check which midpoint was caught to ne colliding with the player
                                    if (k == 2)
                                    {
                                        //Change the players trajectory and remove gravity effects since it hit the top of the brick.
                                        tempBrick.ChangePlayerTraj("top", player);

                                        //If the player is not in the bubble
                                        if (!player.InBubble)
                                        {
                                            player.CancelGravity = true;
                                        }
                                    }
                                    else
                                    {
                                        //Reapply gravity
                                        player.CancelGravity = false;

                                        //Change the player's trajectory or location based on the side that was hit.
                                        if (k == 0)
                                        {
                                            tempBrick.ChangePlayerTraj("right", player);
                                        }
                                        else if (k == 1)
                                        {
                                            // Player.gameState = Player.States.test;
                                            tempBrick.ChangePlayerTraj("left", player);
                                        }
                                        else if (k == 3)
                                        {
                                            tempBrick.ChangePlayerTraj("bottom", player);
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                    }
                    //Reactivate the particles if no specific tile was hit.
                    else
                    {
                        player.NoParticles = false;
                    }
                }
            }
        }

        /// <summary>
        /// Check to see if a collision occured between the rotated postions. 
        /// </summary>
        /// <param name="aCorners"An array of vectors representing the coners of an objrct</param>
        /// <param name="bCorners">An array of vectors representing the coners of an objrct</param>
        /// <returns></returns>
        private bool CalcRectangleSides(Vector2[] aCorners, Vector2[] bCorners)
        {
            //Create the axis' for projection
            Vector2[] axis = new Vector2[4]
            {
                new Vector2(aCorners[1].X - aCorners[0].X, aCorners[1].Y - aCorners[0].Y),
                new Vector2(aCorners[1].X - aCorners[3].X, aCorners[1].Y - aCorners[3].Y),
                new Vector2(bCorners[0].X - bCorners[2].X, bCorners[0].Y - bCorners[2].Y),
                new Vector2(bCorners[0].X - bCorners[1].X, bCorners[0].Y - bCorners[1].Y)
            };

            //Project each corner onto each axis:
            for (int i = 0; i < axis.Length; ++i)
            {
                //Projections variables in order to check for the largest and smallest projection for each object
                float[] aValues = new float[4];
                float[] bValues = new float[4];
                float minA = Int32.MaxValue;
                float maxA = 0;
                float minB = Int32.MaxValue;
                float maxB = 0;

                //Calculate the projection of the first object
                Vector2[] projectionA = new Vector2[4];
                for (int k = 0; k < aCorners.Length; ++k)
                {
                    //Calculate the projection
                    double proj = ((aCorners[k].X * axis[i].X) + (aCorners[k].Y * axis[i].Y)) / (Math.Pow(axis[i].X, 2) + Math.Pow(axis[i].Y, 2));
                    projectionA[i] = new Vector2((float)proj * axis[i].X, (float)proj * axis[i].Y);

                    //Create a scalar value of each projection and add it to the list of projections
                    float scalarValue = ((projectionA[i].X * axis[i].X) + (projectionA[i].Y * axis[i].Y));
                    aValues[k] = scalarValue;
                }

                //Calculate the projection of the second object
                Vector2[] projectionB = new Vector2[4];
                for (int k = 0; k < bCorners.Length; ++k)
                {
                    //Calculate the projection
                    double proj = ((bCorners[k].X * axis[i].X) + (bCorners[k].Y * axis[i].Y)) / (Math.Pow(axis[i].X, 2) + Math.Pow(axis[i].Y, 2));
                    projectionB[i] = new Vector2((float)proj * axis[i].X, (float)proj * axis[i].Y);

                    //Calculate a scalar value of each projection and add it to the list of projections
                    float scalarValue = ((projectionB[i].X * axis[i].X) + (projectionB[i].Y * axis[i].Y));
                    bValues[k] = scalarValue;
                }

                //For the first object, store the largest scalar projection
                for (int k = 0; k < aValues.Length; ++k)
                {
                    if (aValues[k] > maxA)
                    {
                        maxA = aValues[k];
                    }
                }
                //For the first object, store the smallest scalar projection
                for (int k = 0; k < aValues.Length; ++k)
                {
                    if (aValues[k] < minA)
                    {
                        minA = aValues[k];
                    }
                }
                //For the second object, store the largest scalar projection
                for (int k = 0; k < bValues.Length; ++k)
                {
                    if (bValues[k] > maxB)
                    {
                        maxB = bValues[k];
                    }
                }
                //For the second object, store the smallest scalar projection
                for (int k = 0; k < bValues.Length; ++k)
                {
                    if (bValues[k] < minB)
                    {
                        minB = bValues[k];
                    }
                }

                //Check for collision. If there is no collision at any point the scalar values do not intersect on any axis.
                if (minB <= maxA && maxB >= minA)
                {
                    //Collision occured, so check next axis
                    continue;
                }
                else
                {
                    return false;
                }
            }

            //If there is an intersection of the projections on every axis, than a collision exists
            return true;
        }

        #endregion

        /// <summary>
        /// Update the whoopee cushion's player location and then call each tile's update program to update any information they need
        /// </summary>
        public void Update()
        {
            //Update every whoopee cushion's player location and then call 
            for (int i = 0; i < listTiles.Count; ++i)
            {
                if (listTiles[i] is WCushion)
                {
                    ((WCushion)listTiles[i]).SetPlayer = player;
                }
                //Call each tile's own update to modify information
                listTiles[i].Update();
            }
        }

        /// <summary>
        /// Save the level into the bin folder of the game directory with the given name
        /// </summary>
        /// <param name="path">A string representing the name of the file that is saved</param>
        public void SaveLevel(string path)
        {
            //Use the streamwriter to write to the given path
            using (StreamWriter sw = new StreamWriter(path))
            {
                //Write out the length (in rows) of the game
                sw.WriteLine(tiles.Length);

                //For each line of tiles
                for (int i = 0; i < tiles.Length; ++i)
                {
                    //Write out the row from the geerated map
                    sw.WriteLine(tiles[i]);

                    //Write out the row from the generated map
                    //sw.Write(sw.NewLine);
                }
            }
        }

        /// <summary>
        /// Load the level from the given path and save it for later use
        /// </summary>
        /// <param name="path">A string representing the name of the map in the bin folder of the game</param>
        /// <param name="content">The content manager to add all the tiles to the game</param>
        public void LoadLevel(string path, ContentManager content)
        {
            listTiles = new List<MapTile>();

            //Use the streamreader to read from the given path
            using (StreamReader sr = new StreamReader(path))
            {
                //Store the length (in rows) of the map and calculate it's height
                int length = Convert.ToInt32(sr.ReadLine());
                gameHeight = length * TILESIZE;

                //Read in each line (in rows) from the file and call the method to store the tiles
                tiles = new string[length];
                for (int i = 0; i < length; ++i)
                {
                    tiles[i] = sr.ReadLine();
                    UpdateWorld(i, tiles[i], content);
                }
            }
        }

        /// <summary>
        /// Add each tile to the game based on the line given.
        /// </summary>
        /// <param name="lineNum">An integer value representing the current line number for tile height purposes</param>
        /// <param name="line">The line read in by the file for storing as tiles</param>
        /// <param name="content">The content manager to load each tile and store its textures</param>
        public void UpdateWorld(int lineNum, string line, ContentManager content)
        {
            //Split up the line based on the comma.
            string[] s = line.Split(',');

            //Set the width of the game to the length of the line
            gameWidth = s.Length * TILESIZE;

            //Add each tile from the line being read
            int tileColoumn = 0;
            for (int i = 0; i < s.Length; ++i)
            {
                //If the tile is 1 number (not an angle)
                if (s[i].Length == 1)
                {
                    switch (s[i])
                    {
                        //Add each tile and create its location (the coloumn and row each multiplied by the tilesize)
                        case "1":
                            listTiles.Add(new Trampoline(new Vector2(tileColoumn * TILESIZE, lineNum * TILESIZE), content, Convert.ToInt32(s[i + 1])));
                            ++tileColoumn;
                            ++i;
                            break;
                        case "2":
                            listTiles.Add(new Bubble(new Vector2(tileColoumn * TILESIZE, lineNum * TILESIZE), content));
                            ++tileColoumn;
                            break;
                        case "3":
                            listTiles.Add(new Spike(new Vector2(tileColoumn * TILESIZE, lineNum * TILESIZE), content, Convert.ToInt32(s[i + 1])));
                            ++i;
                            ++tileColoumn;
                            break;
                        case "4":
                            listTiles.Add(new WCushion(new Vector2(tileColoumn * TILESIZE, lineNum * TILESIZE), content, Convert.ToInt32(s[i + 1]), player));
                            ++i;
                            ++tileColoumn;
                            break;
                        case "5":
                            listTiles.Add(new Apple(new Vector2(tileColoumn * TILESIZE, lineNum * TILESIZE), content));
                            ++tileColoumn;
                            break;
                        case "6":
                            listTiles.Add(new Brick(new Vector2(tileColoumn * TILESIZE, lineNum * TILESIZE), content));
                            ++tileColoumn;
                            break;
                        default:
                            ++tileColoumn;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Check if there is a collision between a rectangle and a point
        /// </summary>
        /// <param name="a">A rectangle representing the player bounds</param>
        /// <param name="b">A point on the screen representing either a midpoint or a corner of the tile</param>
        /// <param name="isMidpoint">A boolean value whether or not the point is a midpoint</param>
        /// <returns></returns>
        private bool SurfaceCollision(Rectangle a, Vector2 b, bool isMidpoint)
        {
            //Create a variable to hold the width and height of the player location based on the type of point (corner or midpoint)
            int aWidth;
            int aHeight;
            if (isMidpoint)
            {
                aWidth = a.X + (int)(a.Width * 0.5f);
                aHeight = a.Y + (int)(a.Height * 0.5f);
            }
            else
            {
                aWidth = a.X + a.Width;
                aHeight = a.Y + a.Height;
            }

            //Check to see if the rectangle contains the point and return the outcome (true, collision, or false, no collision)
            if (b.X >= a.X - 4 && b.X <= aWidth)
            {
                if (b.Y <= aHeight && b.Y >= a.Y - 4)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Draw all the tiles to the screen
        /// </summary>
        /// <param name="sb">The SpriteBatch that is used to draw the textures</param>
        public void Draw(SpriteBatch sb)
        {
            //Draw every tile to the screen
            for (int i = 0; i < listTiles.Count; ++i)
            {
                listTiles[i].Draw(sb);
            }
        }
    }
}
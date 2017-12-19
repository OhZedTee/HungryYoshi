using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HungryYoshi.Controller.Kinect;
using HungryYoshi.Models.Objects;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using HungryYoshi.Controller.Computer;
using Camera2D_XNA4;

namespace HungryYoshi.Models
{
    class World
    {
        //Objects
        KinectManager kinect;
        MouseController mouse;
        KeyController keyboard;
        Player player;
        SlingShot slingshot;
        Map map;
        Pointer pointer;
        SpriteFont font;
        ContentManager content;
        Cam2D cam;
        Button unPause;
        Button returnToMenu;
        Designer levelDesigner;
        Menu menu;

        //Background Property
        public Sprite Background { get; set; }

        //General Properties
        int screenWidth;
        int screenHeight;
        int gameWidth;
        int gameHeight;
        public string Level { get; set; }
        public static string GameOutcome { get; set; }

        //World States
        public static States worldState = States.menu;
        public enum States : byte
        {
            menu,
            play,
            paused,
            zoom,
            designer,
            finished,
            reset
        };

        public World(int screenWidth, int screenHeight, ContentManager content, Cam2D cam)
        {
            this.screenHeight = screenHeight;
            this.screenWidth = screenWidth;
            this.content = content;
            this.cam = cam;
        }

        /// <summary>
        /// Initialize the world and set up the game
        /// </summary>
        public void Initialize(string path)
        {
            //Create the controllers
            kinect = new KinectManager(gameHeight);
            mouse = new MouseController();
            keyboard = new KeyController();

            //Make sure there is a Kinect connected
            if (!kinect.Initialize())
            {
                Console.WriteLine("No Kinect Sensor Connected!");
                Environment.Exit(0);
            }

            //Load the map and retrieve the dimensions
            map = new Map();
            map.LoadLevel(@"Levels\" + path + ".txt", content);
            gameHeight = map.GameDimensions[0];
            gameWidth = map.GameDimensions[1];
            kinect.GameHeight = gameHeight;

            //Create the objects and set up the world
            slingshot = new SlingShot(content, kinect, gameHeight);
            map.SetSlingShot = slingshot;
            player = new Player(kinect, mouse, content, slingshot);
            map.SetPlayer = player;
            slingshot.PlayerPos = player.GetSprite.GetPosition;
            pointer = new Pointer(content, kinect, mouse);

            //Create the Pause Button
            font = content.Load<SpriteFont>("pauseFont");
            SpriteFont buttonFont = content.Load<SpriteFont>("buttonFont");
            string output = "Play";
            Vector2 textSize = buttonFont.MeasureString(output);
            unPause = new Button(new Vector2((screenWidth * 0.5f) - (textSize.X * 0.5f), screenHeight * 0.5f + textSize.Y), output, buttonFont, content, pointer);
            output = "Return To Menu";
            textSize = buttonFont.MeasureString(output);
            returnToMenu = new Button(new Vector2((screenWidth * 0.5f) - (textSize.X * 0.5f), unPause.GetSprite.GetPosition.Y + 50), output, buttonFont, content, pointer);

            //Set up the background
            Texture2D bg = content.Load<Texture2D>("Background");
            Background = new Sprite(new Rectangle(0, 0, bg.Width, screenHeight), bg);

            //Set up the designer & menu
            levelDesigner = new Designer(kinect, content, pointer, screenWidth, screenHeight);
            menu = new Menu(content, pointer, screenWidth, screenHeight, this);


            // Set up all of your camera options
            //Set the bounding rectangle around the whole World
            cam.SetLimits(new Rectangle(0, 0, gameWidth, gameHeight));
            //Set the maximum factor to zoom in by, 3.0 is the default
            cam.SetZoom(1.0f);
            cam.SetMaxZoom(4.0f);
            //Set the starting World position of the camera, The below code sets it to the exact centre of the default screen size
            cam.SetPosition(player.GetSprite.GetPosition);
            //Set the origin of the camera, typically is the starting position of the camera
            cam.SetOrigin(cam.GetPosition());
            //centre the camera on the player
            cam.LookAt(player.GetSprite.GetBounds);

            //If the game is reseting
            if (worldState == States.reset)
            {
                //Start the game
                worldState = States.play;
            }
        }

        /// <summary>
        /// Reset the game if needed
        /// </summary>
        public void CheckReset()
        {
            //Load the level if the game needs to be reset
            if (Player.gameState == Player.States.reset)
            {
                //Load level and reset player
                map.LoadLevel(@"Levels\" + Level + ".txt", content);
                player.CancelGravity = false;
                player.InBubble = false;
            }
        }

        /// <summary>
        /// Load a level in the game
        /// </summary>
        public void LoadWorld()
        {
            //Reload the world and the entities in it.
            worldState = States.reset;
            Initialize(Level);
            player.Lives = 5;
            Player.gameState = Player.States.setup;
        }

        /// <summary>
        /// Update the world depending on the state.
        /// </summary>
        public void UpdateWorld()
        {
            //Check if anything was inputted in the keyboard
            if (keyboard.CheckInput())
            {
                switch (keyboard.KeyCommand)
                {
                        //Change states/data based on specific input from the keyboard
                    case "Escape":
                        Environment.Exit(0);
                        break;
                    case " ":
                        Player.gameState = Player.States.shooting;
                        break;
                    case "R":
                        Player.gameState = Player.States.reset;
                        break;
                    case "P":
                        if (worldState == States.play)
                        {
                            worldState = States.paused;
                        }
                        else if (worldState == States.paused)
                        {
                            worldState = States.play;
                        }
                        break;
                    case "B":
                        player.PoppedBubble = true;
                        break;
                    case "D":
                        worldState = States.designer;
                        break;
                    case "Z":
                        if (worldState != States.zoom && worldState == States.play)
                        {
                            worldState = States.zoom;
                        }
                        else if(worldState == States.zoom)
                        {
                            worldState = States.play;
                        }
                        break;
                    case "Up":
                        if (worldState == States.zoom)
                        {
                            float zoom = cam.GetZoom();
                            zoom += 0.1f;
                            cam.SetZoom(zoom);
                        }
                        break;
                    case "Down":
                        if (worldState == States.zoom)
                        {
                            float zoom = cam.GetZoom();
                            zoom -= 0.1f;
                            cam.SetZoom(zoom);
                        }
                        break;
                }
            }

            //If the world is in the menu
            if (worldState == States.menu)
            {
                //Set the background of the game to be the menu screen
                Texture2D bg = content.Load<Texture2D>("MenuBackground");
                Background.Texture = bg;

                //If the menu is at the start screen
                if (Menu.menuState == Menu.States.start)
                {
                    //Check to see if anything is clicked
                    menu.CheckClick();
                }

                //Update the menu
                menu.Update();
            }

            //If the player wants to zoom in the game
            if (worldState == States.zoom)
            {
                //Change the zoom based on the up and down keys
                if (keyboard.KeyCommand == "Up")
                {
                    //Increase zoom
                    float zoom = cam.GetZoom();
                    zoom += 0.1f;
                    cam.SetZoom(zoom);
                }
                else if (keyboard.KeyCommand == "Down")
                {
                    //Decrease zoom
                    float zoom = cam.GetZoom();
                    zoom -= 0.1f;
                    cam.SetZoom(zoom);
                }
                else
                {
                    //If the Kinect is being used
                    if (kinect.KinectController)
                    {
                        //Calculate the zoom factor from each update where the hand is moved
                        float scalarValue = pointer.GetSprite.GetBounds.Y;
                        scalarValue = scalarValue * 0.003f;
                        cam.SetZoom(scalarValue);
                    }
                }
            }

                    //If the player is playing:
               if(worldState == States.play || worldState == States.zoom)
               {
                    //Check if the world needs to be reset
                    CheckReset();

                    //Update the controller being used.
                    player.KinectController = kinect.KinectController;

                    //Update the player pass it off the the slingshot
                    player.Update();
                    slingshot.PlayerPos = player.GetSprite.GetPosition;

                    //Check what state the player is in the game
                    if (Player.gameState == Player.States.setup)
                    {
                        //Set the camera to look at the slingshot
                        cam.LookAt(slingshot.GetSprite.GetBounds);
                        //Make sure the player stays within the boundaries
                        map.SetupBoundary();
                    }
                    else if (Player.gameState == Player.States.flying)
                    {
                        //Set the camera to look at the player
                        cam.LookAt(player.GetSprite.GetBounds);
                        //Check collision with the game boundaries and the player
                        BoundaryCollision(player.GetSprite);

                        //Update the bubbles in the map and check if there is any collision between the player and the tiles in the map
                        map.BubbleUpdate();
                        map.CheckPossibleCollision();
                    }

                    //Update the map
                    map.Update();

               }
                    //If the player is in the designer
               else if (worldState == States.designer)
               {
                   //Check to see if any of the buttons were clicked and update the designer
                   menu.ReturnClick();
                   levelDesigner.CheckClick();
                   levelDesigner.Update(content);

               }

            //If the player is using the kinect or is not currently playing
            if (player.KinectController || worldState != States.play)
            {
                //Update the pointer and make sure its within the boundaries of the screen
                pointer.Update();
                BoundaryCollision(pointer.GetSprite);
            }
        }

        /// <summary>
        /// Draw the world to the screen
        /// </summary>
        /// <param name="sb">The SpriteBatch used to draw the textures</param>
        public void Draw(SpriteBatch sb)
        {
            switch (worldState)
            {
                    //If the world is in the zoom state:
                case States.zoom:
                    //Draw the map, player, and slingshot as normally
                    map.Draw(sb);
                    player.Draw(sb);
                    slingshot.Draw(sb);
                    break;
                    
                    //If the world is in the play state:
                case States.play:
                    //Draw the map, player and slingshot
                    map.Draw(sb);
                    player.Draw(sb);
                    slingshot.Draw(sb);
                    break;

                    //If the world is in the menu
                case States.menu:
                        //Draw the menu
                        menu.Draw(sb);
                        break;

                    //If the player finished the level:
                case States.finished:

                    //Draw the map, player, and slingshot
                    map.Draw(sb);
                    player.Draw(sb);
                    slingshot.Draw(sb);

                    //Write out to the screen the result of the game
                    Vector2 size = font.MeasureString(GameOutcome);
                    Sprite text = new Sprite(new Rectangle((int)(screenWidth * 0.5f) - (int)(size.X * 0.5f), (int)cam.GetPosition().Y - (int)size.Y, (int)size.X, (int)size.Y), null);
                    text.DrawString(sb, GameOutcome, font, Color.Black, text.GetPosition);

                    //Draw the button to the screen and check to see if it was clicked
                    returnToMenu.Draw(sb);
                    returnToMenu.CheckClick();

                    //Check if the button was clicked
                    if (returnToMenu.IsClicked)
                    {
                        //Return to the menu
                        worldState = States.menu;
                        Menu.menuState = Menu.States.start;
                        returnToMenu.IsClicked = false;
                    }
                    break;

                    //If the player paused the game
                case States.paused:

                    //Output the the screen that the game is paused
                    string output = "PAUSED";
                    Vector2 textSize = font.MeasureString(output);
                    Sprite draw = new Sprite(new Rectangle((int)(screenWidth * 0.5f) - (int)(textSize.X * 0.5f), (int)(screenHeight * 0.5f) - (int)textSize.Y, (int)textSize.X, (int)textSize.Y), null);
                    draw.DrawString(sb, output, font, Color.Black, draw.GetPosition);

                    //Draw the button the the screen and check to see if it was clicked
                    unPause.Draw(sb);
                    unPause.CheckClick();

                    //Check if the button was clicked
                    if (unPause.IsClicked)
                    {
                        //Continue the game
                        worldState = States.play;
                        unPause.IsClicked = false;
                    }

                    //Draw the button the the screen and check to see if it was clicked
                    returnToMenu.Draw(sb);
                    returnToMenu.CheckClick();

                    //Check if the button was clicked
                    if (returnToMenu.IsClicked)
                    {
                        //Return to the menu
                        worldState = States.menu;
                        Menu.menuState = Menu.States.start;
                        returnToMenu.IsClicked = false;
                    }
                    break;

                    //If the player is making a level
                case States.designer:

                    //Draw the designer && back button
                    levelDesigner.Draw(sb);
                    menu.DrawBack(sb);
                    break;
            }

            //If the game is not being played:
            if (worldState != States.play)
            {
                //Draw the pointers
                pointer.Draw(sb);
                unPause.DrawClick(sb);
                menu.DrawHover(sb);
                levelDesigner.DrawHover(sb);
               
            }
        }

        /// <summary>
        /// Make sure that the object is within the boundaries of the screen.
        /// </summary>
        /// <param name="sprite">A sprite object representing the location of the object being tested for a collision</param>
        private void BoundaryCollision(Sprite sprite)
        {
            //Make sure the object is passed the left side of the screen
            if (sprite.GetBounds.X < 0)
            {
                sprite.SetBounds("X", 0);
            }
            if (sprite.GetBounds.Y < 0)
            {
                sprite.SetBounds("Y", 0);

                //Check if the game is being played, and if so the player is hitting the top.
                if (worldState == States.play)
                {
                    player.HitTop = true;
                }
            }

            //Preform collision detection with the game width if the player is playing a level
            if (worldState == States.play)
            {
                //Make sure the object is not passed the right side of the screen
                if (sprite.GetBounds.X > gameWidth - sprite.GetBounds.Width)
                {
                    sprite.SetBounds("X", gameWidth - sprite.GetBounds.Width);
                }

                if (sprite.GetBounds.Y > gameHeight - sprite.GetBounds.Height)
                {
                    sprite.SetBounds("Y", gameHeight - sprite.GetBounds.Height);
                }
            }
            else
            {
                //Make sure the object is passed the right side of the screen
                if (sprite.GetBounds.X > screenWidth - sprite.GetBounds.Width)
                {
                    sprite.SetBounds("X", screenWidth - sprite.GetBounds.Width);
                }

                if (sprite.GetBounds.Y > screenHeight - sprite.GetBounds.Height)
                {
                    sprite.SetBounds("Y", screenHeight - sprite.GetBounds.Height);
                }
            }
        }
    }
}
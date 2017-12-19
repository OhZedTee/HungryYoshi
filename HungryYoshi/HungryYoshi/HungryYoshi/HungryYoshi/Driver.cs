using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;
using HungryYoshi.Models;
using Camera2D_XNA4;

namespace HungryYoshi
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Driver : Microsoft.Xna.Framework.Game
    {
        //Graphics
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //View properties
        int screenWidth;
        int screenHeight;

        //The Frame Rate
        public const int REFRESH_RATE = 60;
        public static bool ResetAll { get; set; }

        //Objects
        World world;
        Cam2D cam;

        public Driver()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            //Remove the exit button from the top right corner of the game
                Application.EnableVisualStyles();
                Form gameForm = (Form)Form.FromHandle(Window.Handle);
                gameForm.FormBorderStyle = FormBorderStyle.None;

            //Set the Properties of the Graphics Model
            graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferWidth = 1080;
            graphics.PreferredBackBufferHeight = 640;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            //Save the screen dimensions
            screenHeight = graphics.PreferredBackBufferHeight;
            screenWidth = graphics.PreferredBackBufferWidth;
                        
            //Initialize the camera object with the viewport
            cam = new Cam2D(GraphicsDevice.Viewport);

            //Initialize the world and create the world set up all required data
            world = new World(screenWidth, screenHeight, Content, cam);
            world.Initialize("map1");

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                this.Exit();

            //Update the world and all objects in the world
            world.UpdateWorld();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Draw the background to the screen
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp, null, null);
            spriteBatch.Draw(world.Background.Texture, world.Background.GetBounds, Color.White);
            spriteBatch.End();

            //Draw the game with a camera as long as the game is in play mode
            if (World.worldState == World.States.play || World.worldState == World.States.zoom || World.worldState == World.States.finished)
            {
                //Draw the world with the camera
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, cam.GetTransformation());
                world.Draw(spriteBatch);
                spriteBatch.End();
            }
            else
            {
                //Draw the world without the camera
                spriteBatch.Begin();
                world.Draw(spriteBatch);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
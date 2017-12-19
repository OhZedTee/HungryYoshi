using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HungryYoshi.Controller.Kinect;
using HungryYoshi.Models.Objects;
using Microsoft.Kinect;
using HungryYoshi.Controller.Computer;
using HungryYoshi.Models.Particle_System;

namespace HungryYoshi.Models
{
    class Player
    {
        //Objects
        private Sprite sprite;
        private KinectManager kinect;
        private MouseController mouse;
        private bool isKinectController = true;
        private Texture2D texture;
        private SlingShot slingshot;
        private ParticleEngine particleEngine;

        //Motion
        private double distance;
        private double angle;
        private float vel;
        private Vector2 traj;
        private Vector2 startingTraj;
        private const float GRAVITY = 9.81f / Driver.REFRESH_RATE;
        private float scale = 2.0f;
        private int dir = 1;
        private Vector2 destination;

        //Collision Detection
        int radius;
        Vector2[] corners = new Vector2[4];
        public bool HitTop { get; set; }
        public bool CancelGravity { get; set; }
        public bool NoParticles { get; set; }

        //Properties
        bool isInBubble = false;
        int lives = 5;
        //State of the player
        public static States gameState = States.setup;
        public enum States : byte
        {
            noplayer,
            setup,
            shooting,
            flying,
            reset
        };

        public Player(KinectManager kinect, MouseController mouse, ContentManager content, SlingShot slingshot)
        {
            this.kinect = kinect;
            this.mouse = mouse;
            this.slingshot = slingshot;
            texture = content.Load<Texture2D>("Player");
            sprite = new Sprite(new Rectangle(texture.Width * 2, (slingshot.GetSprite.GetBounds.Y + slingshot.GetSprite.GetBounds.Height) - texture.Height, texture.Width, texture.Height), texture);

            //Store the radius of the player for collision detection
            if (sprite.GetBounds.Width > sprite.GetBounds.Height)
            {
                radius = sprite.GetBounds.Width + 2;
            }
            else
            {
                radius = sprite.GetBounds.Height + 2;
            }

            //Calculate the corners of the player
            CalcCorners();

            //Set up particle engine
            List<Texture2D> textures = new List<Texture2D>();
            textures.Add(content.Load<Texture2D>("circle"));
            textures.Add(content.Load<Texture2D>("star"));
            textures.Add(content.Load<Texture2D>("diamond"));
            particleEngine = new ParticleEngine(textures, new Vector2(400, 240));
        }

        /// <summary>
        /// Property to retrieve the player sprite
        /// </summary>
        public Sprite GetSprite
        {
            get { return sprite; }
        }

        /// <summary>
        /// Property to get and set the amount of lives that the player has
        /// </summary>
        public int Lives
        {
            get { return lives; }
            set { lives = value; }
        }

        /// <summary>
        /// Property used to check whether or not the player is in a bubble for trajectory calculations
        /// </summary>
        public bool InBubble
        {
            get { return isInBubble; }
            set { isInBubble = value; }
        }

        /// <summary>
        /// Property to check whether the player is inside a bubble or not
        /// </summary>
        public bool PoppedBubble
        {
            get { return kinect.Popped; }
            set { kinect.Popped = value; }
        }

        /// <summary>
        /// Property to retrieve the radius of the player for collision detection
        /// </summary>
        public int GetRadius
        {
            get { return radius; }
        }

        /// <summary>
        /// Property to get or set the player's trajectory
        /// </summary>
        public Vector2 Trajectory
        {
            get { return traj; }
            set { traj = value; }
        }

        /// <summary>
        /// Property to get the player's velocity
        /// </summary>
        public float Velocity
        {
            get { return vel; }
            set { vel = value; }
        }

        /// <summary>
        /// Property to retrieve the player's angle
        /// </summary>
        public double Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        /// <summary>
        /// Property to get the player's current direction
        /// </summary>
        public int Direction
        {
            get { return dir; }
            set { dir = value; }
        }

        /// <summary>
        /// Property to get the player's corners
        /// </summary>
        public Vector2[] GetCorners
        {
            get { return corners; }
        }

        /// <summary>
        /// Property to maintain the controller being used for the game
        /// </summary>
        public bool KinectController
        {
            get { return isKinectController; }
            set { isKinectController = value; }
        }

        /// <summary>
        /// Calculates the angle between two points.
        /// </summary>
        /// <param name="a">The destination point</param>
        /// <param name="b">The point from which the angle is calculated</param>
        /// <returns></returns>
        public double CalcAngle(Vector2 a, Vector2 b)
        {
            //Calculate the differences between the points and then return the angle
            float xDiff = a.X - b.X;
            float yDiff = a.Y - b.Y;
            return Math.Atan2(yDiff, xDiff);
        }

        /// <summary>
        /// Calculate the rotated points from each corner of the player
        /// </summary>
        public void CalcCorners()
        {
            //Create the corners for the axis aligned player
            corners[(int)MapTile.Corners.topLeft] = new Vector2(sprite.GetBounds.Left - (sprite.GetBounds.Width / 3 + 1), sprite.GetBounds.Top - (sprite.GetBounds.Height / 3 + 1));
            corners[(int)MapTile.Corners.topRight] = new Vector2(sprite.GetBounds.Right - (sprite.GetBounds.Width * 0.5f + sprite.GetBounds.Width / 6), sprite.GetBounds.Top - (sprite.GetBounds.Height / 3 + 1));
            corners[(int)MapTile.Corners.bottomLeft] = new Vector2(GetSprite.GetBounds.Left - (sprite.GetBounds.Width / 3 + 1), sprite.GetBounds.Bottom - (sprite.GetBounds.Height * 0.5f + sprite.GetBounds.Height / 5 - 1));
            corners[(int)MapTile.Corners.bottomRight] = new Vector2(sprite.GetBounds.Right - (sprite.GetBounds.Width * 0.5f + sprite.GetBounds.Width / 6), sprite.GetBounds.Bottom - (sprite.GetBounds.Height * 0.5f + sprite.GetBounds.Height / 5 - 1));

            for (int i = 0; i < corners.Length; ++i)
            {
                //Create pivot point and move the points back by the pivot
                Vector2 pivot = new Vector2(sprite.GetBounds.X, sprite.GetBounds.Y);
                corners[i].X = corners[i].X - pivot.X;
                corners[i].Y = corners[i].Y - pivot.Y;

                //Rotate the points
                Vector2 newCorner;
                newCorner.X = (float)(corners[i].X * Math.Cos(MathHelper.ToRadians((float)angle)) - corners[i].Y * Math.Sin(MathHelper.ToRadians((float)angle)));
                newCorner.Y = (float)(corners[i].X * Math.Sin(MathHelper.ToRadians((float)angle)) + corners[i].Y * Math.Cos(MathHelper.ToRadians((float)angle)));

                //Reset the points to the new rotated location and add the pivot back
                corners[i].X = newCorner.X + pivot.X;
                corners[i].Y = newCorner.Y + pivot.Y;
            }
        }

        /// <summary>
        /// Update the player's properties and the player's location based on the state it is in
        /// </summary>
        public void Update()
        {
            switch (gameState)
            {
                //If the player is in the reset state:
                case States.reset:

                    if (Lives > 1)
                    {
                        //Reset the player location
                        sprite.SetBounds("X", lives * sprite.Texture.Height);
                        sprite.SetBounds("Y", (slingshot.GetSprite.GetBounds.Y + slingshot.GetSprite.GetBounds.Height) - sprite.Texture.Height);

                        //Reset the player properties and change the state to setup
                        --Lives;
                        mouse.IsLeftClicked = false;
                        Direction = 1;
                        CancelGravity = false;
                        gameState = States.setup;
                    }
                    else
                    {
                        World.GameOutcome = "LEVEL FAILED!";
                        World.worldState = World.States.finished;
                    }
                    break;
                //If the player is in the setup state
                case States.setup:

                    //Update the player location based on the controller being used
                    if (isKinectController)
                    {
                        SkeletonPoint leftHand = kinect.ScaledHand("left").Position;
                        sprite.SetPosition(new Vector2(leftHand.X, leftHand.Y));
                    }
                    else
                    {
                        //Update the player's location or shoot the player if the left mouse button is clicked
                        if (mouse.IsLeftClicked)
                        {
                            gameState = States.shooting;
                        }
                        else
                        {
                            Vector2 mousePos = mouse.CheckInput();

                            //Make sure the player is within boundaries
                            if (mousePos.X < 16)
                            {
                                mousePos.X = 16;
                            }
                            else if (mousePos.Y < 16)
                            {
                                mousePos.Y = 16;
                            }
                            else if (mousePos.X > slingshot.GetPosition.X - sprite.GetBounds.Width)
                            {
                                mousePos.X = slingshot.GetPosition.X - sprite.GetBounds.Width;
                            }
                            else if (mousePos.Y > (slingshot.GetPosition.Y + slingshot.GetSprite.GetBounds.Height) - sprite.GetBounds.Height - sprite.GetBounds.Height * 0.5f)
                            {
                                mousePos.Y = (slingshot.GetPosition.Y + slingshot.GetSprite.GetBounds.Height) - sprite.GetBounds.Height - sprite.GetBounds.Height * 0.5f;
                            }

                            //Set the player's location to the mouse position
                            sprite.SetPosition(mousePos);
                        }
                    }

                    //Calculate the distance between the slingshot and the player
                    float xDiff = slingshot.GetPosition.X - sprite.GetPosition.X;
                    float yDiff = slingshot.GetPosition.Y - sprite.GetPosition.Y;
                    distance = Math.Sqrt(Math.Pow(xDiff, 2) + Math.Pow(yDiff, 2)) * scale;

                    //Calculate the velocity based on the distance
                    vel = (float)distance / (Driver.REFRESH_RATE / scale);

                    //Calculate the angle based on the player's position relative to the slingshot
                    angle = CalcAngle(slingshot.GetPosition, sprite.GetPosition);

                    //Call the procedure to calculate the player's rotated corners
                    CalcCorners();
                    break;
                //If the player is in the shooting state
                case States.shooting:

                    //Calculate the player's starting trajectory based on the angle, direction, and velocity calculated
                    startingTraj = CalcTrajectory(angle, dir, vel);

                    //Store the starting trajectory calculated to be used while the player is flying
                    traj = startingTraj;

                    //Calculate the player's destination for rotation during air time
                    float time = (float)Math.Sqrt(Math.Abs((2 * startingTraj.Y) / (GRAVITY * Driver.REFRESH_RATE)));
                    destination.X = (startingTraj.X * time) + sprite.GetPosition.X;
                    destination.Y = sprite.GetPosition.Y;
                    //Set the player's state to flying since all required properties and trajectories have been calcuated
                    gameState = States.flying;
                    break;
                //If the player is in the flying state
                case States.flying:

                    //Check if the player is in the bubble and change his trajectory based on that
                    if (isInBubble)
                    {
                        //Change the player's trajectory so that he floats and make sure no particles appear
                        NoParticles = true;
                        traj.Y -= GRAVITY / (scale * scale);
                    }
                    else
                    {
                        //Check whether or not the gravity was canceled
                        if (!CancelGravity)
                        {
                            //Increment the player's trajectory by gravity
                            traj.Y += GRAVITY;
                        }
                        else
                        {
                            //Make sure the player has no Y trajectory
                            traj.Y = 0;
                        }
                    }

                    //Update the player's particle emittor location and the particles themselves
                    particleEngine.EmitterLocation = sprite.GetPosition;
                    particleEngine.Update();

                    //Check to see if new particles should be generated
                    if (NoParticles)
                    {
                        //Change the particle engine's porperties so that no new particles are created
                        particleEngine.Generate = false;
                    }
                    else
                    {
                        //Continue generating new particles as the player flies around
                        particleEngine.Generate = true;
                    }

                    //Update the player's location based on the trajectory
                    sprite.SetPosition(sprite.GetPosition + (traj * scale));

                    //If the player hit the top of the screen, reset it's trajectory so that it bounces off
                    if (HitTop)
                    {
                        gameState = States.shooting;
                    }

                    //angle = CalcAngle(destination, sprite.GetPosition);

                    //Check if the player might still move, if not the level is failed
                    if (traj == Vector2.Zero)
                    {
                        gameState = States.reset;
                    }

                    //Recalculate the player's rotated corner's for future collision detection
                    CalcCorners();
                    break;
            }
        }

        /// <summary>
        /// Calculate the player's trajectory based on the given angle, direction, and velocity
        /// </summary>
        /// <param name="angle">A decimal number representing the player's angle in degrees.</param>
        /// <param name="dir">An integer value representing the direction in which the player is flying</param>
        /// <param name="vel">A decimal value representing the player's current velocity.</param>
        /// <returns>Returns a vector representing the amount of pixels the player should move in each direction (X & Y)
        /// on each update based on the given angle, direction, and velocity</returns>
        public Vector2 CalcTrajectory(double angle, int dir, float vel)
        {
            //If the player hit the top of the screen, switch over his Y direction so that be bounces off.
            if (HitTop)
            {
                //reset the property
                HitTop = false;
                //Return the calculated trajectory by which the player moves on each update.
                return new Vector2(vel * (float)Math.Cos(angle) * dir, vel * (float)Math.Sin(angle) * -dir);
            }

            //Return the calculated trajectory by which the player moves on each update.
            return new Vector2(vel * (float)Math.Cos(angle) * dir, vel * (float)Math.Sin(angle) * dir);
        }

        /// <summary>
        /// Draw the player onto the screen rotated depending on the given angle
        /// </summary>
        /// <param name="sb"></param>
        public void Draw(SpriteBatch sb)
        {
            //Depending on the state, draw the player slightly different
            if (gameState == States.setup)
            {
                //Draw the player with a rotation pivoted on the left center of the player and no calculated angle
                sprite.DrawRotation(sb, Color.White, slingshot.GetPosition, sprite.GetPosition, new Vector2(0, sprite.GetBounds.Height * 0.5f), false);
            }
            else if (gameState == States.flying)
            {
                //Draw the player with a rotation pivoted around the center of the player, a pre specifed angle, and then draw the particles behind the player
                sprite.DrawRotation(sb, Color.White, (float)angle, new Vector2(sprite.GetBounds.Width * 0.5f, sprite.GetBounds.Height * 0.5f));
                particleEngine.Draw(sb);
            }
            else
            {
                //Draw the player regularly if not in the specific states above
                sprite.Draw(sb);
            }

            //Draw the player's extra lives
            for (int i = 1; i < lives; ++i)
            {
                //Create the player's extra lives sprite's and place then to the left of the slingshot
                Sprite extraLives = new Sprite(new Rectangle((i - 1) * sprite.Texture.Width, (slingshot.GetSprite.GetBounds.Y + slingshot.GetSprite.GetBounds.Height) - sprite.Texture.Height, sprite.Texture.Width, sprite.Texture.Height), sprite.Texture);

                //Draw the extralives as an indication to the player as to the lives he has left for the level.
                extraLives.Draw(sb);
            }
        }
    }
}
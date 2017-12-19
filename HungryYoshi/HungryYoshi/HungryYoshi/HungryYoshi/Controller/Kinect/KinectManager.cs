using System;
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
using Microsoft.Speech;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.IO;
using HungryYoshi.Models;

namespace HungryYoshi.Controller.Kinect
{
    public class KinectManager
    {
        //Active Kinect Sensor
        private KinectSensor kinectSensor;

        //Speech Recognition
        SpeechRecognitionEngine speechEngine;

        //Joint Dictionary
        Dictionary<JointType, ColorImagePoint> skeletonPoints = new Dictionary<JointType, ColorImagePoint>();

        //Joint storig the hands
        Joint leftHand;
        Joint rightHand;

        //game height for scaling
        public int GameHeight { get; set; }

        //Is the kinect or mouse being used?
        bool isKinectController = false;

        //Bubble property
        public bool Popped { get; set; }            //Get whether or not the player wants the bubble popped

        //Designer properties
        public bool PlaceObject { get; set; }
        public bool Rotating { get; set; }

        public KinectManager(int gameHeight)
        {
            GameHeight = gameHeight;
        }

        /// <summary>
        /// Property to get or set whether the kinect is being used
        /// </summary>
        public bool KinectController
        {
            get { return isKinectController; }
            set { isKinectController = value; }
        }

        /// <summary>
        /// Scales the hand by a factor for easier movement
        /// </summary>
        /// <param name="handType">string representing hand being used</param>
        /// <returns>Joint with scaled position</returns>
        public Joint ScaledHand(string handType)
        {
            //Scale the hand based on the given hand joint
            if (handType == "left")
            {
                //Scale the left hand
                return ScaleTo(leftHand, 200, GameHeight, 0.3f, 0.3f);                
            }
            else
            {
                //Scale the right hand
                return ScaleTo(rightHand, 1080, 640, 0.2f, 0.2f);
            }
        }        

        /// <summary>
        /// Start the sensor, begin tracking, and start audio recognition algorithms on the kinect
        /// </summary>
        /// <returns>A boolean value representing the connection status of the kinect</returns>
        public bool Initialize()
        {
            //Check to see if kinect is connected
            if (KinectSensor.KinectSensors.Count == 0)
            {
                return false;
            }

            //Set up kinect and tracking
            kinectSensor = KinectSensor.KinectSensors[0];

            //Set the parameters for the kinect so it is easier to use
            TransformSmoothParameters parameters = new TransformSmoothParameters
            {
                Smoothing = 0.3f,
                Correction = 0,
                Prediction = 0,
                JitterRadius = 1,
                MaxDeviationRadius = 0.5f,
            };

            //Begin tracking skeleton and create event handler
            kinectSensor.SkeletonStream.Enable(parameters);
            kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_SkeletonFrameReady);

            if (World.worldState != World.States.reset)
            {
                //Start the sensor
                kinectSensor.Start();
            }

            //Find Kinect's Speech Recognition Information
            RecognizerInfo recKinect = null;
            for (int i = 0; i < SpeechRecognitionEngine.InstalledRecognizers().Count; ++i)
            {
                if (SpeechRecognitionEngine.InstalledRecognizers()[i].AdditionalInfo.ContainsKey("Kinect"))
                {
                    if (SpeechRecognitionEngine.InstalledRecognizers()[i].Culture.Name == "en-US")
                    {
                        if (SpeechRecognitionEngine.InstalledRecognizers()[i].AdditionalInfo["Kinect"] == "True")
                        {
                            recKinect = SpeechRecognitionEngine.InstalledRecognizers()[i];
                            break;
                        }
                    }
                }
            }
            if (recKinect == null)
            {
                return false;
            }

            //Initialize engine with recognition library
            speechEngine = new SpeechRecognitionEngine(recKinect);

            //Add commands the the kinect will look for
            Choices speech = new Choices();
            speech.Add("Fire", "Menu", "Up", "Down", "Exit", "Zoom", "Pause", "Play", "Kinect", "Computer", "Pop", "Place", "Rotate");

            //Add phrases into engine
            GrammarBuilder builder = new GrammarBuilder();
            builder.Culture = recKinect.Culture;
            builder.Append(speech);
            speechEngine.LoadGrammar(new Grammar(builder));

            //Set the Speech Engine to use Kinect's Microphones
            //Turn on microphones 
            KinectAudioSource source = kinectSensor.AudioSource;
            source.BeamAngleMode = BeamAngleMode.Adaptive;
            Stream audioStream = source.Start();
            //Stream sound to speech engine in compatible format
            speechEngine.SetInputToAudioStream(audioStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));

            //Event Handler
            speechEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(engine_SpeechRecognized);

            //Turn on Recognizer
            speechEngine.RecognizeAsync(RecognizeMode.Multiple);

            return true;
        }

        /// <summary>
        /// Event called when the kinect believes something was said (Audio was recognized).
        /// Event then begins operations based on audio recognized
        /// </summary>
        private void engine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //Check if kinect isn't at least 60% confident that the player said a command
            if (e.Result.Confidence < 0.60)
            {
                return;
            }

            //Process results
            switch (e.Result.Text)
            {
                    //Change game states based on user audio
                case "Fire":
                    Player.gameState = Player.States.shooting;
                    break;
                case "Menu":
                    World.worldState = World.States.menu;
                    break;
                case "Zoom":
                    if (World.worldState != World.States.zoom && World.worldState == World.States.play)
                    {
                        World.worldState = World.States.zoom;
                    }
                    else
                    {
                        World.worldState = World.States.play;
                    }
                    break;
                case "Pause":
                    World.worldState = World.States.paused;
                    break;
                case "Play":
                    World.worldState = World.States.play;
                    break;
                    //Choose controller
                case "Kinect":
                    isKinectController = true;
                    break;
                case "Computer":
                    isKinectController = false;
                    break;
                    //Player properties
                case "Pop":
                    Popped = true;
                    break;
                    //Designer Properties
                case "Place":
                    if (World.worldState == World.States.designer)
                    {
                        PlaceObject = true;

                        if (Rotating)
                        {
                            Rotating = false;
                        }

                    }
                    break;
                case "Rotate":
                    if(World.worldState == World.States.designer)
                    {
                            Rotating = true;
                    }
                    break;
                    //Quit the game
                case "Exit":
                    Environment.Exit(0);
                    break;
                case "Up":
                    //Increase Elevation of kinect sensor until it reaches its max
                    if (kinectSensor.ElevationAngle + 5 < kinectSensor.MaxElevationAngle)
                    {
                        kinectSensor.ElevationAngle += 5;
                    }
                    else
                    {
                        kinectSensor.ElevationAngle = kinectSensor.MaxElevationAngle;
                    }                    
                    break;
                case "Down":
                    //Decrease Elevation of Kinect sensor until it reaches its min
                    if (kinectSensor.ElevationAngle - 5 > kinectSensor.MinElevationAngle)
                    {
                        kinectSensor.ElevationAngle -= 5;
                    }
                    else
                    {
                        kinectSensor.ElevationAngle = kinectSensor.MinElevationAngle;
                    } 
                    break;
            }
        }

        /// <summary>
        /// Event called when the kinect believes new skeletal data exits (Player moved).
        /// Event then collects data on wanted joints
        /// </summary>
        private void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            //If the player is using the kinect controller
            if (isKinectController)
            {
                //Retrieve players
                SkeletonFrame frame = e.OpenSkeletonFrame();
                if (frame == null)
                {
                    return;
                }

                //Store joints from players
                Skeleton[] players = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(players);

                //Grab the first player that is being tracked
                Skeleton firstPlayer = null;
                for (int i = 0; i < players.Length; ++i)
                {
                    if (players[i].TrackingState == SkeletonTrackingState.Tracked || players[i].TrackingState == SkeletonTrackingState.PositionOnly)
                    {
                        firstPlayer = players[i];
                    }
                }

                //Dispose the non tracking frame
                if (firstPlayer == null)
                {
                    Player.gameState = Player.States.noplayer;
                    frame.Dispose();
                    return;
                }

                //If there was no player found in the game
                if (Player.gameState == Player.States.noplayer)
                {
                    //Change the state to setup
                    Player.gameState = Player.States.setup;
                }

                //Store the left and right joints for later scaling
                leftHand = firstPlayer.Joints[JointType.HandLeft];
                rightHand = firstPlayer.Joints[JointType.HandRight];

                //Remove previous points from Dictionary
                skeletonPoints.Clear();

                //Map specific joints
                int joint = 0;
                for (int i = 0; i < 2; ++i)
                {
                    //Update the left arm joint
                    if (i == 0)
                    {
                        joint = 7;
                    }

                    //Update the right arm joint
                    else
                    {
                        joint = 11;
                    }

                    //Retrienve the joint type
                    JointType type = ((JointType[])Enum.GetValues(typeof(JointType)))[joint];

                    //Map the joint point on the ColorImage
                    ColorImagePoint mappedPoints;
                    mappedPoints = kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(firstPlayer.Joints[type].Position,
                                                             ColorImageFormat.RgbResolution640x480Fps30);

                    //Add mapped Coordinate into dictionary
                    skeletonPoints.Add(type, mappedPoints);
                }
                frame.Dispose();
            }
        }

        /// <summary>
        /// Reset the joint location by scaling it
        /// </summary>
        /// <param name="joint">The joint to be scaled</param>
        /// <param name="width">The width of the bounding box</param>
        /// <param name="height">The height of the bounding box</param>
        /// <param name="maxSkeletonX">The X scale factor</param>
        /// <param name="maxSkeletonY">The Y scale factor</param>
        /// <returns>The scaled joint</returns>
        private Joint ScaleTo(Joint joint, int width, int height, float maxSkeletonX, float maxSkeletonY)
        {
            //Create a new skeleton point with the scaled position
            SkeletonPoint pos = new SkeletonPoint()
            {
                X = Scale(width, maxSkeletonX, joint.Position.X),
                Y = Scale(height, maxSkeletonY, -joint.Position.Y),
                Z = joint.Position.Z
            };

            //Set the joint to the scaled position
            joint.Position = pos;
            return joint;
        }
        
        /// <summary>
        /// Scale the coordinate position by the factor
        /// </summary>
        /// <param name="maxPixel">The amount of pixels in a direction of the bounding box</param>
        /// <param name="maxSkeleton">The scaling factor</param>
        /// <param name="position">The current position of the coordinate</param>
        /// <returns>Float representing the new location of the coordinate</returns>
        private float Scale(int maxPixel, float maxSkeleton, float position)
        {
            //Scale the position
            float value = ((((maxPixel / maxSkeleton) * 0.5f) * position) + (maxPixel * 0.5f));
            //Return the scaled factor within the bounding box
            if (value > maxPixel)
            {
                return maxPixel;
            }
            if (value < 0)
            {
                return 0;
            }
            return value;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace ForcesGame
{
    /// <summary>
    /// This is the main type for your application.
    /// </summary>
    public class App1 : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager graphics;
        private ContactTarget contactTarget;
        private Color backgroundColor = new Color(81, 81, 81);
        private bool applicationLoadCompleteSignalled;

        private UserOrientation currentOrientation = UserOrientation.Bottom;
        private Matrix screenTransform = Matrix.Identity;
        private Matrix inverted;

        // application state: Activated, Previewed, Deactivated,
        // start in Activated state
        private bool isApplicationActivated = true;
        private bool isApplicationPreviewed;

        private bool applyingEffectors = false;

        private int screenWidth;
        private int screenHeight;

        GameState gs = new GameState();
        FluidSimulation simulation;
        Texture2D arrowTexture;
        Texture2D particleTexture;
        Texture2D startMenu;

        SpriteFont timerFont;
        List<Particle> stream = new List<Particle>();
        Emitter emitter = new Emitter();
        private Dictionary<int, Effector> effectors = new Dictionary<int, Effector>();
        int maxLife = 1000;
        private int cellsX = 1024 / 32;
        private int cellsY = 768 / 32;
        private TimeSpan endTime;

        private bool isClear = true; // make sure all fingers come off of the start button before accepting a new start.

        SpriteBatch sb;
        /// <summary>
        /// The graphics device manager for the application.
        /// </summary>
        protected GraphicsDeviceManager Graphics
        {
            get { return graphics; }
        }

        /// <summary>
        /// The target receiving all surface input for the application.
        /// </summary>
        protected ContactTarget ContactTarget
        {
            get { return contactTarget; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public App1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        #region Initialization

        /// <summary>
        /// Moves and sizes the window to cover the input surface.
        /// </summary>
        private void SetWindowOnSurface()
        {
            System.Diagnostics.Debug.Assert(Window.Handle != System.IntPtr.Zero,
                "Window initialization must be complete before SetWindowOnSurface is called");
            if (Window.Handle == System.IntPtr.Zero)
                return;

            // We don't want to run in full-screen mode because we need
            // overlapped windows, so instead run in windowed mode
            // and resize to take up the whole surface with no border.

            // Make sure the graphics device has the correct back buffer size.
            InteractiveSurface interactiveSurface = InteractiveSurface.DefaultInteractiveSurface;
            if (interactiveSurface != null)
            {
                graphics.PreferredBackBufferWidth = interactiveSurface.Width;
                graphics.PreferredBackBufferHeight = interactiveSurface.Height;
                graphics.ApplyChanges();

                screenHeight = interactiveSurface.Height;
                screenWidth = interactiveSurface.Width;

                // Remove the border and position the window.
                Program.RemoveBorder(Window.Handle);
                Program.PositionWindow(Window);
            }
        }

        /// <summary>
        /// Initializes the surface input system. This should be called after any window
        /// initialization is done, and should only be called once.
        /// </summary>
        private void InitializeSurfaceInput()
        {
            System.Diagnostics.Debug.Assert(Window.Handle != System.IntPtr.Zero,
                "Window initialization must be complete before InitializeSurfaceInput is called");
            if (Window.Handle == System.IntPtr.Zero)
                return;
            System.Diagnostics.Debug.Assert(contactTarget == null,
                "Surface input already initialized");
            if (contactTarget != null)
                return;

            // Create a target for surface input.
            contactTarget = new ContactTarget(Window.Handle, EventThreadChoice.OnBackgroundThread);

            contactTarget.ContactAdded += new EventHandler<ContactEventArgs>(contactTarget_ContactAdded);
            contactTarget.ContactChanged += new EventHandler<ContactEventArgs>(contactTarget_ContactChanged);
            contactTarget.ContactRemoved += new EventHandler<ContactEventArgs>(contactTarget_ContactRemoved);

            contactTarget.EnableInput();
        }

        /// <summary>
        /// Reset the application's orientation and transform based on the current launcher orientation.
        /// </summary>
        private void ResetOrientation()
        {
            UserOrientation newOrientation = ApplicationLauncher.Orientation;

            if (newOrientation == currentOrientation) { return; }

            currentOrientation = newOrientation;

            if (currentOrientation == UserOrientation.Top)
            {
                screenTransform = inverted;
            }
            else
            {
                screenTransform = Matrix.Identity;
            }
        }

        #endregion

        #region Overridden Game Methods

        /// <summary>
        /// Allows the app to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            SetWindowOnSurface();
            InitializeSurfaceInput();

            // Set the application's orientation based on the current launcher orientation
            currentOrientation = ApplicationLauncher.Orientation;

            // Subscribe to surface application activation events
            ApplicationLauncher.ApplicationActivated += OnApplicationActivated;
            ApplicationLauncher.ApplicationPreviewed += OnApplicationPreviewed;
            ApplicationLauncher.ApplicationDeactivated += OnApplicationDeactivated;

            // Setup the UI to transform if the UI is rotated.
            // Create a rotation matrix to orient the screen so it is viewed correctly
            // when the user orientation is 180 degress different.
            inverted = Matrix.CreateRotationZ(MathHelper.ToRadians(180)) *
                       Matrix.CreateTranslation(graphics.GraphicsDevice.Viewport.Width,
                                                 graphics.GraphicsDevice.Viewport.Height,
                                                 0);

            if (currentOrientation == UserOrientation.Top)
            {
                screenTransform = inverted;
            }

            // srmoore additions  -- might want this in LoadContent?
            simulation = new FluidSimulation(cellsX, cellsY, graphics.PreferredBackBufferWidth / cellsX, graphics.PreferredBackBufferHeight / cellsY, graphics.GraphicsDevice);

            base.Initialize();
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {

            sb = new SpriteBatch(graphics.GraphicsDevice);
            // TODO: Load any content
            //Color[] particleTextureGrid = new Color[1];
            //particleTextureGrid[0] = Color.White;
            //particleTexture = new Texture2D(graphics.GraphicsDevice, 1, 1, 0, TextureUsage.None, SurfaceFormat.Color);
            //particleTexture.SetData<Color>(particleTextureGrid);
            particleTexture = Content.Load<Texture2D>("bubble2"); //Texture2D.FromFile(graphics.GraphicsDevice, @"Content\bubble2.tga");
            //particleTexture = Texture2D.FromFile(graphics.GraphicsDevice, @"Resources\particle.tga");
            arrowTexture = Content.Load<Texture2D>("Arrow");//Texture2D.FromFile(graphics.GraphicsDevice, @"Content\Arrow.bmp");//this.Content.Load<Texture2D>("Arrow");
            timerFont = Content.Load<SpriteFont>("timerFont");
            startMenu = Content.Load<Texture2D>("Menu");

            simulation.arrowTexture = arrowTexture;

            Random rand = new Random();

            //emitter.setPos(graphics.PreferredBackBufferWidth/2, graphics.PreferredBackBufferHeight / 2);
            //emitter.setVelocity(rand.Next(400) - 200, rand.Next(400)-200);
            emitter.setPos(112, 624);
            emitter.setVelocity(0, -200);
            emitter.setRadius(10);
            emitter.setParticlesPerSecond(5);

            //static walls?
            for (int i = 0; i < 11; i++)
                simulation.setWall(i, 3);
            for (int i = 14; i < 24; i++)
                simulation.setWall(7, i);
            for (int i = 19; i < 23; i++)
                simulation.setWall(i, 10);
            for (int i = 0; i < 15; i++)
                simulation.setWall(23, i);

            // static effectors
            for (int i = -5; i > -5; i--)
            {
                Effector eff = new Effector();
                eff.xPos = rand.Next(graphics.PreferredBackBufferWidth);
                eff.yPos = rand.Next(graphics.PreferredBackBufferHeight);
                eff.energyX = rand.Next(100) - 50;
                eff.energyY = rand.Next(100) - 50;
                eff.gridWidthX = 0;
                eff.gridWidthY = 0;
                effectors.Add(i, eff);
            }
        }

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
        }

        /// <summary>
        /// Allows the app to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (isApplicationActivated || isApplicationPreviewed)
            {
                if (isApplicationActivated)
                {
                    if (!gs.started)
                    {                    
                        // deal with not running the game here.
                        ReadOnlyContactCollection contacts = contactTarget.GetState();
                        bool isLocalClear = true;
                        foreach (Contact contact in contacts)
                        {
                            // Create a sprite for each contact that has been recognized as a finger.
                            if (contact.IsFingerRecognized)
                            {
                                
                                // see if we are in the start bubble....
                                // we know that the start circle is at 200,200 and has a radius of 100?
                                int distance = (int)Math.Sqrt(Math.Pow(contact.CenterX - 200, 2) + Math.Pow(contact.CenterY - 200, 2));
                                if (distance < 100 && isClear)
                                {
                                    gs = new GameState();
                                    gs.setLength(5);
                                    gs.startGame(gameTime.TotalRealTime);
                                    gs.setGoal(880, 112, 50, 100);
                                    // clear the particle streem
                                    stream = new List<Particle>();
                                    // clear the effectors:
                                    effectors = new Dictionary<int, Effector>();
                                }
                                else if (distance < 100)
                                {
                                    isLocalClear = false;
                                }
                            }
                        }
                        isClear = isLocalClear;

                        // set up the level?

                    }
                    else // level is running
                    {
                        if (!gs.won && !gs.timeExpired)
                        {
                            applyingEffectors = true;
                            simulation.applyEffectors(effectors);
                            applyingEffectors = false;
                            simulation.Update();
                            simulation.UpdateParticles(stream);
                            // TODO: Process contacts, 
                            // use the following code to get the state of all current contacts.
                            // ReadOnlyContactCollection contacts = contactTarget.GetState();
                            
                            // emit X num Particles:
                            //emitter.emit(stream, 5);
                            // emit the Particles per second:
                            emitter.emit(stream, gameTime.ElapsedRealTime);
                            // copy particles to a temporary array, so you can remove them from the 
                            // real array so without worrying about going out of array bounds.
                            Particle[] tempStream = new Particle[stream.Count];
                            stream.CopyTo(tempStream);
                            foreach (Particle p in tempStream)
                            {
                                if (gs.checkParticle(p))
                                {
                                    stream.Remove(p);
                                }
                                // remove particls that have lived too long (logan's run particals?) and particals that have left the screen.... NO COMING BACK!
                                if ((p.lifeSpan > maxLife) || (p.X < 0 || p.X > graphics.PreferredBackBufferWidth || p.Y < 0 || p.Y > graphics.PreferredBackBufferHeight))
                                    stream.Remove(p);
                                else
                                    p.update();
                            }
                            if (gs.won)
                            { // see if we won
                                this.endTime = new TimeSpan(gameTime.TotalRealTime.Ticks);
                                isClear = false;
                            }
                            gs.checkTime(gameTime.TotalRealTime);
                            if (gs.timeExpired)
                            {
                                isClear = false;
                            }
                        } // level is in play
                    } // Level is running  
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the app should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!applicationLoadCompleteSignalled)
            {
                // Dismiss the loading screen now that we are starting to draw
                ApplicationLauncher.SignalApplicationLoadComplete();
                applicationLoadCompleteSignalled = true;
            }

            //TODO: Rotate the UI based on the value of screenTransform here if desired

            graphics.GraphicsDevice.Clear(backgroundColor);
            if (gs.started && !gs.timeExpired && !gs.won) // we are playing the game.
            {
                //TODO: Add your drawing code here
                //TODO: Avoid any expensive logic if application is neither active nor previewed
                simulation.Draw();

                sb.Begin(SpriteBlendMode.AlphaBlend);
                applyingEffectors = true;
                foreach (Effector e in effectors.Values)
                {
                    // draw an arrow indicating direction and force...
                    float rotation = (float)Math.Atan2(e.energyY, e.energyX);
                    //double scale = Math.Sqrt(Math.Pow(e.energyX, 2) + Math.Pow(e.energyY, 2)) / Math.Sqrt(Math.Pow(e.getMaxEnergy(), 2) + Math.Pow(e.getMaxEnergy(), 2));
                    double dist = Math.Sqrt(Math.Pow(e.contactX - e.xPos, 2)+ Math.Pow(e.contactY - e.yPos, 2));
                    double scale = dist / arrowTexture.Width;
                    Color effColor = new Color(Color.DarkBlue, .25f);
                    sb.Draw(arrowTexture, new Vector2(e.xPos, e.yPos), null, effColor, rotation, new Vector2(0, arrowTexture.Height / 2),new Vector2((float)scale,1f), SpriteEffects.None, 0.8f);
                }
                applyingEffectors = false;
                foreach (Particle p in stream)
                {
                    //sb.Draw(particleTexture, new Vector2(p.X, p.Y), Color.White);
                    Color c = new Color(new Vector4(255, 255, 255, 128));
                    sb.Draw(particleTexture, new Vector2(p.X, p.Y), null, c, 0.0f, new Vector2(particleTexture.Width / 2, particleTexture.Height / 2), (float)20.0 / particleTexture.Width, SpriteEffects.None, .9f);
                }

                // Draw the goal
                float goalScale = (2f * gs.goalRadius) / (particleTexture.Width * 1f);
                sb.Draw(particleTexture, new Vector2(gs.goalX, gs.goalY), null, Color.Turquoise, 0f, new Vector2(particleTexture.Width / 2, particleTexture.Height / 2), goalScale, SpriteEffects.None, .5f);
                Vector2 goalTextSize = timerFont.MeasureString(gs.goalPointsString());
                sb.DrawString(timerFont, gs.goalPointsString(), new Vector2(gs.goalX - goalTextSize.X / 2, gs.goalY - goalTextSize.Y / 2), Color.Turquoise);
                TimeSpan timeLeft = gs.getTimeLeft(gameTime.TotalRealTime);
                string secsLeft = timeLeft.Seconds.ToString();
                if (timeLeft.Seconds < 10)
                    secsLeft = "0" + secsLeft;
                string timerString = "Time Remaining: " + timeLeft.Minutes.ToString() + ":" + secsLeft;
                sb.DrawString(timerFont, timerString, new Vector2(100, 100), Color.Yellow);
                sb.End();
            }
            else if (!gs.started)
            {
                // we arn't started so show a menu to start.
                sb.Begin();
                sb.Draw(startMenu, new Vector2(0, 0), Color.White);
                if (gs.won)
                {
                    TimeSpan timeLeft = gs.getTimeLeft(endTime);
                    string secsLeft = timeLeft.Seconds.ToString();
                    if (timeLeft.Seconds < 10)
                        secsLeft = "0" + secsLeft;
                    sb.DrawString(timerFont, "You Won with " + timeLeft.Minutes.ToString() + ":" + secsLeft + " remaining!", new Vector2(400, 100), Color.Blue);
                }
                else if (gs.timeExpired)
                {
                    sb.DrawString(timerFont, "Time Expired! Try again!", new Vector2(400, 100), Color.Blue);
                }
                sb.End();
            }
            
            base.Draw(gameTime);
        }

        #endregion

        #region Application Event Handlers

        void contactTarget_ContactRemoved(object sender, ContactEventArgs e)
        {
            // only process contacts if the game is started and the level is in play, else we'll handle it in the update.
            if (gs.started && !gs.won && !gs.timeExpired)
            {
                Contact c = e.Contact;
                if (c.CenterX >= 0 && c.CenterX < screenWidth && c.CenterY >= 0 && c.CenterY < screenHeight && effectors.ContainsKey(c.Id))
                {
                    while (applyingEffectors) { }
                    effectors.Remove(c.Id);
                }
            }
        }

        void contactTarget_ContactChanged(object sender, ContactEventArgs e)
        {
            // only process contacts if the game is started and the level is in play, else we'll handle it in the update.
            if (gs.started && !gs.won && !gs.timeExpired)
            {
                Contact c = e.Contact;
                if (c.CenterX >= 0 && c.CenterX < screenWidth && c.CenterY >= 0 && c.CenterY < screenHeight && effectors.ContainsKey(c.Id))
                {
                    while (applyingEffectors) { }
                    effectors[c.Id].move((int)c.CenterX, (int)c.CenterY);
                }
            }
        }

        void contactTarget_ContactAdded(object sender, ContactEventArgs e)
        {
            // only process contacts if the game is started and the level is in play, else we'll handle it in the update.
            if (gs.started && !gs.won && !gs.timeExpired)
            {
                Contact c = e.Contact;

                if (c.IsFingerRecognized)
                {
                    while (applyingEffectors) { }
                    Effector eff = new Effector();
                    eff.contactX = (int)c.CenterX;
                    eff.contactY = (int)c.CenterY;
                    eff.xPos = (int)c.CenterX;
                    eff.yPos = (int)c.CenterY;
                    eff.scaleX = graphics.PreferredBackBufferWidth;
                    eff.scaleY = graphics.PreferredBackBufferHeight;
                    eff.gridWidthX = 0;
                    eff.gridWidthY = 0;
                    eff.energyX = 0;
                    eff.energyY = 0;
                    effectors.Add(c.Id, eff);
                }
            }
        }

        /// <summary>
        /// This is called when application has been activated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationActivated(object sender, EventArgs e)
        {
            // Update application state.
            isApplicationActivated = true;
            isApplicationPreviewed = false;

            // Orientaton can change between activations.
            ResetOrientation();

            //TODO: Enable audio, animations here

            //TODO: Optionally enable raw image here
        }

        /// <summary>
        /// This is called when application is in preview mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationPreviewed(object sender, EventArgs e)
        {
            // Update application state.
            isApplicationActivated = false;
            isApplicationPreviewed = true;

            //TODO: Disable audio here if it is enabled

            //TODO: Optionally enable animations here
        }

        /// <summary>
        ///  This is called when application has been deactivated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationDeactivated(object sender, EventArgs e)
        {
            // Update application state.
            isApplicationActivated = false;
            isApplicationPreviewed = false;

            //TODO: Disable audio, animations here

            //TODO: Disable raw image if it's enabled
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources.
                IDisposable graphicsDispose = graphics as IDisposable;
                if (graphicsDispose != null)
                {
                    graphicsDispose.Dispose();
                }
                if (contactTarget != null)
                {
                    contactTarget.Dispose();
                    contactTarget = null;
                }
            }

            // Release unmanaged Resources.

            // Set large objects to null to facilitate garbage collection.

            base.Dispose(disposing);
        }


        #endregion

    }
}

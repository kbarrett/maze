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

namespace Maze
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font_small;
        public static Texture2D background;
        Maze maze;
        double lastTurn = -1;
        bool won;
        IEnumerator<MazePiece> goalLocs;
        IEnumerator<float> GoalWaiter;

        int difficulty = 1;

        TimeSpan timeTaken;
        TimeSpan lastTimeTaken;
        TimeSpan pausedTime;
        bool stopped;
        bool showTime = true;

        KeyboardState lastKS;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 30 * 20;
            graphics.PreferredBackBufferWidth = 30 * 20;
            Content.RootDirectory = "Content";
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
            base.Initialize();
            maze = new Maze();
            reset();
        }

        private void reset()
        {
            stopped = true;
            goalLocs = maze.getStartPoints(difficulty).GetEnumerator();
            GoalWaiter = null;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = this.Content.Load<SpriteFont>("Font");
            font_small = this.Content.Load<SpriteFont>("Font_small");
           background = this.Content.Load<Texture2D>("art//background");
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (lastTimeTaken == null)
            {
                lastTimeTaken = gameTime.TotalGameTime;
            }
            if (!stopped)
            {
                lastTimeTaken += pausedTime;
                timeTaken = gameTime.TotalGameTime + pausedTime;
                pausedTime = TimeSpan.Zero;
            }
            else if(!won)
            {
                pausedTime += gameTime.ElapsedGameTime;
            }

            if (lastTurn == -1 || lastTurn + 300 < gameTime.TotalGameTime.TotalMilliseconds)
            {
                if (acceptKeyboardInput(gameTime))
                {
                    lastTurn = gameTime.TotalGameTime.TotalMilliseconds;
                }
            }

            won = maze.checkGoal();

            if (won) { stopped = true; }

            base.Update(gameTime);
        }

        private bool acceptKeyboardInput(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();

            if (!stopped)
            {
                if (ks.IsKeyDown(Keys.Up) &&
                    maze.movePlayer(new Vector2(0, -1)))
                {
                    return true;
                }
                else if (ks.IsKeyDown(Keys.Down) && maze.movePlayer(new Vector2(0, 1))) { return true; }
                else if (ks.IsKeyDown(Keys.Left) && maze.movePlayer(new Vector2(-1, 0))) { return true; }
                else if (ks.IsKeyDown(Keys.Right) && maze.movePlayer(new Vector2(1, 0))) { return true; }
            }

            if (pressed(ks, Keys.R))
            {
                maze.reset();
                lastTurn = gameTime.TotalGameTime.TotalMilliseconds;

                lastTimeTaken = timeTaken;
                timeTaken = gameTime.TotalGameTime;

                reset();
                return true;
            }
            if (pressed(ks, Keys.P))
            {
                stopped = !stopped;
                return true;
            }
            if (pressed(ks, Keys.T))
            {
                showTime = !showTime;
                return true;
            }

            lastKS = ks;

            return false;
        }

        private bool pressed(KeyboardState ks, Keys key)
        {
            return ks.IsKeyDown(key) && (lastKS == null || lastKS.IsKeyUp(key));
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            if(goalLocs != null)
            {
                if (GoalWaiter == null)
                {
                    if (goalLocs.MoveNext())
                    {
                        // Wait another 30 frames before advancing again
                        GoalWaiter = WaitDelay(10).GetEnumerator();
                    }
                    else
                    {
                        goalLocs = null;
                    }
                }
                else
                {
                    if (!GoalWaiter.MoveNext())
                    {
                        GoalWaiter = null;
                        stopped = false;
                    }
                }

            }

            if (goalLocs != null)
            {
                float alpha = GoalWaiter != null ? GoalWaiter.Current : 0.0f;
                goalLocs.Current.Draw(spriteBatch, Color.Yellow * alpha);
            }
            else
            {
                maze.Draw(spriteBatch);

                if (showTime)
                {
                    spriteBatch.DrawString(
                        font_small,
                        "Time: " + (timeTaken - lastTimeTaken),
                        new Vector2((Maze.mazeSize - 5) * Maze.mazePieceSize, (Maze.mazeSize - 1) * Maze.mazePieceSize),
                        Color.Yellow);
                }

                if (won)
                {
                    int pos = Maze.mazeSize * Maze.mazePieceSize * 2 / 6;
                    spriteBatch.DrawString(font, "WON", new Vector2(pos, pos), Color.Yellow);
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        protected IEnumerable<float> WaitDelay(int length)
        {
            for (int i = 0; i < length; ++i)
            {
                yield return (float) (length - i) / (float) length;
            }
        }
    }
}

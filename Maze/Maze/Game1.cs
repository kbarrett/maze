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
        public static Texture2D background;
        Maze maze;
        double lastTurn = -1;
        bool won;
        int goalcurrent = 0;
        IEnumerator<MazePiece> goalLocs;
        IEnumerator<bool> GoalWaiter;
        MazePiece currentGoalPiece;

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

            goalLocs = maze.getStartPoints().GetEnumerator();
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

            if (lastTurn == -1 || lastTurn + 300 < gameTime.TotalGameTime.TotalMilliseconds)
            {
                KeyboardState ks = Keyboard.GetState();

                if (!won)
                {
                    bool succeed = false;
                    if (ks.IsKeyDown(Keys.Up) &&
                        maze.movePlayer(new Vector2(0, -1)))
                    {
                        succeed = true;
                    }
                    else if (ks.IsKeyDown(Keys.Down) && maze.movePlayer(new Vector2(0, 1))) { succeed = true; }
                    else if (ks.IsKeyDown(Keys.Left) && maze.movePlayer(new Vector2(-1, 0))) { succeed = true; }
                    else if (ks.IsKeyDown(Keys.Right) && maze.movePlayer(new Vector2(1, 0))) { succeed = true; }

                    if (succeed)
                    {
                        lastTurn = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                }

                if (ks.IsKeyDown(Keys.R)) 
                {
                    maze.reset();
                    lastTurn = gameTime.TotalGameTime.TotalMilliseconds;
                    goalLocs = null;
                    goalcurrent = 0;
                }
            }

            won = maze.checkGoal();

            base.Update(gameTime);
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
                        GoalWaiter = WaitDelay(30).GetEnumerator();
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
                    }
                }

            }

            if (goalLocs != null)
            {
                goalLocs.Current.Draw(spriteBatch, Color.Yellow);
            }
            else
            {
                maze.Draw(spriteBatch);
                if (won)
                {
                    int pos = Maze.mazeSize * Maze.mazePieceSize * 2 / 6;
                    spriteBatch.DrawString(font, "WON", new Vector2(pos, pos), Color.Yellow);
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        protected IEnumerable<bool> WaitDelay(int length)
        {
            for (int i = 0; i < length; ++i)
            {
                yield return false;
            }
        }
    }
}

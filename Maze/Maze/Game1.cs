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
        public static Texture2D background;
        Maze maze;
        double lastTurn = -1;

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

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

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

            // FIXME: please
            if (lastTurn == -1 || lastTurn + 300 < gameTime.TotalGameTime.TotalMilliseconds)
            {
                KeyboardState ks = Keyboard.GetState();
                bool succeed = false;
                if (ks.IsKeyDown(Keys.Up) && maze.movePlayer(new Vector2(0, -1))) { succeed = true; }
                else if (ks.IsKeyDown(Keys.Down) && maze.movePlayer(new Vector2(0, 1))) { succeed = true; }
                else if (ks.IsKeyDown(Keys.Left) && maze.movePlayer(new Vector2(-1, 0))) { succeed = true; }
                else if (ks.IsKeyDown(Keys.Right) && maze.movePlayer(new Vector2(1, 0))) { succeed = true; }

                if (succeed)
                {
                    lastTurn = gameTime.TotalGameTime.TotalMilliseconds;
                }

                if (ks.IsKeyDown(Keys.R)) { maze = new Maze(); }
            }

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
            maze.Draw(spriteBatch);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}

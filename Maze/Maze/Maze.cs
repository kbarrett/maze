using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Maze
{
    class Maze
    {
        Vector2 playerLoc;
        int mazePieceSize = 20;
        int mazeSize = 30;
        MazePiece[,] maze;
        public Maze()
        {
            maze = new MazePiece[mazeSize, mazeSize];
            Random random = new Random();
            for (int i = 0; i < mazeSize; ++i)
            {
                for (int j = 0; j < mazeSize; ++j)
                {
                    maze[i,j] = new MazePiece(i * mazePieceSize, j * mazePieceSize, random.Next(0,3)%3 == 0);
                }
            }
            playerLoc = Vector2.Zero;
            maze[0, 0] = new MazePiece(0, 0, false);
            maze[0, 0].givePlayer();
            maze[0, 0].makeVisible();
            findVisibles(0, 0);

            maze[random.Next(0, mazeSize), random.Next(0, mazeSize)].makeGoal();
        }

        public void Draw(SpriteBatch sb)
        {
            for (int i = 0; i < mazeSize; ++i)
            {
                for (int j = 0; j < mazeSize; ++j)
                {
                    maze[i, j].Draw(sb);
                }
            }
        }

        public bool movePlayer(Vector2 direction)
        {
            Vector2 newLoc = playerLoc + direction;
            if (newLoc.X >= mazeSize || newLoc.X < 0 || newLoc.Y >= mazeSize || newLoc.Y < 0) { return false; }
            if (maze[(int)(playerLoc.X + direction.X), (int)(playerLoc.Y + direction.Y)].wall)
            {
                return false;
            }

            maze[(int)playerLoc.X, (int)playerLoc.Y].removePlayer();
            playerLoc += direction;
            maze[(int)playerLoc.X, (int)playerLoc.Y].givePlayer();
            findVisibles((int)playerLoc.X, (int)playerLoc.Y);

            return true;
        }

        private void findVisibles(int x, int y)
        {
            for (int i = x; i < mazeSize; ++i)
            {
                maze[i, y].makeVisible();
                if (maze[i, y].wall) { break; }
            }
            for (int i = x; i >= 0; --i)
            {
                maze[i, y].makeVisible();
                if (maze[i, y].wall) { break; }
            }
            for (int i = y; i < mazeSize; ++i)
            {
                maze[x, i].makeVisible();
                if (maze[x, i].wall) { break; }
            }
            for (int i = y; i >= 0; --i)
            {
                maze[x, i].makeVisible();
                if (maze[x, i].wall) { break; }
            }
        }

    }

    class MazePiece
    {
        public bool wall { get; private set; }
        bool visible;
        Vector2 loc;
        bool player;
        bool goal;

        public MazePiece(int x, int y, bool wall)
        {
            loc = new Vector2(x,y);
            this.wall = wall;
            visible = false;
        }

        public void Draw(SpriteBatch sb)
        {
            Color colour;
            if(player) colour = Color.Blue;
            else if(wall) colour = Color.Red;
            else if (goal) colour = Color.Yellow;
            else colour = Color.White;

            if(visible)
            {
                sb.Draw(Game1.background, loc, colour);
            }
        }

        public void makeVisible()
        {
            visible = true;
        }

        public void givePlayer()
        {
            player = true;
        }
        public void removePlayer()
        {
            player = false;
        }
        public void makeGoal()
        {
            goal = true;
            wall = false;
        }
    }
}

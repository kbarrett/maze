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
        Vector2 goal = new Vector2(-1,-1);
        public static int mazePieceSize = 20;
        public static int mazeSize = 30;
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
            if (!isValidLoc(newLoc))
            { 
                return false; 
            }
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

        public IEnumerable<MazePiece> placeGoal()
        {
            Vector2 current = new Vector2(playerLoc.X, playerLoc.Y);
            Random random = new Random();
            do
            {
                Vector2 next;
                switch (random.Next(0, 4))
                {
                    case 0:
                        {
                            next = getAbove(current);
                            break;
                        }
                    case 1 :
                        {
                            next = getBelow(current);
                            break;
                        }
                    case 2:
                        {
                            next = getLeft(current);
                            break;
                        }
                    default :
                        {
                            next = getRight(current);
                            break;
                        }
                }
                if (isValidLoc(next))
                {
                    if (!getMazeLoc(next).wall)
                    {
                        current = next;
                    }
                }
                yield return getMazeLoc(current);
            }
            while(random.Next(0, mazeSize * mazeSize * 4) != 0);
            goal = current;
            getMazeLoc(current).makeGoal();
            yield break;
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

        public bool checkGoal()
        {
            return goal == playerLoc;
        }

        public static Vector2 getAbove(Vector2 first) { return first + new Vector2(0, -1); }
        public static Vector2 getBelow(Vector2 first) { return first + new Vector2(0, 1); }
        public static Vector2 getRight(Vector2 first) { return first + new Vector2(1, 0); }
        public static Vector2 getLeft(Vector2 first) { return first + new Vector2(-1, 0); }

        private bool isValidLoc(Vector2 vec)
        {
            return vec.X < mazeSize && vec.X >= 0 && vec.Y < mazeSize && vec.Y >= 0;
        }
        private MazePiece getMazeLoc(Vector2 vec)
        {
            return maze[(int)vec.X, (int)vec.Y];
        }
        public bool goalPlaced()
        {
            return goal != new Vector2(-1, -1);
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

            Draw(sb, colour);
            
        }
        public void Draw(SpriteBatch sb, Color colour)
        {
            if (visible)
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

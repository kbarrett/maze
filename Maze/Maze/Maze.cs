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
        List<Vector2> goalRoute;

        public Maze()
        {
            maze = new MazePiece[mazeSize, mazeSize];
            reset();
        }

        public void reset()
        {
            do
            {
                Random random = new Random();
                for (int i = 0; i < mazeSize; ++i)
                {
                    for (int j = 0; j < mazeSize; ++j)
                    {
                        maze[i, j] = new MazePiece(i * mazePieceSize, j * mazePieceSize, random.Next(0, 3) % 3 == 0);
                    }
                }
                playerLoc = Vector2.Zero;
                maze[0, 0] = new MazePiece(0, 0, false);
                maze[0, 0].givePlayer();
                maze[0, 0].makeVisible();
                findVisibles(0, 0);

                placeGoal();
            }
            while (goalRoute.Count < 10);

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

        public void placeGoal()
        {
            Vector2 current = new Vector2(playerLoc.X, playerLoc.Y);
            goalRoute = recursion(current);

            goal = goalRoute.FirstOrDefault<Vector2>();
            getMazeLoc(goal).makeGoal();

            resetMaze();
        }

        private void resetMaze()
        {
            for (int i = 0; i < mazeSize; ++i)
            {
                for (int j = 0; j < mazeSize; ++j)
                {
                    maze[i, j].explored = false;
                }
            }
        }

        List<Vector2> recursion(Vector2 loc)
        {
            List<Vector2> children = getChildren(loc);
            if (children.Count == 0)
            {
                return new List<Vector2>();
            }
            else
            {
                List<Vector2> longestList = null;
                foreach (Vector2 vec in children)
                {
                    getMazeLoc(vec).explored = true;
                    List<Vector2> returnList = recursion(vec);
                    returnList.Add(vec);
                    if (longestList == null || returnList.Count > longestList.Count)
                    {
                        longestList = returnList;
                    }
                }

                return longestList;
            }
        }

        List<Vector2> getChildren(Vector2 piece)
        {
            List<Vector2> children = new List<Vector2>(4);
            if (isValidLoc(getAbove(piece)) && !getMazeLoc(getAbove(piece)).explored && !getMazeLoc(getAbove(piece)).wall)
            {
                children.Add(getAbove(piece));
            }
            if (isValidLoc(getBelow(piece)) && !getMazeLoc(getBelow(piece)).explored && !getMazeLoc(getBelow(piece)).wall)
            {
                children.Add(getBelow(piece));
            }
            if (isValidLoc(getLeft(piece)) && !getMazeLoc(getLeft(piece)).explored && !getMazeLoc(getLeft(piece)).wall)
            {
                children.Add(getLeft(piece));
            }
            if (isValidLoc(getRight(piece)) && !getMazeLoc(getRight(piece)).explored && !getMazeLoc(getRight(piece)).wall)
            {
                children.Add(getRight(piece));
            }
            return children;
        }

        public IEnumerable<MazePiece> getStartPoints(int steps = 0)
        {
            if (goalRoute.Count == 0)
            {
                yield return getMazeLoc(goal);
            }
            else
            {
                for (int current = goalRoute.Count - 1; current >= 0 && (current > goalRoute.Count - 1 - steps); --current)
                {
                    yield return getMazeLoc(goalRoute[current]);
                }
            }
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
        public bool explored {get; set;}
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
            explored = false;
        }

        public void Draw(SpriteBatch sb)
        {
            Color colour;
            if(player) colour = Color.Blue;
            else if(wall) colour = Color.Red;
            else if (goal) colour = Color.Yellow;
            else colour = Color.White;
            if (visible)
            {
                Draw(sb, colour);
            }
            
        }
        public void Draw(SpriteBatch sb, Color colour)
        {
           sb.Draw(Game1.background, loc, colour);
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

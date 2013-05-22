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
        Vector2 player2Loc;
        List<Vector2> goal;
        public static int mazePieceSize = 20;
        public static int mazeSize = 30;
        MazePiece[,] maze;
        List<Vector2> goalRoute;

        public Maze()
        {
            maze = new MazePiece[mazeSize, mazeSize];
            reset(1);
        }

        public void reset(byte players)
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
                
                goal = new List<Vector2>();

                playerLoc = Vector2.Zero;
                initialisePlayerSquare(1, playerLoc);
                placeGoal(1);
                if (players > 1)
                {
                    player2Loc = new Vector2(mazeSize - 1, mazeSize - 1);
                    initialisePlayerSquare(2, player2Loc);
                    placeGoal(2);
                }
                else
                {
                    if (player2Loc != new Vector2(-1, -1))
                    {
                        player2Loc = new Vector2(-1,-1);
                        foreach (MazePiece mp in maze)
                        {
                            mp.removePlayer(2);
                        }
                    }
                }
            }
            while (goalRoute.Count < 10);

        }

        void initialisePlayerSquare(byte playerNumber, Vector2 playerLoc)
        {
            MazePiece piece = getMazeLoc(playerLoc);
            piece = new MazePiece(mazePieceSize * (int)playerLoc.X, mazePieceSize * (int)playerLoc.Y, false);
            piece.givePlayer(playerNumber);
            piece.makeVisible();
            findVisibles((int)playerLoc.X, (int)playerLoc.Y);
            
            maze[(int)playerLoc.X, (int)playerLoc.Y] = piece;
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

        public bool movePlayer(byte playerNo, Vector2 direction)
        {
            Vector2 thisLoc = playerNo == 1 ? playerLoc : player2Loc;
            Vector2 newLoc = thisLoc + direction;
            if (!isValidLoc(newLoc))
            { 
                return false; 
            }
            if (maze[(int)(thisLoc.X + direction.X), (int)(thisLoc.Y + direction.Y)].wall)
            {
                return false;
            }

            getMazeLoc(thisLoc).removePlayer(playerNo);
            thisLoc += direction;
            getMazeLoc(thisLoc).givePlayer(playerNo);
            findVisibles((int)thisLoc.X, (int)thisLoc.Y);

            if (playerNo == 1)
            {
                playerLoc = thisLoc;
            }
            else if (playerNo == 2)
            {
                player2Loc = thisLoc;
            }

            return true;
        }

        public void placeGoal(byte playerNo)
        {
            Vector2 current = playerNo == 1 ? new Vector2(playerLoc.X, playerLoc.Y) : new Vector2(player2Loc.X, player2Loc.Y);
            goalRoute = recursion(current);

            Vector2 goalLoc = goalRoute.FirstOrDefault<Vector2>();

            goal.Add(goalLoc);

            getMazeLoc(goalLoc).makeGoal();

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
                yield return getMazeLoc(goal[0]);
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
            foreach (Vector2 vec in goal)
            {
                if (vec == playerLoc || vec == player2Loc)
                {
                    return true;
                }
            }
            return false;
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
            return goal.Count == 0;
        }
    }

    class MazePiece
    {
        public bool explored {get; set;}
        public bool wall { get; private set; }
        bool visible = false;
        Vector2 loc;
        bool player = false;
        bool player2 = false;
        bool goal = false;

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
            if (player)
            {
                colour = Color.Blue;
            }
            else if (player2)
            {
                colour = Color.Green;
            }
            else if (wall)
            {
                colour = Color.Red;
            }
            else if (goal)
            {
                colour = Color.Yellow;
            }
            else
            {
                colour = Color.White;
            }
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

        public void givePlayer(byte playerNo)
        {
            switch (playerNo)
            {
                case 1: player = true; break;
                case 2: player2 = true; break;
            }
        }
        public void removePlayer(byte playerNo)
        {
            switch (playerNo)
            {
                case 1: player = false; break;
                case 2: player2 = false; break;
            }
        }
        public void makeGoal()
        {
            goal = true;
            wall = false;
        }
    }
}

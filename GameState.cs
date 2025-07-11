﻿

using System.Windows.Media;

namespace ProjectSnake
{
    public class GameState
    {
        public int Rows { get; }
        public int Cols { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }

        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();

        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
        private readonly Random random = new Random();

        public GameState(int rows, int cols) {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols];
            Dir = Direction.Right; // Initial direction


            AddSnake();
            AddFood();

        }

        private void AddSnake()
        {
            int r = Rows / 2;

            for (int c = 1; c < 4; c++)
            {

                Grid[r, c] = GridValue.Snake;
                snakePositions.AddFirst(new Position(r, c));

            }
        }

        private IEnumerable<Position> EmptyPositions()
        {

            for (int r = 0; r < Rows; r++)
            {

                for (int c = 0; c < Cols; c++) { 
                
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }

                }
            
            }

        }

        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPositions());
            if (empty.Count == 0) return; // No empty positions to place food
            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Food;
        }

        public Position HeadPosition()
        {
            return snakePositions.First.Value;
        }

        public Position TailPosition()
        {
            return snakePositions.Last.Value;
        }

        public IEnumerable<Position> SnakePositions()
        {
            return snakePositions;
        }
        private void AddHead(Position pos)
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;
        }
        private void RemoveTail()
        {
            Position tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Col] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if (dirChanges.Count == 0)
            {
                return Dir; // No direction change, return current direction
            }
            return dirChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            if (dirChanges.Count == 2)
            {
                return false; // No previous direction changes, can change
            }

            Direction lastDir = GetLastDirection();
            // Check if the new direction is opposite to the last direction
            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        public void ChangeDirection(Direction dir)
        {
            if (CanChangeDirection(dir))
            {
               dirChanges.AddLast(dir);
            }
        }

        private bool OutsideGrid(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }

        private GridValue WillHit(Position newHeadPos)
        {
            if (OutsideGrid(newHeadPos)) { 
                return GridValue.Outside; 
            }

            if (newHeadPos == TailPosition())
            {
                return GridValue.Empty; // Hitting the tail is allowed
            }

            return Grid[newHeadPos.Row, newHeadPos.Col];
        }

        public void Move()
        {

            if (dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value; // Get the first direction change
                dirChanges.RemoveFirst(); // Remove it from the list
            }

            Position newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit = WillHit(newHeadPos);
            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;

            }
            else if (hit == GridValue.Empty)
            {
                RemoveTail(); // Remove tail if moving to an empty space
                AddHead(newHeadPos); // Add new head position
            }
            else if (hit == GridValue.Food)
            {
                AddHead(newHeadPos); // Add new head position
                Score++; // Increase score
                AddFood(); // Add new food
            }
        }


    }
}

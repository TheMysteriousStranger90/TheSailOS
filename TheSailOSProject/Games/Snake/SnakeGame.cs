using System;
using System.Collections.Generic;
using System.Threading;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Games.Snake
{
    public class SnakeGame
    {
        private const int WIDTH = 20;
        private const int HEIGHT = 20;
        private const int FRAME_DELAY = 200;

        private List<Position> snake;
        private Position food;
        private Direction currentDirection;
        private bool gameOver;
        private Random random;
        private int score;

        public SnakeGame()
        {
            random = new Random();
            InitializeGame();
        }

        private void InitializeGame()
        {
            snake = new List<Position>
            {
                new Position(WIDTH / 2, HEIGHT / 2)
            };
            currentDirection = Direction.Right;
            gameOver = false;
            score = 0;
            GenerateFood();
        }

        public void Run()
        {
            Console.CursorVisible = false;
            Console.Clear();

            while (!gameOver)
            {
                if (Console.KeyAvailable)
                {
                    HandleInput();
                }

                MoveSnake();
                CheckCollision();
                Draw();

                Thread.Sleep(FRAME_DELAY);
            }

            ShowGameOver();
        }

        private void HandleInput()
        {
            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.UpArrow when currentDirection != Direction.Down:
                    currentDirection = Direction.Up;
                    break;
                case ConsoleKey.DownArrow when currentDirection != Direction.Up:
                    currentDirection = Direction.Down;
                    break;
                case ConsoleKey.LeftArrow when currentDirection != Direction.Right:
                    currentDirection = Direction.Left;
                    break;
                case ConsoleKey.RightArrow when currentDirection != Direction.Left:
                    currentDirection = Direction.Right;
                    break;
                case ConsoleKey.Escape:
                    gameOver = true;
                    break;
            }
        }

        private void MoveSnake()
        {
            var head = snake[0];
            var newHead = new Position(head.X, head.Y);

            switch (currentDirection)
            {
                case Direction.Up:
                    newHead.Y--;
                    break;
                case Direction.Down:
                    newHead.Y++;
                    break;
                case Direction.Left:
                    newHead.X--;
                    break;
                case Direction.Right:
                    newHead.X++;
                    break;
            }

            snake.Insert(0, newHead);

            if (newHead.X == food.X && newHead.Y == food.Y)
            {
                score += 10;
                GenerateFood();
            }
            else
            {
                snake.RemoveAt(snake.Count - 1);
            }
        }

        private void CheckCollision()
        {
            var head = snake[0];
            
            if (head.X < 0 || head.X >= WIDTH || head.Y < 0 || head.Y >= HEIGHT)
            {
                gameOver = true;
                return;
            }
            
            for (int i = 1; i < snake.Count; i++)
            {
                if (head.X == snake[i].X && head.Y == snake[i].Y)
                {
                    gameOver = true;
                    return;
                }
            }
        }

        private void GenerateFood()
        {
            do
            {
                food = new Position(random.Next(WIDTH), random.Next(HEIGHT));
            } while (snake.Exists(p => p.X == food.X && p.Y == food.Y));
        }

        private void Draw()
        {
            Console.SetCursorPosition(0, 0);
            
            ConsoleManager.WriteLineColored(new string('═', WIDTH + 2), ConsoleStyle.Colors.Primary);

            for (int y = 0; y < HEIGHT; y++)
            {
                ConsoleManager.WriteColored("║", ConsoleStyle.Colors.Primary);
                
                for (int x = 0; x < WIDTH; x++)
                {
                    if (snake.Exists(p => p.X == x && p.Y == y))
                    {
                        ConsoleManager.WriteColored("█", ConsoleStyle.Colors.Success);
                    }
                    else if (food.X == x && food.Y == y)
                    {
                        ConsoleManager.WriteColored("●", ConsoleStyle.Colors.Error);
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                }
                
                ConsoleManager.WriteLineColored("║", ConsoleStyle.Colors.Primary);
            }
            
            ConsoleManager.WriteLineColored(new string('═', WIDTH + 2), ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored($"Score: {score}", ConsoleStyle.Colors.Accent);
        }

        private void ShowGameOver()
        {
            Console.Clear();
            ConsoleManager.WriteLineColored("Game Over!", ConsoleStyle.Colors.Error);
            ConsoleManager.WriteLineColored($"Final Score: {score}", ConsoleStyle.Colors.Success);
            ConsoleManager.WriteLineColored("Press any key to exit...", ConsoleStyle.Colors.Primary);
            Console.ReadKey(true);
        }
    }

    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
using System;
using System.Threading;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Games.Tetris
{
    public class TetrisGame
    {
        private const int WIDTH = 10;
        private const int HEIGHT = 20;
        private const int FRAME_DELAY = 300;

        private readonly int[] _playField;
        private TetrisPiece _currentPiece;
        private Position _currentPosition;
        private Random _random;
        private bool _gameOver;
        private int _score;
        private int _level;
        private int _linesCleared;

        public TetrisGame()
        {
            _playField = new int[WIDTH * HEIGHT];
            _random = new Random();
            InitializeGame();
        }

        private void InitializeGame()
        {
            _gameOver = false;
            _score = 0;
            _level = 1;
            _linesCleared = 0;
            ClearPlayField();
            SpawnNewPiece();
        }

        public void Run()
        {
            DrawStartScreen();
            Console.ReadKey(true);

            Console.CursorVisible = false;
            Console.Clear();

            var lastMoveTime = System.DateTime.Now;

            while (!_gameOver)
            {
                if (Console.KeyAvailable)
                {
                    HandleInput();
                }

                if ((System.DateTime.Now - lastMoveTime).TotalMilliseconds > FRAME_DELAY / _level)
                {
                    MovePieceDown();
                    lastMoveTime = System.DateTime.Now;
                }

                Draw();
                Thread.Sleep(50);
            }

            ShowGameOver();
        }

        private void DrawStartScreen()
        {
            Console.Clear();
            ConsoleManager.WriteLineColored("=== TETRIS ===", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored("\nControls:", ConsoleStyle.Colors.Success);
            ConsoleManager.WriteLineColored("← → : Move left/right", ConsoleStyle.Colors.Accent);
            ConsoleManager.WriteLineColored("↑   : Rotate", ConsoleStyle.Colors.Accent);
            ConsoleManager.WriteLineColored("↓   : Soft drop", ConsoleStyle.Colors.Accent);
            ConsoleManager.WriteLineColored("Space: Hard drop", ConsoleStyle.Colors.Accent);
            ConsoleManager.WriteLineColored("ESC  : Exit game", ConsoleStyle.Colors.Accent);
            ConsoleManager.WriteLineColored("\nPress any key to start...", ConsoleStyle.Colors.Warning);
        }

        private void HandleInput()
        {
            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    if (CanMoveTo(_currentPosition.X - 1, _currentPosition.Y))
                        _currentPosition.X--;
                    break;

                case ConsoleKey.RightArrow:
                    if (CanMoveTo(_currentPosition.X + 1, _currentPosition.Y))
                        _currentPosition.X++;
                    break;

                case ConsoleKey.DownArrow:
                    MovePieceDown();
                    _score++;
                    break;

                case ConsoleKey.UpArrow:
                    RotatePiece();
                    break;

                case ConsoleKey.Spacebar:
                    HardDrop();
                    break;

                case ConsoleKey.Escape:
                    _gameOver = true;
                    break;
            }
        }

        private void MovePieceDown()
        {
            if (CanMoveTo(_currentPosition.X, _currentPosition.Y + 1))
            {
                _currentPosition.Y++;
            }
            else
            {
                PlacePiece();
                CheckLines();
                SpawnNewPiece();
            }
        }

        private void HardDrop()
        {
            while (CanMoveTo(_currentPosition.X, _currentPosition.Y + 1))
            {
                _currentPosition.Y++;
                _score += 2;
            }
            PlacePiece();
            CheckLines();
            SpawnNewPiece();
        }

        private void RotatePiece()
        {
            var rotated = _currentPiece.GetRotated();
            if (CanPlace(rotated, _currentPosition))
            {
                _currentPiece = rotated;
            }
        }

        private void PlacePiece()
        {
            for (int y = 0; y < TetrisPiece.SIZE; y++)
            {
                for (int x = 0; x < TetrisPiece.SIZE; x++)
                {
                    if (_currentPiece.GetShape(x, y) != 0)
                    {
                        int fieldX = _currentPosition.X + x;
                        int fieldY = _currentPosition.Y + y;
                        _playField[fieldY * WIDTH + fieldX] = _currentPiece.Color;
                    }
                }
            }
        }

        private void CheckLines()
        {
            int linesCleared = 0;

            for (int y = HEIGHT - 1; y >= 0; y--)
            {
                bool isLineFull = true;
                for (int x = 0; x < WIDTH; x++)
                {
                    if (_playField[y * WIDTH + x] == 0)
                    {
                        isLineFull = false;
                        break;
                    }
                }

                if (isLineFull)
                {
                    ClearLine(y);
                    linesCleared++;
                }
            }

            if (linesCleared > 0)
            {
                _linesCleared += linesCleared;
                _score += linesCleared * 100 * _level;
                _level = (_linesCleared / 10) + 1;
            }
        }

        private void ClearLine(int row)
        {
            for (int y = row; y > 0; y--)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    _playField[y * WIDTH + x] = _playField[(y - 1) * WIDTH + x];
                }
            }
        }

        private void SpawnNewPiece()
        {
            _currentPiece = TetrisPiece.GetRandomPiece(_random);
            _currentPosition = new Position(WIDTH / 2 - 2, 0);

            if (!CanPlace(_currentPiece, _currentPosition))
            {
                _gameOver = true;
            }
        }

        private bool CanMoveTo(int newX, int newY)
        {
            return CanPlace(_currentPiece, new Position(newX, newY));
        }

        private bool CanPlace(TetrisPiece piece, Position pos)
        {
            for (int y = 0; y < TetrisPiece.SIZE; y++)
            {
                for (int x = 0; x < TetrisPiece.SIZE; x++)
                {
                    if (piece.GetShape(x, y) != 0)
                    {
                        int worldX = pos.X + x;
                        int worldY = pos.Y + y;

                        if (worldX < 0 || worldX >= WIDTH || worldY >= HEIGHT)
                            return false;

                        if (worldY >= 0 && _playField[worldY * WIDTH + worldX] != 0)
                            return false;
                    }
                }
            }
            return true;
        }

        private void Draw()
        {
            Console.SetCursorPosition(0, 0);

            ConsoleManager.WriteLineColored("╔" + new string('═', WIDTH * 2) + "╗", ConsoleStyle.Colors.Primary);

            for (int y = 0; y < HEIGHT; y++)
            {
                ConsoleManager.WriteColored("║", ConsoleStyle.Colors.Primary);

                for (int x = 0; x < WIDTH; x++)
                {
                    bool isPieceCell = false;
                    if (_currentPiece != null)
                    {
                        for (int py = 0; py < TetrisPiece.SIZE; py++)
                        {
                            for (int px = 0; px < TetrisPiece.SIZE; px++)
                            {
                                if (_currentPosition.X + px == x &&
                                    _currentPosition.Y + py == y &&
                                    _currentPiece.GetShape(px, py) != 0)
                                {
                                    var colors = GetColorForNumber(_currentPiece.Color);
                                    ConsoleManager.WriteColored("██", colors.fore, colors.back);
                                    isPieceCell = true;
                                }
                            }
                        }
                    }

                    if (!isPieceCell)
                    {
                        int cell = _playField[y * WIDTH + x];
                        if (cell == 0)
                            Console.Write("  ");
                        else
                        {
                            var colors = GetColorForNumber(cell);
                            ConsoleManager.WriteColored("██", colors.fore, colors.back);
                        }
                    }
                }

                ConsoleManager.WriteLineColored("║", ConsoleStyle.Colors.Primary);
            }

            ConsoleManager.WriteLineColored("╚" + new string('═', WIDTH * 2) + "╝", ConsoleStyle.Colors.Primary);
            
            DrawStats();
        }

        private void DrawStats()
        {
            ConsoleManager.WriteLineColored($"Score: {_score}", ConsoleStyle.Colors.Accent);
            ConsoleManager.WriteLineColored($"Level: {_level}", ConsoleStyle.Colors.Success);
            ConsoleManager.WriteLineColored($"Lines: {_linesCleared}", ConsoleStyle.Colors.Warning);
        }

        private void ClearPlayField()
        {
            for (int i = 0; i < _playField.Length; i++)
            {
                _playField[i] = 0;
            }
        }

        private void ShowGameOver()
        {
            Console.Clear();
            ConsoleManager.WriteLineColored("=== GAME OVER ===", ConsoleStyle.Colors.Error);
            ConsoleManager.WriteLineColored($"\nFinal Score: {_score}", ConsoleStyle.Colors.Success);
            ConsoleManager.WriteLineColored($"Level Reached: {_level}", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored($"Lines Cleared: {_linesCleared}", ConsoleStyle.Colors.Warning);
            ConsoleManager.WriteLineColored("\nPress any key to exit...", ConsoleStyle.Colors.Accent);
            Console.ReadKey(true);
        }

        private (ConsoleColor fore, ConsoleColor back) GetColorForNumber(int num)
        {
            return num switch
            {
                1 => (ConsoleColor.White, ConsoleColor.Red),      // I Piece
                2 => (ConsoleColor.White, ConsoleColor.Blue),     // O Piece
                3 => (ConsoleColor.Black, ConsoleColor.Yellow),   // T Piece
                4 => (ConsoleColor.White, ConsoleColor.DarkGreen),// S Piece
                5 => (ConsoleColor.White, ConsoleColor.Magenta),  // Z Piece
                6 => (ConsoleColor.Black, ConsoleColor.Cyan),     // J Piece
                7 => (ConsoleColor.White, ConsoleColor.DarkRed),  // L Piece
                _ => (ConsoleColor.Gray, ConsoleColor.Black)      // Empty/Default
            };
        }
    }
}

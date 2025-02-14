using System;
using System.Linq;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Games.TicTacToe
{
    public class TicTacToeGame
    {
        private int[] _board;
        private bool _isPlayerTurn;
        private bool _gameOver;
        private int _winner;
        private Random _random;

        public TicTacToeGame()
        {
            _board = new int[9];
            _random = new Random();
            InitializeGame();
        }

        private void InitializeGame()
        {
            for (int i = 0; i < 9; i++)
                _board[i] = 0;

            _isPlayerTurn = true;
            _gameOver = false;
            _winner = 0;
        }

        public void Run()
        {
            try
            {
                DrawStartScreen();
                Console.ReadKey(true);
                Console.Clear();

                while (!_gameOver)
                {
                    Draw();

                    if (_isPlayerTurn)
                    {
                        HandlePlayerTurn();
                    }
                    else
                    {
                        HandleComputerTurn();
                    }

                    CheckGameState();
                }

                ShowGameOver();
            }
            catch (Exception ex)
            {
                Console.Clear();
                ConsoleManager.WriteLineColored("Game crashed: " + ex.Message, ConsoleStyle.Colors.Error);
                Console.ReadKey(true);
            }
        }

        private void DrawStartScreen()
        {
            Console.Clear();
            ConsoleManager.WriteLineColored("=== TIC TAC TOE ===", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored("\nControls:", ConsoleStyle.Colors.Success);
            ConsoleManager.WriteLineColored("Use numpad (1-9) to place X", ConsoleStyle.Colors.Accent);
            ConsoleManager.WriteLineColored("ESC to exit game", ConsoleStyle.Colors.Accent);
            ConsoleManager.WriteLineColored("\nBoard Layout:", ConsoleStyle.Colors.Warning);
            ConsoleManager.WriteLineColored("7|8|9", ConsoleStyle.Colors.Warning);
            ConsoleManager.WriteLineColored("-+-+-", ConsoleStyle.Colors.Warning);
            ConsoleManager.WriteLineColored("4|5|6", ConsoleStyle.Colors.Warning);
            ConsoleManager.WriteLineColored("-+-+-", ConsoleStyle.Colors.Warning);
            ConsoleManager.WriteLineColored("1|2|3", ConsoleStyle.Colors.Warning);
            ConsoleManager.WriteLineColored("\nPress any key to start...", ConsoleStyle.Colors.Primary);
        }

        private void HandlePlayerTurn()
        {
            bool validMove = false;
            while (!validMove && !_gameOver)
            {
                try
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth));

                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        _gameOver = true;
                        return;
                    }

                    int move = -1;
                    switch (key.KeyChar)
                    {
                        case '1':
                            move = 6;
                            break;
                        case '2':
                            move = 7;
                            break;
                        case '3':
                            move = 8;
                            break;
                        case '4':
                            move = 3;
                            break;
                        case '5':
                            move = 4;
                            break;
                        case '6':
                            move = 5;
                            break;
                        case '7':
                            move = 0;
                            break;
                        case '8':
                            move = 1;
                            break;
                        case '9':
                            move = 2;
                            break;
                    }

                    if (move != -1 && IsValidMove(move))
                    {
                        _board[move] = 1;
                        validMove = true;
                        _isPlayerTurn = false;
                    }
                    else
                    {
                        ConsoleManager.WriteLineColored("Invalid move! Use numbers 1-9", ConsoleStyle.Colors.Error);
                    }

                    Draw();
                }
                catch
                {
                    _gameOver = true;
                    return;
                }
            }
        }

        private void Draw()
        {
            try
            {
                Console.Clear();
                ConsoleManager.WriteLineColored("=== TIC TAC TOE ===\n", ConsoleStyle.Colors.Primary);

                for (int i = 0; i < 9; i += 3)
                {
                    DrawRow(i);
                    if (i < 6)
                    {
                        ConsoleManager.WriteLineColored("-+-+-", ConsoleStyle.Colors.Primary);
                    }
                }

                Console.WriteLine();
                if (!_gameOver)
                {
                    var turnMessage = _isPlayerTurn ? "Your turn (X)" : "Computer's turn (O)";
                    ConsoleManager.WriteLineColored(turnMessage, ConsoleStyle.Colors.Warning);
                }

                System.Threading.Thread.Sleep(50);
            }
            catch
            {
                _gameOver = true;
            }
        }

        private void DrawRow(int startIndex)
        {
            for (int i = 0; i < 3; i++)
            {
                DrawCell(_board[startIndex + i]);
                if (i < 2) ConsoleManager.WriteColored("|", ConsoleStyle.Colors.Primary);
            }

            Console.WriteLine();
        }

        private void DrawCell(int value)
        {
            switch (value)
            {
                case 1:
                    ConsoleManager.WriteColored("X", ConsoleStyle.Colors.Success);
                    break;
                case 2:
                    ConsoleManager.WriteColored("O", ConsoleStyle.Colors.Error);
                    break;
                default:
                    Console.Write(" ");
                    break;
            }
        }

        private void HandleComputerTurn()
        {
            System.Threading.Thread.Sleep(500);

            for (int i = 0; i < 9; i++)
            {
                if (IsValidMove(i))
                {
                    _board[i] = 2;
                    if (CheckWinner() == 2)
                        return;
                    _board[i] = 0;
                }
            }

            for (int i = 0; i < 9; i++)
            {
                if (IsValidMove(i))
                {
                    _board[i] = 1;
                    if (CheckWinner() == 1)
                    {
                        _board[i] = 2;
                        _isPlayerTurn = true;
                        return;
                    }

                    _board[i] = 0;
                }
            }

            if (IsValidMove(4))
            {
                _board[4] = 2;
                _isPlayerTurn = true;
                return;
            }

            var corners = new[] { 0, 2, 6, 8 };
            var availableCorners = corners.Where(c => IsValidMove(c)).ToList();
            if (availableCorners.Any())
            {
                int corner = availableCorners[_random.Next(availableCorners.Count)];
                _board[corner] = 2;
                _isPlayerTurn = true;
                return;
            }

            var sides = new[] { 1, 3, 5, 7 };
            var availableSides = sides.Where(s => IsValidMove(s)).ToList();
            if (availableSides.Any())
            {
                int side = availableSides[_random.Next(availableSides.Count)];
                _board[side] = 2;
            }

            _isPlayerTurn = true;
        }

        private int GetMoveFromKey(ConsoleKey key)
        {
            return key switch
            {
                ConsoleKey.NumPad1 or ConsoleKey.D1 => 6,
                ConsoleKey.NumPad2 or ConsoleKey.D2 => 7,
                ConsoleKey.NumPad3 or ConsoleKey.D3 => 8,
                ConsoleKey.NumPad4 or ConsoleKey.D4 => 3,
                ConsoleKey.NumPad5 or ConsoleKey.D5 => 4,
                ConsoleKey.NumPad6 or ConsoleKey.D6 => 5,
                ConsoleKey.NumPad7 or ConsoleKey.D7 => 0,
                ConsoleKey.NumPad8 or ConsoleKey.D8 => 1,
                ConsoleKey.NumPad9 or ConsoleKey.D9 => 2,
                _ => -1
            };
        }

        private bool IsValidMove(int position)
        {
            return position >= 0 && position < 9 && _board[position] == 0;
        }

        private void CheckGameState()
        {
            _winner = CheckWinner();
            if (_winner != 0 || IsBoardFull())
            {
                _gameOver = true;
            }
        }

        private int CheckWinner()
        {
            for (int i = 0; i < 9; i += 3)
                if (_board[i] != 0 && _board[i] == _board[i + 1] && _board[i] == _board[i + 2])
                    return _board[i];

            for (int i = 0; i < 3; i++)
                if (_board[i] != 0 && _board[i] == _board[i + 3] && _board[i] == _board[i + 6])
                    return _board[i];

            if (_board[0] != 0 && _board[0] == _board[4] && _board[0] == _board[8])
                return _board[0];
            if (_board[2] != 0 && _board[2] == _board[4] && _board[2] == _board[6])
                return _board[2];

            return 0;
        }

        private bool IsBoardFull()
        {
            return !_board.Contains(0);
        }

        private void ShowGameOver()
        {
            Console.WriteLine();
            if (_winner == 1)
                ConsoleManager.WriteLineColored("You win!", ConsoleStyle.Colors.Success);
            else if (_winner == 2)
                ConsoleManager.WriteLineColored("Computer wins!", ConsoleStyle.Colors.Error);
            else
                ConsoleManager.WriteLineColored("It's a draw!", ConsoleStyle.Colors.Warning);

            ConsoleManager.WriteLineColored("\nPress any key to exit...", ConsoleStyle.Colors.Primary);
            Console.ReadKey(true);
        }
    }
}
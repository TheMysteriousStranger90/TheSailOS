using System;

namespace TheSailOSProject.Games.Tetris
{
    public class TetrisPiece
    {
        private int[] _shape;
        public int Color { get; private set; }
        public const int SIZE = 4;

        private static readonly int[][] Pieces =
        {
            // I Piece
            new int[] { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
            // O Piece
            new int[] { 0, 2, 2, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            // T Piece
            new int[] { 0, 3, 0, 0, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            // S Piece
            new int[] { 0, 4, 4, 0, 4, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            // Z Piece
            new int[] { 5, 5, 0, 0, 0, 5, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            // J Piece
            new int[] { 6, 0, 0, 0, 6, 6, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            // L Piece
            new int[] { 0, 0, 7, 0, 7, 7, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        public TetrisPiece(int[] shape, int color)
        {
            _shape = shape;
            Color = color;
        }

        public int GetShape(int x, int y)
        {
            return _shape[y * SIZE + x];
        }

        public static TetrisPiece GetRandomPiece(Random random)
        {
            int index = random.Next(Pieces.Length);
            return new TetrisPiece(Pieces[index], index + 1);
        }

        public TetrisPiece GetRotated()
        {
            int[] rotated = new int[SIZE * SIZE];

            for (int y = 0; y < SIZE; y++)
            {
                for (int x = 0; x < SIZE; x++)
                {
                    rotated[(SIZE - 1 - y) + x * SIZE] = _shape[y * SIZE + x];
                }
            }

            return new TetrisPiece(rotated, Color);
        }
    }
}
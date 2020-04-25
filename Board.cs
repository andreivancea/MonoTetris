using System;

namespace Tetris
{
    class Board
    {
        public int NumRows { get; private set; }
        public int NumColumns { get; private set; }
        public int[][] Color { get; private set; }

        public bool[][] Block { get; private set; }

        readonly int[] count;

        public Board(int n, int m)
        {
            this.NumRows = n;
            this.NumColumns = m;
            Block = new bool[NumRows][];
            Color = new int[NumRows][];
            for (int i = 0; i < NumRows; i++)
            {
                Block[i] = new bool[NumColumns];
                Color[i] = new int[NumColumns];
            }
            count = new int[NumRows];
        }


        public bool DoesTetrominoFit(Tetromino t)
        {
            for (int i = 0; i < t.NumRows; i++)
                for (int j = 0; j < t.NumColumns; j++)
                {
                    if (!t.Block(i, j))
                    {
                        continue;
                    }
                    int xx = t.PositionX + i - t.NumRows / 2;
                    int yy = t.PositionY + j - t.NumColumns / 2;
                    if (xx < 0 || xx >= NumRows || yy < 0 || yy >= NumColumns || Block[xx][yy])
                    {
                        return false;
                    }
                }
            return true;
        }

        public void Add(Tetromino t)
        {
            if (!DoesTetrominoFit(t))
            {
                throw new System.InvalidOperationException("Tetromino does not fit.");
            }

            for (int i = 0; i < t.NumRows; i++)
                for (int j = 0; j < t.NumColumns; j++)
                {
                    if (t.Block(i, j))
                    {
                        int xx = t.PositionX + i - t.NumRows / 2;
                        int yy = t.PositionY + j - t.NumColumns / 2;
                        Block[xx][yy] = true;
                        Color[xx][yy] = t.Color;
                        count[xx]++;
                    }
                }
        }

        public bool LineFull(int k)
        {
            return count[k] == NumColumns;
        }

        public int Clear()
        {
            int ii = NumRows - 1;
            int result = 0;
            for (int i = NumRows - 1; i >= 0; i--)
            {
                if (LineFull(i))
                {
                    result++;
                    continue;
                }
                Block[ii] = Block[i];
                Color[ii] = Color[i];
                count[ii] = count[i];               
                ii--;
            }
            for (int i = 0; i < result; i++)
            {
                count[i] = 0;
                Color[i] = new int[NumColumns];
                Block[i] = new bool[NumColumns];
            }
            return result;
        }

        public bool CanClearLines()
        {
            for (int i = NumRows - 1; i >= 0; i--)
            {
                if (LineFull(i))
                {
                    return true;
                }
            }
            return false;
        }
    }

}

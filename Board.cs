using System;

namespace Tetris
{
    class Board
    {
        public int N { get; private set; }
        public int M { get; private set; }
        public int[][] Color { get; private set; }

        public bool[][] Block { get; private set; }
        int[] count;

        public Board(int n, int m)
        {
            this.N = n;
            this.M = m;
            Block = new bool[N][];
            Color = new int[N][];
            for (int i = 0; i < N; i++)
            {
                Block[i] = new bool[M];
                Color[i] = new int[M];
            }
            count = new int[N];
        }


        public bool Fits(Tetromino t)
        {
            for (int i = 0; i < t.N; i++)
                for (int j = 0; j < t.M; j++)
                {
                    if (!t.Block(i, j))
                    {
                        continue;
                    }
                    int xx = t.PositionX + i - t.N / 2;
                    int yy = t.PositionY + j - t.M / 2;
                    if (xx < 0 || xx >= N || yy < 0 || yy >= M || Block[xx][yy])
                    {
                        return false;
                    }
                }
            return true;
        }

        public void Add(Tetromino t)
        {
            if (!Fits(t))
            {
                throw new System.InvalidOperationException("Tetromino does not fit.");
            }

            for (int i = 0; i < t.N; i++)
                for (int j = 0; j < t.M; j++)
                {
                    if (t.Block(i, j))
                    {
                        int xx = t.PositionX + i - t.N / 2;
                        int yy = t.PositionY + j - t.M / 2;
                        Block[xx][yy] = true;
                        Color[xx][yy] = t.Color;
                        count[xx]++;
                    }
                }
        }

        public bool LineFull(int k)
        {
            return count[k] == M;
        }

        public int Clear()
        {
            int ii = N - 1;
            int result = 0;
            for (int i = N - 1; i >= 0; i--)
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
                Color[i] = new int[M];
                Block[i] = new bool[M];
            }
            return result;
        }

        public bool CanClear()
        {
            for (int i = N - 1; i >= 0; i--)
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

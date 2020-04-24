namespace Tetris
{
    class Tetromino
    {
        private readonly bool[,] blocks;

        public int N
        {
            get
            {
                return (dx[rotation,0] != 0) ? blocks.GetLength(0) : blocks.GetLength(1);
            }
        }
        public int M
        {
            get
            {
                return (dx[rotation,0] != 0) ? blocks.GetLength(1) : blocks.GetLength(0);
            }
        }

        private int rotation;
        public int Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = (4 + value) % 4;
            }

        }

        public int PositionX { get; set; }
        public int PositionY { get; set; }


        // X unit (for each rotation)
        private static readonly int[,] dx = { 
            {1, 0},
            {0, 1},
            {-1, 0},
            {0, -1},
        };


        // Y unit (for each rotation)
        private static readonly int[,] dy = {
             {0, 1},
             {-1, 0},
             {0, -1},
             {1, 0},
        };

        public int Color { get; }


        public Tetromino(bool[,] blocks, int color)
        {
            this.blocks = blocks;
            Color = color;
        }

        public Tetromino(Tetromino other)
        {
            this.blocks = other.blocks;
            this.Color = other.Color;
            this.PositionX = other.PositionX;
            this.PositionY = other.PositionY;
            this.rotation = other.rotation;
        }

        public bool Block(int i, int j)
        {
            int cx = (dx[rotation,0] == -1 || dy[rotation,0] == -1) ? blocks.GetLength(0) - 1 : 0;
            int cy = (dx[rotation,1] == -1 || dy[rotation,1] == -1) ? blocks.GetLength(1) - 1 : 0;
            return blocks[cx + i * dx[rotation,0] + j * dy[rotation,0], 
                          cy + i * dx[rotation,1] + j * dy[rotation,1]];
        }

        public bool LineEmpty(int i)
        {
            for (int j = 0; j < M; j++)
            {
                if (Block(i, j))
                {
                    return false;
                }
            }
            return true;                
        }
    }
}

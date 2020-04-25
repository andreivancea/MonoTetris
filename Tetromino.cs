using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace Tetris
{
    class Tetromino
    {
        private readonly bool[,] blocks;


        public int NumRows
        {
            get
            {
                return (hUnit[rotation,0] != 0) ? blocks.GetLength(0) : blocks.GetLength(1);
            }
        }
        public int NumColumns
        {
            get
            {
                return (hUnit[rotation,0] != 0) ? blocks.GetLength(1) : blocks.GetLength(0);
            }
        }


        public int[,,] SrsRotationOffset { get; private set; }

        private int rotation;
        public int Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                if (value < 0 || value > 3)
                {
                    throw new System.InvalidOperationException("Invalid rotation value.");
                }                    
                rotation = (4 + value) % 4;
            }

        }

        public int PositionX { get; set; }
        public int PositionY { get; set; }


        // horizontal unit (for each rotation)
        private static readonly int[,] hUnit = { 
            {1, 0},
            {0, 1},
            {-1, 0},
            {0, -1},
        };


        // vertical unit (for each rotation)
        private static readonly int[,] vUnit = {
             {0, 1},
             {-1, 0},
             {0, -1},
             {1, 0},
        };

        public int Color { get; }


        public Tetromino(bool[,] blocks, int[,,] srsRotationOffset, int color)
        {
            SrsRotationOffset = srsRotationOffset;
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
            this.SrsRotationOffset = other.SrsRotationOffset;
        }

        public bool Block(int i, int j)
        {
            int cx = (hUnit[rotation,0] == -1 || vUnit[rotation,0] == -1) ? blocks.GetLength(0) - 1 : 0;
            int cy = (hUnit[rotation,1] == -1 || vUnit[rotation,1] == -1) ? blocks.GetLength(1) - 1 : 0;
            return blocks[cx + i * hUnit[rotation,0] + j * vUnit[rotation,0], 
                          cy + i * hUnit[rotation,1] + j * vUnit[rotation,1]];
        }

        public bool IsLineEmpty(int i)
        {
            for (int j = 0; j < NumColumns; j++)
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

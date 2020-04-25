using System;
using System.Collections.Generic;

namespace Tetris
{
    class Game
    {


        private static readonly int[,,] srsRotationOffset = new int[,,]
        {
            {  {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}  },
            {  {0, 0}, {+1, 0}, {+1,-1}, {0,+2 }, {+1,+2} },
            {  {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0} },
            {  {0, 0 }, {-1, 0}, {-1,-1}, {0,+2}, {-1,+2} },
        };


        private static readonly int[,,] srsRotationOffsetI = new int[,,]
        {
            {  {0, 0}, {-1, 0}, {+2, 0}, {-1, 0}, {+2, 0}  },
            {  {-1, 0}, {0, 0}, {0, 0}, {0,+1}, {0,-2} },
            {  { -1,+1}, {+1,+1}, {-2,+1}, {+1, 0}, {-2, 0}  },
            {  {0,+1}, {0,+1}, {0, +1}, {0,-1}, {0,+2}  },
        };

        private static readonly int[,,] srsRotationOffsetO = new int[,,]
        {
            {  {0, 0}  },
            {  {0, -1} },
            {  {-1, -1} },
            {  {-1, 0} },
        };

        private static readonly IList<Tetromino> tetrominos = new List<Tetromino>
        {
            // XXXX
            new Tetromino(new bool[,]
            {
                {false, false, false, false, false},
                {false, false, false, false, false},
                {false, true, true, true, true},
                {false, false, false, false, false},
                {false, false, false, false, false},
            }, srsRotationOffsetI, 0),
            

            // XX
            // XX
            new Tetromino(new bool[,]
            {
                {false, true, true},
                {false, true, true},
                {false, false, false},
            }, srsRotationOffsetO, 1),

            //  X
            // XXX
            new Tetromino(new bool[,]
            {
                {false, true, false},
                {true, true, true},
                {false, false,  false}
            }, srsRotationOffset, 2),


            //  XX
            // XX
            new Tetromino(new bool[,]
            {
                {false, true, true},
                {true, true,  false},
                {false, false, false},
            }, srsRotationOffset, 3),


            // XX
            //  XX
            new Tetromino(new bool[,]
            {
                {true, true, false},
                {false, true,  true},
                {false, false, false},
            }, srsRotationOffset, 4),


            // X
            // XXX
            new Tetromino(new bool[,]
            {
                {true, false, false},
                {true, true, true},
                {false, false,  false}
            }, srsRotationOffset, 5),

            //   X
            // XXX
            new Tetromino(new bool[,]
            {
                {false, false, true},
                {true, true, true},
                {false, false,  false}
            }, srsRotationOffset, 6),
        };

        public Board Board { get; private set; }

        public Tetromino HoldTetromino { get; private set; }

        public int NumHiddenRows { get; private set; }

        public Tetromino CurrentTetromino { get; private set; }
        public IList<Tetromino> NextTetrominos { get; private set; }

        public int Level { get; private set; }
        public int Score { get; private set; }

        private readonly Random rnd = new Random();

        private TimeSpan dropTime = new TimeSpan(0);
        private TimeSpan clearTime = new TimeSpan(0);
        private TimeSpan lockTime = new TimeSpan(0);


        public static readonly TimeSpan ClearDelay = new TimeSpan(0, 0, 0, 0, 500);

        public static readonly TimeSpan LockDelay = new TimeSpan(0, 0, 0, 0, 500);

        private bool cancelLockDelay = false;


        private int numClear;

        private bool canHold = true;

        public enum Event
        {
            None,
            Left,
            Right,
            SoftDrop,
            HardDrop,
            RotateRight,
            RotateLeft,
            Hold,
            TogglePause,
            Quit,
        }

        public enum GameState
        {
            NotStarted,
            Running,
            Locking,
            Clearing,
            Paused,
            GameOver,
            Quiting,
        }

        public GameState State { get; private set; }


        public Game(int numRows, int numColumns, int numHiddenRows)
        {
            Board = new Board(numRows, numColumns);
            NumHiddenRows = numHiddenRows;
            State = GameState.Running;
            NextTetrominos = new List<Tetromino>();
            AddRandomTetrominos();
            PickTetromino();
        }

        private void AddRandomTetrominos()
        {
            for (int i = 0; i < tetrominos.Count; i++)
            {
                NextTetrominos.Insert(NextTetrominos.Count - i + rnd.Next(i + 1),
                    new Tetromino(tetrominos[i]));
            }
        }

        private void PickTetromino()
        {
            CurrentTetromino = NextTetrominos[0];

            CurrentTetromino.PositionX = NumHiddenRows + CurrentTetromino.NumRows / 2;
            CurrentTetromino.PositionY = Board.NumColumns / 2 - 1;

            NextTetrominos.RemoveAt(0);
            if (NextTetrominos.Count < tetrominos.Count)
            {
                AddRandomTetrominos();
            }
            if (!Board.DoesTetrominoFit(CurrentTetromino))
            {
                State = GameState.GameOver;
                CurrentTetromino = null;
            }
        }

        private TimeSpan GetGravity()
        {
            // see:  https://tetris.wiki/Tetris_(NES,_Nintendo)
            int frames;
            if (Level <= 7)
            {
                frames = 48 - Level * 5;
            }
            else if (Level == 8)
            {
                frames = 8;
            }
            else if (Level == 9)
            {
                frames = 6;
            }
            else if (Level <= 12)
            {
                frames = 5;
            }
            else if (Level <= 15)
            {
                frames = 4;
            }
            else if (Level <= 18)
            {
                frames = 3;
            }
            else if (Level <= 28)
            {
                frames = 2;
            }
            else
            {
                frames = 1;
            }

            return new TimeSpan((long)(frames / 60.0 * TimeSpan.TicksPerSecond));
        }



        void HardDrop()
        {
            while (Board.DoesTetrominoFit(CurrentTetromino))
            {
                CurrentTetromino.PositionX++;
            }
            CurrentTetromino.PositionX--;
        }

        void SoftDrop()
        {
            CurrentTetromino.PositionX++;
            if (!Board.DoesTetrominoFit(CurrentTetromino))
            {
                CurrentTetromino.PositionX--;
            }
        }

        void Rotate(int d)
        {

            int oldRotation = CurrentTetromino.Rotation;
            int nextRotation = (oldRotation + 4 + d) % 4;
            

            CurrentTetromino.Rotation = nextRotation;

            for (int i = 0; i < CurrentTetromino.SrsRotationOffset.GetLength(1); i++)
            {
                int dx = -(CurrentTetromino.SrsRotationOffset[oldRotation, i, 1] -
                           CurrentTetromino.SrsRotationOffset[nextRotation, i, 1]) ;
                int dy = CurrentTetromino.SrsRotationOffset[oldRotation, i, 0] -
                         CurrentTetromino.SrsRotationOffset[nextRotation, i, 0];

                CurrentTetromino.PositionX += dx;
                CurrentTetromino.PositionY += dy;

                if (Board.DoesTetrominoFit(CurrentTetromino))
                {
                    return;
                }

                CurrentTetromino.PositionX -= dx;
                CurrentTetromino.PositionY -= dy;
            }

            // unable to rotate; give up.
            CurrentTetromino.Rotation = oldRotation;
        }

        void Left()
        {
            CurrentTetromino.PositionY--;
            if (!Board.DoesTetrominoFit(CurrentTetromino))
            {
                CurrentTetromino.PositionY++;
            }
        }


        void Right()
        {
            CurrentTetromino.PositionY++;
            if (!Board.DoesTetrominoFit(CurrentTetromino))
            {
                CurrentTetromino.PositionY--;
            }
        }

        void Hold()
        {
            if (HoldTetromino != null)
            {
                NextTetrominos.Insert(0, HoldTetromino);
            }
            HoldTetromino = CurrentTetromino;
            HoldTetromino.Rotation = 0;
            PickTetromino();
        }


        private void HandleEvent(Event ev, TimeSpan gameTime)
        {
            if (State == GameState.Clearing || State == GameState.GameOver)
            {
                return;
            }
            if (State == GameState.Paused)
            {
                if (ev == Event.TogglePause)
                {
                    State = GameState.Running;
                }
                return;
            }


            switch (ev)
            {
                case Event.Left:
                    Left();
                    break;

                case Event.Right:
                    Right();
                    break;

                case Event.SoftDrop:
                    SoftDrop();
                    dropTime = gameTime;
                    break;

                case Event.HardDrop:
                    HardDrop();
                    cancelLockDelay = true;
                    dropTime = gameTime;
                    break;

                case Event.RotateRight:
                    Rotate(1);
                    break;

                case Event.RotateLeft:
                    Rotate(-1);
                    break;

                case Event.Hold:
                    if (!canHold)
                    {
                        break;
                    }
                    Hold();
                    canHold = false;
                    break;

                case Event.TogglePause:
                    State = GameState.Paused;
                    break;
            }

        }

        private void UpdateScoreLevel(int r)
        {
            int[] bs = { 0, 40, 100, 300, 1200 };

            Score += bs[r] * (Level + 1);

            numClear += r;
            Level = numClear / 10;
        }

        private bool ShouldLock()
        {
            if (CurrentTetromino == null || State != GameState.Running) 
            {
                return false;
            }

            bool result = false;
            CurrentTetromino.PositionX++;
            if (!Board.DoesTetrominoFit(CurrentTetromino))
            {
                result = true;
            }
            CurrentTetromino.PositionX--;
            return result;
        }

        public void Update(Event ev, TimeSpan gameTime)
        {
            HandleEvent(ev, gameTime);
            if (gameTime - dropTime >= GetGravity())
            {
                HandleEvent(Event.SoftDrop, gameTime);
            }


            if (ev == Event.Quit)
            {
                State = GameState.Quiting;
            }

            if (State == GameState.Clearing && gameTime - clearTime >= ClearDelay)
            {
                UpdateScoreLevel(Board.Clear());
                State = GameState.Running;

                dropTime = gameTime;
                PickTetromino();
            }

            if (State == GameState.Locking && (gameTime - lockTime >= LockDelay || cancelLockDelay))
            {
                canHold = true;
                cancelLockDelay = false;
                HardDrop();

                Board.Add(CurrentTetromino);
                if (Board.CanClearLines())
                {
                    CurrentTetromino = null;
                    State = GameState.Clearing;
                    clearTime = gameTime;
                }
                else
                {
                    State = GameState.Running;
                    PickTetromino();
                }
            }


            if (State == GameState.Running && ShouldLock())
            {
                State = GameState.Locking;
                lockTime = gameTime;
                canHold = false;
            }
        }
    }
}

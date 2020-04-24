using System; 
using System.Collections.Generic;

namespace Tetris
{
    class Game
    {

        private static readonly IDictionary<Tuple<int, int>, int[,]> wallKicksTable = new Dictionary<Tuple<int, int>, int[,]>()
        {
            { new Tuple<int, int>(0, 1), new int[,] { {-1, 0}, {-1,+1}, { 0,-2}, {-1,-2} } },
            { new Tuple<int, int>(1, 0), new int[,] { { +1, 0 }, { +1, -1 }, { 0, +2 }, { +1, +2 } } },
            { new Tuple<int, int>(1, 2), new int[,] { {+1, 0}, {+1,-1}, { 0,+2}, {+1,+2} } },
            { new Tuple<int, int>(2, 1), new int[,] { { -1, 0 }, { -1, +1 }, { 0, -2 }, { -1, -2 } } },
            { new Tuple<int, int>(2, 3), new int[,] { { +1, 0 }, { +1, +1 }, { 0, -2 }, { +1, -2 } } },
            { new Tuple<int, int>(3, 2), new int[,] { {-1, 0}, {-1,-1}, { 0,+2}, {-1,+2} } },
            { new Tuple<int, int>(3, 0), new int[,] { {-1, 0}, {-1,-1}, { 0,+2}, {-1,+2} } },
            { new Tuple<int, int>(0, 3), new int[,] { { +1, 0 }, { +1, +1 }, { 0, -2 }, { +1, -2 } } }, 
        };




        private static readonly IDictionary<Tuple<int, int>, int[,]> wallKicksTableI = new Dictionary<Tuple<int, int>, int[,]>()
        {
            { new Tuple<int, int>(0, 1), new int[,] { {-1, 0}, {-1,+1}, { 0,-2}, {-1,-2} } },
            { new Tuple<int, int>(1, 0), new int[,] { { +1, 0 }, { +1, -1 }, { 0, +2 }, { +1, +2 } } },
            { new Tuple<int, int>(1, 2), new int[,] { {+1, 0}, {+1,-1}, { 0,+2}, {+1,+2} } },
            { new Tuple<int, int>(2, 1), new int[,] { { -1, 0 }, { -1, +1 }, { 0, -2 }, { -1, -2 } } },
            { new Tuple<int, int>(2, 3), new int[,] { { +1, 0 }, { +1, +1 }, { 0, -2 }, { +1, -2 } } },
            { new Tuple<int, int>(3, 2), new int[,] { {-1, 0}, {-1,-1}, { 0,+2}, {-1,+2} } },
            { new Tuple<int, int>(3, 0), new int[,] { {-1, 0}, {-1,-1}, { 0,+2}, {-1,+2} } },
            { new Tuple<int, int>(0, 3), new int[,] { { +1, 0 }, { +1, +1 }, { 0, -2 }, { +1, -2 } } },
        };


        private static readonly IList<Tetromino> tetrominos = new List<Tetromino>
        {
            // XXXX
            new Tetromino(new bool[,]
            {
                {false, false, false, false},
                {true, true, true, true},
                {false, false, false, false},
                {false, false, false, false},
            }, 0),
            

            // XX
            // XX
            new Tetromino(new bool[,]
            {
                {false, true, true, false},
                {false, true, true, false},
                {false, false, false, false}
            }, 1),



            // XXX
            //  X
            new Tetromino(new bool[,]
            {
                {false, false, false},
                {true, true, true},
                {false, true,  false}
            }, 2),


            //  XX
            // XX
            new Tetromino(new bool[,]
            {
                {false, true, true},
                {true, true,  false},
                {false, false, false},
            }, 3),


            // XX
            //  XX
            new Tetromino(new bool[,]
            {
                {true, true, false},
                {false, true,  true},
                {false, false, false},
            }, 4),


            // XXX
            //   X
            new Tetromino(new bool[,]
            {
                {false, false, false},
                {true, true, true},
                {false, false,  true}
            }, 5),

            // XXX
            // X
            new Tetromino(new bool[,]
            {
                {false, false, false},
                {true, true, true},
                {true, false,  false}
            }, 6),
        };

        public Board Board { get; private set; }

        public Tetromino HoldTetromino { get; private set; }

        public Tetromino CurrentTetromino { get; private set; }
        public IList<Tetromino> NextTetromino { get; private set;}

        public int Level { get; private set; }
        public int Score { get; private set;  }

        private readonly Random rnd = new Random();

        private TimeSpan lastDropTime = new TimeSpan(0);

        public readonly TimeSpan ClearDelay = new TimeSpan(0, 0, 0, 0, 500);
        
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
            Clearing,
            Paused,
            GameOver,
            Quiting,
        }

        public GameState State { get; private set; }


        public Game(int n, int m)
        {
            Board = new Board(n, m);
            State = GameState.Running;
            NextTetromino = new List<Tetromino>();
            AddRandomTetrominos();
            PickTetromino();
        }
       
        private void AddRandomTetrominos()
        {
            for (int i = 0; i < tetrominos.Count; i++)
            {
                NextTetromino.Insert(NextTetromino.Count - i + rnd.Next(i + 1), 
                    new Tetromino(tetrominos[i]));
            }
        }

        private void PickTetromino()
        {
            CurrentTetromino = NextTetromino[0];
            CurrentTetromino.PositionX =  Math.Max(CurrentTetromino.N, CurrentTetromino.M) / 2;
            CurrentTetromino.PositionY = Board.M / 2;

            NextTetromino.RemoveAt(0);
            if (NextTetromino.Count < tetrominos.Count)
            {
                AddRandomTetrominos();
            }
            if (!Board.Fits(CurrentTetromino))
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


        void TryRotate(int d)
        {
            CurrentTetromino.Rotation += d;
            if (Board.Fits(CurrentTetromino))
            {
                return;
            }

            for (int i = 1; i <= CurrentTetromino.M / 2; i++)
            {
                CurrentTetromino.PositionY -= i;
                if (Board.Fits(CurrentTetromino))
                {
                    return;
                }
                CurrentTetromino.PositionY += i;


                CurrentTetromino.PositionY += i;
                if (Board.Fits(CurrentTetromino))
                {
                    return;
                }
                CurrentTetromino.PositionY -= i;
            }

            // unable to rotate; give up.
            CurrentTetromino.Rotation -= d;
        }



        private void HandleRunningEvent(Event ev)
        {            
            switch (ev)
            {
                case Event.Left:
                    CurrentTetromino.PositionY--;
                    if (!Board.Fits(CurrentTetromino))
                    {
                        CurrentTetromino.PositionY++;
                    }
                    break;

                case Event.Right:
                    CurrentTetromino.PositionY++;
                    if (!Board.Fits(CurrentTetromino))
                    {
                        CurrentTetromino.PositionY--;
                    }

                    break;

                case Event.SoftDrop:
                    CurrentTetromino.PositionX++;
                    if (!Board.Fits(CurrentTetromino))
                    {
                        CurrentTetromino.PositionX--;
                    }
                    break;

                case Event.HardDrop:
                    while (Board.Fits(CurrentTetromino))
                    {
                        CurrentTetromino.PositionX++;
                    }
                    CurrentTetromino.PositionX--;
                    break;

                case Event.RotateRight:
                    TryRotate(1);
                    break;

                case Event.RotateLeft:
                    TryRotate(-1);
                    break;

                case Event.Hold:
                    if (!canHold)
                    {
                        break;
                    }
                    if (HoldTetromino != null) {
                        NextTetromino.Insert(0, HoldTetromino);
                    }
                    HoldTetromino = CurrentTetromino;
                    HoldTetromino.Rotation = 0;
                    PickTetromino();
                    canHold = false;
                    break;

                case Event.TogglePause:
                    State = GameState.Paused;
                    break;
            }
        }

        private void HandleEvent(Event ev)
        {
            if (ev == Event.None)
            {
                return;
            }

            switch (State)
            {
                case GameState.Running:
                    HandleRunningEvent(ev);
                    break;
                case GameState.Paused:
                    if (ev == Event.TogglePause)
                    {
                        State = GameState.Running;
                    }
                    break;
            }

        }

        private void UpdateScoreLevel(int r)
        {
            int[] bs = {0, 40, 100, 300, 1200};

            Score += bs[r] * (Level + 1);

            numClear += r;
            Level = numClear / 10;
         }

        public void Update(Event ev, TimeSpan gameTime)
        {
            HandleEvent(ev);
            if (ev == Event.Quit)
            {
                State = GameState.Quiting;
            }         
            if (State == GameState.Clearing && gameTime - lastDropTime >= ClearDelay)
            {
                UpdateScoreLevel(Board.Clear());
                lastDropTime = gameTime;
                State = GameState.Running;
                PickTetromino();
            }
            if (State == GameState.Running && (gameTime - lastDropTime >= GetGravity() || ev == Event.HardDrop))
            {
                lastDropTime = gameTime;
                CurrentTetromino.PositionX++; ;
                if (!Board.Fits(CurrentTetromino))
                {
                    canHold = true;
                    CurrentTetromino.PositionX--;
                    Board.Add(CurrentTetromino);
                    if (Board.CanClear())
                    {
                        CurrentTetromino = null;
                        State = GameState.Clearing;
                    }
                    else PickTetromino();
                }
            }
        }
    }
}

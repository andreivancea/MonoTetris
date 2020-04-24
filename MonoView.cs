using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace Tetris
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MonoView : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D whiteRectangle;
        Tetris.Game game;

        SpriteFont font;
        SpriteFont fontLarge;


        private const int blockSize = 50;
        private const int boardWidthBlocks = 10;
        private const int boardHeightBlocks = 20;

        private const int boardWidth = boardWidthBlocks * blockSize;
        private const int boardHeight = boardHeightBlocks * blockSize;

        private const int rightBorder = 5 * blockSize;


        private readonly Color[] colors =
        {
            Color.Cyan,
            Color.Yellow,
            Color.Purple,
            Color.Green,
            Color.Red,
            Color.Blue,
            Color.Orange,
        };


        public MonoView()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = boardWidth + rightBorder;
            graphics.PreferredBackBufferHeight = boardHeight;
            Content.RootDirectory = "Content";
            game = new Tetris.Game(boardHeightBlocks, boardWidthBlocks);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("digi");
            fontLarge = Content.Load<SpriteFont>("digilarge");

            whiteRectangle = new Texture2D(GraphicsDevice, 1, 1);
            whiteRectangle.SetData(new[] { Color.White });
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
        }


        private readonly IDictionary<Keys, TimeSpan> keyDownTime = new Dictionary<Keys, TimeSpan>();
        private readonly TimeSpan keyRepeatTime = new TimeSpan(0, 0, 0, 0, 200);

        private ISet<Keys> KeysPressed(KeyboardState currentKeyboardState, TimeSpan gameTime)
        {
            ISet<Keys> result = new HashSet<Keys>();

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (!currentKeyboardState.IsKeyDown(key))
                {
                    keyDownTime.Remove(key);
                    continue;
                }
                if (!keyDownTime.ContainsKey(key) || gameTime - keyDownTime[key] >= keyRepeatTime)
                {
                    result.Add(key);
                    keyDownTime[key] = gameTime;
                }
            }
            return result;
        }


        private readonly IDictionary<Keys, Tetris.Game.Event> keyToEvent = new Dictionary<Keys, Tetris.Game.Event>()
            {
                {Keys.Left,  Tetris.Game.Event.Left}, 
                {Keys.Right, Tetris.Game.Event.Right},
                {Keys.Down, Tetris.Game.Event.SoftDrop},
                {Keys.Up, Tetris.Game.Event.RotateRight},
                {Keys.Z,  Tetris.Game.Event.RotateLeft},
                {Keys.Space, Tetris.Game.Event.HardDrop},
                {Keys.Enter,  Tetris.Game.Event.HardDrop},
                {Keys.H,  Tetris.Game.Event.Hold},
                {Keys.P,  Tetris.Game.Event.TogglePause},
            };
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (gameTime == null)
            {
                throw new ArgumentNullException(nameof(gameTime));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var keysPressed = KeysPressed(Keyboard.GetState(), gameTime.TotalGameTime);

            var ev = Tetris.Game.Event.None;
            foreach (var item in keyToEvent)
            {
                if (keysPressed.Contains(item.Key))
                {
                    ev = item.Value;
                    break;
                }
            }
            game.Update(ev, gameTime.TotalGameTime);
            base.Update(gameTime);
        }


        private TimeSpan clearStartTime = new TimeSpan(0);

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (gameTime == null)
            {
                throw new ArgumentNullException(nameof(gameTime));
            }

            graphics.GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            DrawGrid();
            DrawShadow(gameTime);
            DrawCurrentTetromino();
            DrawBoard(gameTime);
            DrawNextTetrominos();
            DrawHold();
            DrawInfo();

            spriteBatch.End();


            base.Draw(gameTime);
        }


        private  void DrawInfo()
        {
            spriteBatch.DrawString(font, String.Format(CultureInfo.CurrentCulture, "{0,7}", "L" + game.Level), 
                new Vector2((int)(boardWidth + 0.4 * blockSize), (int)(0.75 * blockSize)), Color.Green);
            spriteBatch.DrawString(font, String.Format(CultureInfo.CurrentCulture, "{0,7}", game.Score), 
                new Vector2((int)(boardWidth + 0.4 * blockSize), (int)(1.75 * blockSize)), Color.Green);


            string msg = "";
            if (game.State == Tetris.Game.GameState.Paused)
            {
                msg = MonoTetris.Properties.Resources.Pause;
            }
            if (game.State == Tetris.Game.GameState.GameOver)
            {
                msg = MonoTetris.Properties.Resources.GameOver;
            }

            if (!String.IsNullOrEmpty(msg))
            {
                var m = fontLarge.MeasureString(msg);
                spriteBatch.DrawString(fontLarge, msg, 
                    new Vector2(boardWidth / 2 - m.X / 2, boardHeight / 2 - m.Y / 2), Color.White);

            }
        }

        private void DrawGrid()
        {
            spriteBatch.Draw(whiteRectangle, new Rectangle(0, 0, boardWidth, boardHeight), 
                Color.Lerp(Color.White, Color.Black, 0.80f));
            for (int i = 0; i < boardHeightBlocks; i++)
                for (int j = 0; j < boardWidthBlocks; j++)
                    spriteBatch.Draw(whiteRectangle, new Rectangle(blockSize * j , i * blockSize ,   
                            blockSize - 1, blockSize - 1), Color.Black);
        }


        private void DrawTetramino(Tetromino t, Color color, int bs = blockSize, int x = 0, int y = 0)
        {
            for (int i = 0; i < t.N; i++)
                for (int j = 0; j < t.M; j++)
                {
                    if (!t.Block(i, j))
                    {
                        continue;
                    }
                    int xx = x + j * bs;
                    int yy = y + i * bs;
                    int width = bs - 1;
                    int height = bs - 1;
                    spriteBatch.Draw(whiteRectangle, 
                           new Rectangle(xx, yy, width, height), color);
                }
        }

        private void DrawCurrentTetromino()
        {
            if (game.State == Tetris.Game.GameState.Clearing || game.State == Tetris.Game.GameState.GameOver)
            {
                return;
            }
            var col = colors[game.CurrentTetromino.Color];
            if (game.State == Tetris.Game.GameState.GameOver || game.State == Tetris.Game.GameState.Paused)
            {
                col = Color.Lerp(col, Color.Black, 0.7f);
            }

            DrawTetramino(game.CurrentTetromino, col,
                blockSize,
                (game.CurrentTetromino.PositionY - game.CurrentTetromino.M / 2) * blockSize,
                (game.CurrentTetromino.PositionX - game.CurrentTetromino.N / 2) * blockSize);
        }

        private void DrawShadow(GameTime gameTime)
        {
            if (game.State == Tetris.Game.GameState.Clearing || game.State == Tetris.Game.GameState.GameOver || game.State == Tetris.Game.GameState.Paused)
            {
                return;
            }
            var shadow = new Tetromino(game.CurrentTetromino);
            
            while (game.Board.Fits(shadow))
            {
                shadow.PositionX++;
            }
            shadow.PositionX--;

            float amount = 0.80f;
            if ((gameTime.TotalGameTime.Ticks /  (TimeSpan.TicksPerMillisecond * 100)) % 2 == 0)
            {
                amount = 0.83f;
            }

            DrawTetramino(shadow, Color.Lerp(Color.White, Color.Black, amount),
                blockSize,
                (shadow.PositionY - shadow.M / 2) * blockSize,
                (shadow.PositionX - shadow.N / 2) * blockSize);
        }

        private void DrawBoard(GameTime gameTime)
        {
            if (game.State == Tetris.Game.GameState.Clearing)
            {
                if (clearStartTime.Ticks == 0)
                {
                    clearStartTime = gameTime.TotalGameTime;
                }
            }
            else
            {
                clearStartTime = new TimeSpan(0);
            }

            var yy = blockSize * game.Board.N;
            for (int i = game.Board.N - 1; i >= 0; i--)
            {
                var w = blockSize;
                var h = blockSize;

                if (game.Board.LineFull(i) && game.State == Tetris.Game.GameState.Clearing)
                {                            
                    h = (int)(blockSize * (1.0 - (gameTime.TotalGameTime - clearStartTime).TotalSeconds / game.ClearDelay.TotalSeconds));
                }

                yy -= h;
                for (int j = 0; j < game.Board.M; j++)
                    if (game.Board.Block[i][j])
                    {
                        var col = colors[game.Board.Color[i][j]];
                        if (game.State == Tetris.Game.GameState.GameOver || game.State == Tetris.Game.GameState.Paused)
                        {
                            col = Color.Lerp(col, Color.Black, 0.7f);
                        }

                        spriteBatch.Draw(whiteRectangle, new Rectangle(w * j, yy, w - 1, h - 1), col);
                    }
            }

        }

        private void DrawNextTetrominos()
        {


            var bs = (int)(blockSize * 0.80);
            var yy = (int)(4.5 * blockSize);


            var m = font.MeasureString(MonoTetris.Properties.Resources.Next);
            spriteBatch.DrawString(font, MonoTetris.Properties.Resources.Next,
                new Vector2((int)(boardWidth + 0.5 * rightBorder - 0.5 * m.X), yy), Color.Green);

            yy += (int)(2.0 * blockSize);

            for (int i = 0;   i < Math.Min(3, game.NextTetromino.Count); i++)
            {
                var t = game.NextTetromino[i];
                var color = colors[t.Color];
                for (int l = 0; l < t.N; l++)
                {
                    if (t.LineEmpty(l))
                    {
                        yy -= bs;
                    }
                    else
                    {
                        break;
                    }
                }
                if (game.State == Tetris.Game.GameState.GameOver || game.State == Tetris.Game.GameState.Paused)
                {
                    color = Color.Lerp(color, Color.Black, 0.7f);
                }
                DrawTetramino(t, color, bs, boardWidth + rightBorder / 2 - t.M * bs / 2, yy);
                yy += (t.N + 1) * bs;
                for (int l = t.N - 1; l >= 0; l--)
                {
                    if (t.LineEmpty(l))
                    {
                        yy -= bs;
                    } else
                    {
                        break;
                    }
                }
            }
        }

        private  void DrawHold()
        {
            var bs = (int)(blockSize * 0.80);
            var yy = (int)(15.0 * blockSize); ;


            var m = font.MeasureString(MonoTetris.Properties.Resources.Hold);
            spriteBatch.DrawString(font, MonoTetris.Properties.Resources.Hold,
                new Vector2((int)(boardWidth + 0.5 * rightBorder - 0.5 * m.X), yy), Color.Green);

            var h = game.HoldTetromino;
            if (h == null)
            {
                return;
            }

            yy += (int)(2.0 * blockSize);

            var color = colors[h.Color];
            for (int l = 0; l < h.N; l++)
            {
                if (h.LineEmpty(l))
                {
                    yy -= bs;
                }
                else
                {
                    break;
                }
            }

            if (game.State == Tetris.Game.GameState.GameOver || game.State == Tetris.Game.GameState.Paused)
            {
                color = Color.Lerp(color, Color.Black, 0.7f);
            }
            DrawTetramino(h, color, bs, boardWidth + rightBorder / 2 - h.M * bs / 2, yy);

        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TRexRunner.Entities;
using TRexRunner.Extensions;
using TRexRunner.System;

namespace TRexRunner;

public class TRexRunnerGame : Game
{
    public enum DisplayMode
    {
        Default,
        Zoomed
    }

    public const string GAME_TITLE = "T-Rex Runner";

    //storing constants for our assets in the content pipeline
    private const string ASSET_NAME_SPRITESHEET = "TrexSpritesheet";
    private const string ASSET_NAME_SFX_HIT = "hit";
    private const string ASSET_NAME_SFX_SCORE_REACHED = "score-reached";
    private const string ASSET_NAME_SFX_BUTTON_PRESS = "button-press";

    public const int WINDOW_WIDTH = 600;
    public const int WINDOW_HEIGHT = 150;

    public const int TREX_START_POS_Y = WINDOW_HEIGHT - 16;
    public const int TREX_START_POS_X = 1;
    private const float FADE_IN_ANIMATION_SPEED = 820f;

    private const int SCORE_BOARD_POS_X = WINDOW_WIDTH - 130;
    private const int SCORE_BOARD_POS_Y = 10;
    private const string SAVE_FILE_NAME = "Save.dat";
    private const int DISPLAY_ZOOM_FACTOR = 2;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    //in a larger game, this should probably be a separate asset manager class that handles this
    private SoundEffect _sfxHit;
    private SoundEffect _sfxScoreReached;
    private SoundEffect _sfxButtonPress;
    private Texture2D _spriteSheetTexture;
    private Texture2D _invertedSpriteSheet;
    private Texture2D _fadeInTexture;
    private float _fadeInTexturePosX;
    private InputController _inputController;
    private EntityManager _entityManager;
    private GroundManager _groundManager;
    private Trex _trex;
    private ScoreBoard _scoreBoard;
    private KeyboardState _previousKeyboardState;
    private ObstacleManager _obstacleManager;
    private GameOverScreen _gameOverScreen;
    private SkyManager _skyManager;
    private DateTime _highscoreDate;
    private Matrix _transformMatrix = Matrix.Identity;

    public GameState State { get; private set; }
    public DisplayMode WindowDisplayMode { get; set; } = DisplayMode.Default;

    public float ZoomFactor => WindowDisplayMode is DisplayMode.Default ? 1 : DISPLAY_ZOOM_FACTOR;

    public TRexRunnerGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _entityManager = new EntityManager();
        State = GameState.Initial;
        _fadeInTexturePosX =
            Trex.TREX_DEFAULT_SPRITE_WIDTH; // we can start it right after the trex sprite ends, so everything to trex right is white
    }

    protected override void Initialize()
    {
        base.Initialize();

        _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
        _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
        _graphics.SynchronizeWithVerticalRetrace = true; //vsync 
        _graphics.ApplyChanges();

        Window.Title = GAME_TITLE;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        //load our assets
        //notably we're loading the sounds as SoundEffects, this has to match the processor
        //that we have set in the mgcb-editor tool. by default, it originally chose song for mp3s
        _sfxButtonPress = Content.Load<SoundEffect>(ASSET_NAME_SFX_BUTTON_PRESS);
        _sfxHit = Content.Load<SoundEffect>(ASSET_NAME_SFX_HIT);
        _sfxScoreReached = Content.Load<SoundEffect>(ASSET_NAME_SFX_SCORE_REACHED);

        _spriteSheetTexture = Content.Load<Texture2D>(ASSET_NAME_SPRITESHEET);
        _invertedSpriteSheet =
            _spriteSheetTexture.InvertColors(Color.Transparent); //inverting the transparency ends up weird
        _fadeInTexture = new Texture2D(GraphicsDevice, 1, 1);
        _fadeInTexture.SetData(new[] { Color.White }); //texture of 1x1 pixel, that is White

        //offsetting the position here by it's height as we draw from the top left corner
        //if we didn't, the top left corner of the dino would be drawn there, quite low on the screen_
        _trex = new Trex(_spriteSheetTexture,
            new Vector2(TREX_START_POS_X, TREX_START_POS_Y - Trex.TREX_DEFAULT_SPRITE_HEIGHT), _sfxButtonPress);
        _trex.DrawOrder = 10; //ensure drawn on top of the ground
        //subscribe to the JumpComplete event on the trex, trigger this method when the event fires
        _trex.JumpComplete += TrexOnJumpComplete;
        _trex.Died += TrexOnDied;

        _scoreBoard = new ScoreBoard(_spriteSheetTexture, new Vector2(SCORE_BOARD_POS_X, SCORE_BOARD_POS_Y), _trex,
            _sfxScoreReached);
        _inputController = new InputController(_trex);
        _groundManager = new GroundManager(_spriteSheetTexture, _entityManager, _trex);
        _obstacleManager = new ObstacleManager(_entityManager, _trex, _scoreBoard, _spriteSheetTexture);
        _gameOverScreen = new GameOverScreen(_spriteSheetTexture,
            new Vector2(WINDOW_WIDTH / 2 - GameOverScreen.GAME_OVER_SPRITE_WIDTH / 2, WINDOW_HEIGHT / 2 - 30), this);
        _skyManager = new SkyManager(_trex, _spriteSheetTexture, _invertedSpriteSheet, _entityManager, _scoreBoard);

        _entityManager.AddEntity(_trex);
        _entityManager.AddEntity(_groundManager);
        _entityManager.AddEntity(_scoreBoard);
        _entityManager.AddEntity(_obstacleManager);
        _entityManager.AddEntity(_gameOverScreen);
        _entityManager.AddEntity(_skyManager);

        _groundManager.Initialize();

        LoadGame();
    }

    private void TrexOnDied(object sender, EventArgs e)
    {
        State = GameState.GameOver;
        _obstacleManager.IsEnabled = false;
        _gameOverScreen.IsEnabled = true;
        _sfxHit.Play();

        if (_scoreBoard.DisplayScore > _scoreBoard.HiScore)
        {
            Debug.WriteLine("New highscore set!: " + _scoreBoard.DisplayScore);
            _scoreBoard.HiScore = _scoreBoard.DisplayScore;
            _highscoreDate = DateTime.Now;

            SaveGame();
        }
    }

    private void TrexOnJumpComplete(object sender, EventArgs e)
    {
        if (State == GameState.Transition)
        {
            State = GameState.Playing;
            _trex.Initialize();
            _obstacleManager.IsEnabled = true;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);

        KeyboardState keyboardState = Keyboard.GetState();

        if (State == GameState.Playing)
            _inputController.ProcessControls(gameTime);
        else if (State == GameState.Transition)
        {
            _fadeInTexturePosX += (float)gameTime.ElapsedGameTime.TotalSeconds * FADE_IN_ANIMATION_SPEED;
        }
        else if (State == GameState.Initial)
        {
            var isStartKeyPressed = keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.Space);
            var wasStartKeyPressed = _previousKeyboardState.IsKeyDown(Keys.Up) ||
                                     _previousKeyboardState.IsKeyDown(Keys.Space);

            if (isStartKeyPressed && !wasStartKeyPressed)
            {
                StartGame();
            }
        }


        _entityManager.Update(gameTime);

        //secret button to reset the highscore that's saved
        if (keyboardState.IsKeyDown(Keys.F12) && !_previousKeyboardState.IsKeyDown(Keys.F12))
        {
            ResetSaveState();
        }
        
        //secret display changing button
        if (keyboardState.IsKeyDown(Keys.F2) && !_previousKeyboardState.IsKeyDown(Keys.F2))
        {
            ToggleDisplayMode();
        }

        _previousKeyboardState = keyboardState;
    }

    protected override void Draw(GameTime gameTime)
    {
        //if skymanager isn't null, use its clearcolor which might be white or black
        //depending on our score and if its night
        GraphicsDevice.Clear(_skyManager?.ClearColor ?? Color.White);

        //PointClamp makes sure the positioning of texture coordinates is always snapped to pixels, and doesn't move in
        //subpixel areas
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _transformMatrix);

        _entityManager.Draw(_spriteBatch, gameTime);

        if (State is GameState.Transition or GameState.Initial)
        {
            _spriteBatch.Draw(_fadeInTexture,
                new Rectangle((int)Math.Round(_fadeInTexturePosX), 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private bool StartGame()
    {
        if (State != GameState.Initial)
            return false;

        //debugging
        //_scoreBoard.score = 99_900;

        State = GameState.Transition;
        _trex.BeginJump();

        return true;
    }

    public bool Replay()
    {
        //can't restart if not game over'd
        if (State != GameState.GameOver)
            return false;

        State = GameState.Playing;
        _trex.Initialize();
        _obstacleManager.Reset();
        _obstacleManager.IsEnabled = true;
        _gameOverScreen.IsEnabled = false;
        _scoreBoard.Score = 0;
        _groundManager.Initialize();

        return true;
    }

    private void SaveGame()
    {
        var saveState = new SaveState()
        {
            HighScore = _scoreBoard.HiScore,
            HighScoreDate = _highscoreDate
        };

        try
        {
            //just class object to json to binary to base64 string to file
            using var fileStream = new FileStream(SAVE_FILE_NAME, FileMode.OpenOrCreate);
            var writer = new BinaryWriter(fileStream);
            var plainTextBytes = JsonSerializer.SerializeToUtf8Bytes(saveState);
            writer.Write(Convert.ToBase64String(plainTextBytes));
            writer.Flush();
        }
        catch (Exception e)
        {
            Debug.WriteLine("Unable to save game, error: " + e.Message);
        }
    }

    private void LoadGame()
    {
        try
        {
            //read the string, convert from base64 to bytes, deserialize the bytes to an object
            using var fileStream = new FileStream(SAVE_FILE_NAME, FileMode.OpenOrCreate);
            var reader = new BinaryReader(fileStream);
            var json = Convert.FromBase64String(reader.ReadString());
            var state = JsonSerializer.Deserialize<SaveState>(json);
            reader.Close();

            //aaaand set the highscore
            if (state != null && _scoreBoard != null)
            {
                _scoreBoard.HiScore = state.HighScore;
                _highscoreDate = state.HighScoreDate;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Unable to load save game, error: " + e.Message);
        }
    }

    private void ResetSaveState()
    {
        //reset the highscore
        _scoreBoard.HiScore = 0;
        _highscoreDate = default(DateTime);
        SaveGame();
    }

    private void ToggleDisplayMode()
    {
        if (WindowDisplayMode is DisplayMode.Default)
        {
            WindowDisplayMode = DisplayMode.Zoomed;

            _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT * DISPLAY_ZOOM_FACTOR;
            _graphics.PreferredBackBufferWidth = WINDOW_WIDTH * DISPLAY_ZOOM_FACTOR;

            //scale x and y, don't scale z because no z
            _transformMatrix = Matrix.Identity * Matrix.CreateScale(DISPLAY_ZOOM_FACTOR, DISPLAY_ZOOM_FACTOR, 1);
        }
        else
        {
            WindowDisplayMode = DisplayMode.Default;
            
            _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            
            _transformMatrix = Matrix.Identity;
        }
        
        _graphics.ApplyChanges();
    }
}
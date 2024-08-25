using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TRexRunner.Entities;
using TRexRunner.Graphics;
using TRexRunner.System;

namespace TRexRunner;

public class TRexRunnerGame : Game
{
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

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    //in a larger game, this should probably be a separate asset manager class that handles this
    private SoundEffect _sfxHit;
    private SoundEffect _sfxScoreReached;
    private SoundEffect _sfxButtonPress;

    private Texture2D _spriteSheetTexture;
    private Texture2D _fadeInTexture;

    private float _fadeInTexturePosX;

    private InputController _inputController;

    private EntityManager _entityManager;
    private GroundManager _groundManager;

    private Trex _trex;

    private ScoreBoard _scoreBoard;

    private KeyboardState _previousKeyboardState;

    public GameState State { get; private set; }

    public TRexRunnerGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _entityManager = new EntityManager();
        State = GameState.Initial;
        _fadeInTexturePosX = Trex.TREX_DEFAULT_SPRITE_WIDTH; // we can start it right after the trex sprite ends, so everything to trex right is white
    }

    protected override void Initialize()
    {
        base.Initialize();

        _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
        _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
        _graphics.ApplyChanges();
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
        _fadeInTexture = new Texture2D(GraphicsDevice, 1, 1);
        _fadeInTexture.SetData(new[] { Color.White }); //texture of 1x1 pixel, that is White

        //offsetting the position here by it's height as we draw from the top left corner
        //if we didn't, the top left corner of the dino would be drawn there, quite low on the screen_
        _trex = new Trex(_spriteSheetTexture,
            new Vector2(TREX_START_POS_X, TREX_START_POS_Y - Trex.TREX_DEFAULT_SPRITE_HEIGHT), _sfxButtonPress);
        _trex.DrawOrder = 10; //ensure drawn on top of the ground
        
        //subscribe to the JumpComplete event on the trex, trigger this method when the event fires
        _trex.JumpComplete += TrexOnJumpComplete;

        _scoreBoard = new ScoreBoard(_spriteSheetTexture, new Vector2(SCORE_BOARD_POS_X, SCORE_BOARD_POS_Y), _trex);

        _inputController = new InputController(_trex);
        _groundManager = new GroundManager(_spriteSheetTexture, _entityManager, _trex);
        
        _entityManager.AddEntity(_trex);
        _entityManager.AddEntity(_groundManager);
        _entityManager.AddEntity(_scoreBoard);
        
        _groundManager.Initialize();
    }

    private void TrexOnJumpComplete(object sender, EventArgs e)
    {
        if (State == GameState.Transition)
        {
            State = GameState.Playing;
            _trex.Initialize();
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
            var wasStartKeyPressed = _previousKeyboardState.IsKeyDown(Keys.Up) || _previousKeyboardState.IsKeyDown(Keys.Space);
            
            if (isStartKeyPressed && !wasStartKeyPressed)
            {
                StartGame();
            }
        }
        
        
        _entityManager.Update(gameTime);

        _previousKeyboardState = keyboardState;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);
        _spriteBatch.Begin();

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

        State = GameState.Transition;
        _trex.BeginJump();

        return true;
    }
}
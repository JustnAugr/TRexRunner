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

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    //in a larger game, this should probably be a separate asset manager class that handles this
    private SoundEffect _sfxHit;
    private SoundEffect _sfxScoreReached;
    private SoundEffect _sfxButtonPress;

    private Texture2D _spriteSheetTexture;

    private InputController _inputController;

    private EntityManager _entityManager;
    private GroundManager _groundManager;

    public GameState State { get; private set; }

    public TRexRunnerGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _entityManager = new EntityManager();
        State = GameState.Initial;
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

        //offsetting the position here by it's height as we draw from the top left corner
        //if we didn't, the top left corner of the dino would be drawn there, quite low on the screen
        var trex = new Trex(_spriteSheetTexture,
            new Vector2(TREX_START_POS_X, TREX_START_POS_Y - Trex.TREX_DEFAULT_SPRITE_HEIGHT), _sfxButtonPress);
        trex.DrawOrder = 10; //ensure drawn on top of the ground

        _entityManager.AddEntity(trex);
        _inputController = new InputController(trex);
        _groundManager = new GroundManager(_spriteSheetTexture, _entityManager, trex);
        _entityManager.AddEntity(_groundManager);
        _groundManager.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);

        _inputController.ProcessControls(gameTime);

        _entityManager.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);

        _spriteBatch.Begin();
        _entityManager.Draw(_spriteBatch, gameTime);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
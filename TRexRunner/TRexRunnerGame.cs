using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TRexRunner.Entities;
using TRexRunner.Graphics;

namespace TRexRunner;

public class TRexRunnerGame : Game
{
    //storing constants for our assets in the content pipeline
    private const string ASSET_NAME_SPRITESHEET = "TrexSpritesheet";
    private const string ASSET_NAME_SFX_HIT = "hit";
    private const string ASSET_NAME_SFX_SCORE_REACHED = "score-reached";
    private const string ASSET_NAME_SFX_BUTTON_PRESS = "button-press";

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    //in a larger game, this should probably be a separate asset manager class that handles this
    private SoundEffect _sfxHit;
    private SoundEffect _sfxScoreReached;
    private SoundEffect _sfxButtonPress;

    private Texture2D _spriteSheetTexture;

    private Trex _trex;

    public TRexRunnerGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
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

        _trex = new Trex(_spriteSheetTexture, new Vector2(20, 20));
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _trex.Draw(_spriteBatch, gameTime);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
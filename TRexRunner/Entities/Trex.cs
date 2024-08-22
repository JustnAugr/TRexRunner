using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TRexRunner.Graphics;

namespace TRexRunner.Entities;

public class Trex : IGameEntity
{
    private const int TREX_IDLE_BACKGROUND_SPRITE_POS_X = 40;
    private const int TREX_IDLE_BACKGROUND_SPRITE_POS_Y = 0;

    public const int TREX_DEFAULT_SPRITE_POS_X = 848;
    public const int TREX_DEFAULT_SPRITE_POS_Y = 0;
    public const int TREX_DEFAULT_SPRITE_WIDTH = 44;
    public const int TREX_DEFAULT_SPRITE_HEIGHT = 52;

    private const float BLINK_ANIMATION_RANDOM_MIN = 2f;
    private const float BLINK_ANIMATION_RANDOM_MAX = 10f;
    private const float BLINK_ANIMATION_EYE_CLOSE_TIME = .5f;

    private Sprite _idleBackgroundSprite;
    private SpriteAnimation _blinkAnimation;

    private Sprite _idleSprite;
    private Sprite _idleBlinkSprite;

    public Vector2 Position { get; set; }
    
    public TrexState State { get; private set; }

    public bool IsAlive { get; private set; }

    public float Speed { get; private set; }

    public int DrawOrder { get; set; }

    private Random _random;

    public Trex(Texture2D spriteSheet, Vector2 position)
    {
        Position = position;
        //little idle sprite that has ground drawn with it
        _idleBackgroundSprite = new Sprite(spriteSheet, TREX_IDLE_BACKGROUND_SPRITE_POS_X,
            TREX_IDLE_BACKGROUND_SPRITE_POS_Y, TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT);
        State = TrexState.Idle;

        _random = new Random();

        _idleSprite = new Sprite(spriteSheet, TREX_DEFAULT_SPRITE_POS_X, TREX_DEFAULT_SPRITE_POS_Y,
            TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT);
        _idleBlinkSprite = new Sprite(spriteSheet, TREX_DEFAULT_SPRITE_POS_X + TREX_DEFAULT_SPRITE_WIDTH,
            TREX_DEFAULT_SPRITE_POS_Y,
            TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT);

        CreateBlinkAnimation();
        _blinkAnimation.Play();
    }

    public void Update(GameTime gameTime)
    {
        if (State == TrexState.Idle)
        {
            //if it stopped after playing once, queue another one
            if (!_blinkAnimation.IsPlaying)
            {
                CreateBlinkAnimation();
                _blinkAnimation.Play();
            }
            
            _blinkAnimation.Update(gameTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (State == TrexState.Idle)
        {
            //if we're idle, draw the idle sprite in the background so that we can see the ground and blink
            _idleBackgroundSprite.Draw(spriteBatch, Position);
            _blinkAnimation.Draw(spriteBatch, Position);
        }
    }

    private void CreateBlinkAnimation()
    {
        _blinkAnimation = new SpriteAnimation();
        _blinkAnimation.ShouldLoop = false;

        //rand between 0-8, then +2 should give us value between 2 and 10 (seconds)
        double blinkTimeStamp = BLINK_ANIMATION_RANDOM_MIN +
                                _random.NextDouble() * (BLINK_ANIMATION_RANDOM_MAX - BLINK_ANIMATION_RANDOM_MIN);

        _blinkAnimation.AddFrame(_idleSprite, 0);
        //next frame is one width over in the sheet so we add that to the x pos
        _blinkAnimation.AddFrame(_idleBlinkSprite, (float)blinkTimeStamp);
        //our fake frame to signal the end of the animation via the Duration() call
        //here we're adding how long the blink should last
        _blinkAnimation.AddFrame(_idleSprite, (float)blinkTimeStamp + BLINK_ANIMATION_EYE_CLOSE_TIME);
    }
}
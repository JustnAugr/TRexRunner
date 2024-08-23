﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using TRexRunner.Graphics;

namespace TRexRunner.Entities;

public class Trex : IGameEntity
{
    private const float JUMP_START_VELOCITY = -480f; //pixels per second, negative bc up from down
    private const float GRAVITY = 1600f;
    private const float CANCEL_JUMP_VELOCITY = -100f;
    private const float MIN_JUMP_HEIGHT = 40; //in pixels

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

    private SoundEffect _jumpSound;

    public Vector2 Position { get; set; }

    public TrexState State { get; private set; }

    public bool IsAlive { get; private set; }

    public float Speed { get; private set; }

    public int DrawOrder { get; set; }

    private Random _random;

    private float _verticalVelocity;

    private float _startPosY;

    public Trex(Texture2D spriteSheet, Vector2 position, SoundEffect jumpSound)
    {
        Position = position;
        //little idle sprite that has ground drawn with it
        _idleBackgroundSprite = new Sprite(spriteSheet, TREX_IDLE_BACKGROUND_SPRITE_POS_X,
            TREX_IDLE_BACKGROUND_SPRITE_POS_Y, TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT);
        State = TrexState.Idle;

        _jumpSound = jumpSound;

        _random = new Random();

        _idleSprite = new Sprite(spriteSheet, TREX_DEFAULT_SPRITE_POS_X, TREX_DEFAULT_SPRITE_POS_Y,
            TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT);
        _idleBlinkSprite = new Sprite(spriteSheet, TREX_DEFAULT_SPRITE_POS_X + TREX_DEFAULT_SPRITE_WIDTH,
            TREX_DEFAULT_SPRITE_POS_Y,
            TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT);

        _blinkAnimation = new SpriteAnimation();
        CreateBlinkAnimation();
        _blinkAnimation.Play();

        _startPosY = position.Y;
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
        else if (State is TrexState.Jumping or TrexState.Falling)
        {
            //scale by time to be frame independent, and making sure our velocity is in measure of pixels PER SECOND, not dependent on frames
            //this is what sends us to the ground (or up to the sky :])
            Position = new Vector2(Position.X,
                Position.Y + _verticalVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds);
            _verticalVelocity += GRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //make sure we set ourselves as falling if our velocity has changed to positive (sending us downward)
            if (_verticalVelocity >= 0)
                State = TrexState.Falling;
            
            //so we don't fall through the floor because of gravity, we essentially cap how low our y position can go
            if (Position.Y >= _startPosY)
            {
                Position = new Vector2(Position.X, _startPosY);
                _verticalVelocity = 0;
                State = TrexState.Idle; //should probably be running later
            }
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
        else if (State is TrexState.Jumping or TrexState.Falling)
        {
            _idleSprite.Draw(spriteBatch, Position);
        }
    }

    private void CreateBlinkAnimation()
    {
        _blinkAnimation.Clear(); //allows us to reuse same object
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

    public bool BeginJump()
    {
        if (State is TrexState.Jumping or TrexState.Falling)
            return false;

        _jumpSound.Play();
        State = TrexState.Jumping;
        _verticalVelocity = JUMP_START_VELOCITY;

        return true;
    }

    public bool CancelJump()
    {
        if (State != TrexState.Jumping || (_startPosY - Position.Y) < MIN_JUMP_HEIGHT)
            return false;

        State = TrexState.Falling;
        //rather than immediately sending us to 0 velocity which will throw us to the ground really quickly,
        //just set a higher velocity that will have us head to the ground faster
        //if we're closer to 0 already (falling faster) then send us to 0
        _verticalVelocity = _verticalVelocity < CANCEL_JUMP_VELOCITY ? CANCEL_JUMP_VELOCITY : 0;
        
        return true;
    }
}
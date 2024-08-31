using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using TRexRunner.Graphics;

namespace TRexRunner.Entities;

public class Trex : IGameEntity, ICollidable, ITextureInvertible
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

    private const int TREX_RUNNING_SPRITE_ONE_POS_X = TREX_DEFAULT_SPRITE_POS_X + TREX_DEFAULT_SPRITE_WIDTH * 2;
    private const int TREX_RUNNING_SPRITE_ONE_POS_Y = 0;
    private const float RUN_ANIMATION_FRAME_LENGTH = 1 / 10f; //10fps, 1 frame per 10 seconds

    private const int TREX_DUCKING_SPRITE_WIDTH = 59;
    private const int TREX_DUCKING_SPRITE_ONE_POS_X = TREX_DEFAULT_SPRITE_POS_X + TREX_DEFAULT_SPRITE_WIDTH * 6;
    private const int TREX_DUCKING_SPRITE_ONE_POS_Y = 0;
    private const float DROP_VELOCITY = 600f;

    private const int TREX_DEAD_SPRITE_POS_X = 1068;
    private const int TREX_DEAD_SPRITE_POS_Y = 0;

    public const float START_SPEED = 280f;
    public const float MAX_SPEED = 900f;

    public const float
        ACCELERATION = 5f; //pixels per second per second, 1pps^2 would have us increase from 280f to 281f in 1 second

    private const int DUCK_COLLISION_REDUCTION = 20;

    private Sprite _idleBackgroundSprite;
    private Sprite _idleSprite;
    private Sprite _idleBlinkSprite;
    private Sprite _deadSprite;

    private SoundEffect _jumpSound;

    private SpriteAnimation _blinkAnimation;
    private SpriteAnimation _runAnimation;
    private SpriteAnimation _duckAnimation;

    public event EventHandler JumpComplete;
    public event EventHandler Died;

    public Vector2 Position { get; set; }

    public TrexState State { get; private set; }

    public bool IsAlive { get; private set; }

    public float Speed { get; private set; }

    public int DrawOrder { get; set; }

    public Rectangle CollisionBox
    {
        get
        {
            var rect = new Rectangle((int)Math.Round(Position.X), (int)Math.Round(Position.Y),
                TREX_DEFAULT_SPRITE_WIDTH,
                TREX_DEFAULT_SPRITE_HEIGHT);
            COLLISION_BOX_INSET = 10;
            rect.Inflate(-COLLISION_BOX_INSET, -COLLISION_BOX_INSET);
            
            //I'd prefer to pass in the proper height of the sprite into the new Rectangle call tbh
            if (State == TrexState.Ducking)
            {
                rect.Y += DUCK_COLLISION_REDUCTION; //move it down by x pixels
                rect.Height -= DUCK_COLLISION_REDUCTION;
            }

            return rect;
        }
    }

    private Random _random;

    private float _verticalVelocity;
    private float _startPosY;
    private float _dropVelocity;
    private int COLLISION_BOX_INSET;

    public Trex(Texture2D spriteSheet, Vector2 position, SoundEffect jumpSound)
    {
        IsAlive = true;
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

        _runAnimation = new SpriteAnimation();
        _runAnimation.AddFrame(new Sprite(spriteSheet, TREX_RUNNING_SPRITE_ONE_POS_X, TREX_RUNNING_SPRITE_ONE_POS_Y,
            TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT), 0);
        _runAnimation.AddFrame(new Sprite(spriteSheet, TREX_RUNNING_SPRITE_ONE_POS_X + TREX_DEFAULT_SPRITE_WIDTH,
            TREX_RUNNING_SPRITE_ONE_POS_Y,
            TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT), RUN_ANIMATION_FRAME_LENGTH);
        //fake ending sprite, has to be *2 because it's timestamp in overall animation not duration
        _runAnimation.AddFrame(_runAnimation[0].Sprite, RUN_ANIMATION_FRAME_LENGTH * 2);
        _runAnimation.Play();

        //this is using the default height, but shouldn't it be shorter? when ducking, we ARE shorter...
        _duckAnimation = new SpriteAnimation();
        _duckAnimation.AddFrame(new Sprite(spriteSheet, TREX_DUCKING_SPRITE_ONE_POS_X, TREX_DUCKING_SPRITE_ONE_POS_Y,
            TREX_DUCKING_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT), 0);
        _duckAnimation.AddFrame(new Sprite(spriteSheet, TREX_DUCKING_SPRITE_ONE_POS_X + TREX_DUCKING_SPRITE_WIDTH,
            TREX_DUCKING_SPRITE_ONE_POS_Y,
            TREX_DUCKING_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT), RUN_ANIMATION_FRAME_LENGTH);
        //fake ending sprite, has to be *2 because it's timestamp in overall animation not duration
        _duckAnimation.AddFrame(_duckAnimation[0].Sprite, RUN_ANIMATION_FRAME_LENGTH * 2);
        _duckAnimation.Play();

        _deadSprite = new Sprite(spriteSheet, TREX_DEAD_SPRITE_POS_X, TREX_DEAD_SPRITE_POS_Y, TREX_DEFAULT_SPRITE_WIDTH,
            TREX_DEFAULT_SPRITE_HEIGHT);
    }

    public void Initialize()
    {
        Speed = START_SPEED;
        State = TrexState
            .Running; //should be set to running on the end of the jump, but for good measure we make explicit here
        IsAlive = true;
        Position = new Vector2(Position.X, _startPosY);
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
                Position.Y + _verticalVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds +
                _dropVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds);
            _verticalVelocity += GRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //make sure we set ourselves as falling if our velocity has changed to positive (sending us downward)
            if (_verticalVelocity >= 0)
                State = TrexState.Falling;

            //so we don't fall through the floor because of gravity, we essentially cap how low our y position can go
            if (Position.Y >= _startPosY)
            {
                Position = new Vector2(Position.X, _startPosY);
                _verticalVelocity = 0;
                State = TrexState.Running;

                //fire the event as we've completed our Jump
                OnJumpComplete();
            }
        }
        else if (State == TrexState.Running)
        {
            _runAnimation.Update(gameTime);
        }
        else if (State == TrexState.Ducking)
        {
            _duckAnimation.Update(gameTime);
        }

        if (State != TrexState.Idle)
        {
            Speed += ACCELERATION * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        if (Speed > MAX_SPEED)
            Speed = MAX_SPEED;

        _dropVelocity = 0;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!IsAlive)
        {
            _deadSprite.Draw(spriteBatch, Position);
        }
        else
            switch (State)
            {
                case TrexState.Idle:
                    //if we're idle, draw the idle sprite in the background so that we can see the ground and blink
                    _idleBackgroundSprite.Draw(spriteBatch, Position);
                    _blinkAnimation.Draw(spriteBatch, Position);
                    break;
                case TrexState.Jumping or TrexState.Falling:
                    _idleSprite.Draw(spriteBatch, Position);
                    break;
                case TrexState.Running:
                    _runAnimation.Draw(spriteBatch, Position);
                    break;
                case TrexState.Ducking:
                    _duckAnimation.Draw(spriteBatch, Position);
                    break;
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

        //rather than immediately sending us to 0 velocity which will throw us to the ground really quickly,
        //just set a higher velocity that will have us head to the ground faster
        //if we're closer to 0 already (falling faster) then send us to 0
        _verticalVelocity = _verticalVelocity < CANCEL_JUMP_VELOCITY ? CANCEL_JUMP_VELOCITY : 0;

        return true;
    }

    public bool Duck()
    {
        if (State is TrexState.Jumping or TrexState.Falling)
            return false;

        State = TrexState.Ducking;

        return true;
    }

    public bool GetUp()
    {
        if (State != TrexState.Ducking)
            return false;

        State = TrexState.Running;

        return true;
    }

    public bool Drop()
    {
        if (State is not (TrexState.Jumping or TrexState.Falling))
        {
            return false;
        }

        State = TrexState.Falling;
        _dropVelocity = DROP_VELOCITY;
        return true;
    }

    protected virtual void OnJumpComplete()
    {
        //using an event to signal when we completed a jump and can have the ground start moving, for the game to really 'start'
        //clients can sub to this event by adding a method with a matching signature -> which we'll do
        JumpComplete?.Invoke(this, EventArgs.Empty);
    }

    public bool Die()
    {
        //can you die if you're already dead? too philosophical of a question...
        if (!IsAlive)
            return false;

        //reset state to idle
        State = TrexState.Idle;
        Speed = 0;

        IsAlive = false;
        OnDied();
        return true;
    }

    protected virtual void OnDied()
    {
        Died?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateTexture(Texture2D newTexture)
    {
        _idleSprite.Texture = newTexture;
        _deadSprite.Texture = newTexture;
        _idleBackgroundSprite.Texture = newTexture;
        _idleBlinkSprite.Texture = newTexture;

        for (var i = 0; i < _blinkAnimation.FrameCount; i++)
        {
            _blinkAnimation.GetFrame(i).Sprite.Texture = newTexture;
        }
        for (var i = 0; i < _duckAnimation.FrameCount; i++)
        {
            _duckAnimation.GetFrame(i).Sprite.Texture = newTexture;
        }
        for (var i = 0; i < _runAnimation.FrameCount; i++)
        {
            _runAnimation.GetFrame(i).Sprite.Texture = newTexture;
        }
    }
}
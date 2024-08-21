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

    private Sprite _idleBackgroundSprite;
    public Sprite Sprite { get; private set; }

    public Vector2 Position { get; set; }

    public TrexState State { get; private set; }

    public bool IsAlive { get; private set; }

    public float Speed { get; private set; }

    public int DrawOrder { get; set; }

    public Trex(Texture2D spriteSheet, Vector2 position)
    {
        Sprite = new Sprite(spriteSheet, TREX_DEFAULT_SPRITE_POS_X, TREX_DEFAULT_SPRITE_POS_Y,
            TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT);
        Position = position;
        //little idle sprite that has ground drawn with it
        _idleBackgroundSprite = new Sprite(spriteSheet, TREX_IDLE_BACKGROUND_SPRITE_POS_X,
            TREX_IDLE_BACKGROUND_SPRITE_POS_Y, TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT);
        State = TrexState.Idle;
    }

    public void Update(GameTime gameTime)
    {
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (State == TrexState.Idle)
        {
            //if we're idle, draw the idle sprite in the background so that we can see the ground and blink
            _idleBackgroundSprite.Draw(spriteBatch, Position);
        }

        Sprite.Draw(spriteBatch, Position);
    }
}
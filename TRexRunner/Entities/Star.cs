using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TRexRunner.Graphics;

namespace TRexRunner.Entities;

public class Star : SkyObject
{
    private const int INITIAL_FRAME_TEXTURE_COORDS_X = 644;
    private const int INITIAL_FRAME_TEXTURE_COORDS_Y = 2;

    private const int SPRITE_WIDTH = 9;
    private const int SPRITE_HEIGHT = 9;
    private const float ANIMATION_FRAME_LENGTH = .4f;

    private readonly IDayNightCycle _dayNightCycle;

    private SpriteAnimation _spriteAnimation;
    public override float Speed => _trex.Speed * 0.2f;

    public Star(IDayNightCycle dayNightCycle, Texture2D spriteSheet, Trex trex, Vector2 position) : base(trex, position)
    {
        _dayNightCycle = dayNightCycle;
        _spriteAnimation = SpriteAnimation.CreateSimpleAnimation(spriteSheet,
            new Point(INITIAL_FRAME_TEXTURE_COORDS_X, INITIAL_FRAME_TEXTURE_COORDS_Y), SPRITE_WIDTH, SPRITE_HEIGHT,
            new Point(0, SPRITE_HEIGHT), 3, ANIMATION_FRAME_LENGTH);
        _spriteAnimation.ShouldLoop = true;
        _spriteAnimation.Play();
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_dayNightCycle.IsNight)
            _spriteAnimation.Draw(spriteBatch, Position);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        //stars DONT sparkle if im dead
        if (_trex.IsAlive)
            _spriteAnimation.Update(gameTime);
    }
}
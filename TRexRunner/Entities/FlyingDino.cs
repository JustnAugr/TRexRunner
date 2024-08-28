using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TRexRunner.Graphics;

namespace TRexRunner.Entities;

public class FlyingDino : Obstacle
{
    private const int TEXTURE_COORDS_X = 134;
    private const int TEXTURE_COORDS_Y = 0;
    private const int SPRITE_WIDTH = 46;
    private const int SPRITE_HEIGHT = 42;

    private const float ANIMATION_FRAME_LENGTH = 2 / 10f;
    private const float SPEED_PPS = 80f;

    //we're making the terror-dactal's collision box quite small so we can duck under without dying
    private const int VERTICAL_COLLISION_INSET = 10;
    private const int HORIZONTAL_COLLISION_INSET = 4;

    private SpriteAnimation _animation;
    private Trex _trex;

    public override Rectangle CollisionBox
    {
        get
        {
            Rectangle rect = new((int)Math.Round(Position.X), (int)Math.Round(Position.Y),
                SPRITE_WIDTH, SPRITE_HEIGHT);
            rect.Inflate(-HORIZONTAL_COLLISION_INSET, -VERTICAL_COLLISION_INSET); //negatives to make smaller
            return rect;
        }
    }


    public FlyingDino(Trex trex, Vector2 position, Texture2D spriteSheet) : base(trex, position)
    {
        _trex = trex;
        var spriteA = new Sprite(spriteSheet, TEXTURE_COORDS_X, TEXTURE_COORDS_Y, SPRITE_WIDTH, SPRITE_HEIGHT);
        var spriteB = new Sprite(spriteSheet, TEXTURE_COORDS_X + SPRITE_WIDTH, TEXTURE_COORDS_Y, SPRITE_WIDTH,
            SPRITE_HEIGHT);
        _animation = new SpriteAnimation();
        _animation.AddFrame(spriteA, 0);
        _animation.AddFrame(spriteB, ANIMATION_FRAME_LENGTH);
        _animation.AddFrame(spriteA, ANIMATION_FRAME_LENGTH * 2);
        _animation.ShouldLoop = true;
        _animation.Play();
    }

    public override void Update(GameTime gameTime)
    {
        //move the sprite(Animation), check for collisions
        base.Update(gameTime);

        //also need to make sure we're playing the animation until game stops
        if (_trex.IsAlive)
        {
            _animation.Update(gameTime);
            
            //make the trex a little faster since it's moving as well, unlike the cacti
            //we already moved it in line with the trex in the base class, so now do more!
            Position = new Vector2(Position.X - SPEED_PPS * (float)gameTime.ElapsedGameTime.TotalSeconds, Position.Y);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        _animation.Draw(spriteBatch, Position);
    }
}
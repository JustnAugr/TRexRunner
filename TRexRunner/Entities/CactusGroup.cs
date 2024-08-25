using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TRexRunner.Graphics;

namespace TRexRunner.Entities;

public class CactusGroup : Obstacle
{
    public enum GroupSize
    {
        Small,
        Medium,
        Large
    }

    private const int SMALL_CACTUS_SPRITE_HEIGHT = 36;
    private const int SMALL_CACTUS_SPRITE_WIDTH = 17;
    private const int SMALL_CACTUS_TEXTURE_POS_X = 228;
    private const int SMALL_CACTUS_TEXTURE_POS_Y = 0;

    private const int LARGE_CACTUS_SPRITE_HEIGHT = 51;
    private const int LARGE_CACTUS_SPRITE_WIDTH = 25;
    private const int LARGE_CACTUS_TEXTURE_POS_X = 332;
    private const int LARGE_CACTUS_TEXTURE_POS_Y = 0;
    private const int COLLISION_BOX_INSET = 3;

    public bool IsLarge { get; }
    
    public override Rectangle CollisionBox
    {
        get
        {
            //inflate by negative amount to evenly shrink on all 4 sides
            var rect = new Rectangle((int)Math.Round(Position.X), (int)Math.Round(Position.Y),
                Sprite.Width, Sprite.Height);
            rect.Inflate(-COLLISION_BOX_INSET, -COLLISION_BOX_INSET);
            return rect;
        }
    }

    public GroupSize Size { get; }

    public Sprite Sprite { get; }

    public CactusGroup(Texture2D spriteSheet, bool isLarge, GroupSize size, Trex trex, Vector2 position) : base(trex,
        position)
    {
        IsLarge = isLarge;
        Size = size;
        Sprite = GenerateSprite(spriteSheet);
    }

    private Sprite GenerateSprite(Texture2D spriteSheet)
    {
        Sprite sprite;

        //in the sheet it's 6 cacti: the small group of 1 cacti, med group of 2 cacti, large group of 3 cacti
        //so we offset x to where each group starts
        //in the sheet it's 6 cacti: the small group of 1 cacti, med group of 2 cacti, large group of 3 cacti
        //so we offset x to where each group starts
        var offsetX = Size switch
        {
            GroupSize.Small => 0,
            GroupSize.Medium => 1,
            GroupSize.Large => 3,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!IsLarge) //create a group of small cacti
        {
            //similarly, the widths need to change based on # of cacti being accomodated
            var width = Size switch
            {
                GroupSize.Small => SMALL_CACTUS_SPRITE_WIDTH,
                GroupSize.Medium => SMALL_CACTUS_SPRITE_WIDTH * 2,
                GroupSize.Large => SMALL_CACTUS_SPRITE_WIDTH * 3,
                _ => throw new ArgumentOutOfRangeException()
            };

            sprite = new Sprite(spriteSheet, SMALL_CACTUS_TEXTURE_POS_X + offsetX * SMALL_CACTUS_SPRITE_WIDTH,
                SMALL_CACTUS_TEXTURE_POS_Y,
                width, SMALL_CACTUS_SPRITE_HEIGHT);
        }
        else //create a group of large cacti
        {
            var width = Size switch
            {
                GroupSize.Small => LARGE_CACTUS_SPRITE_WIDTH,
                GroupSize.Medium => LARGE_CACTUS_SPRITE_WIDTH * 2,
                GroupSize.Large => LARGE_CACTUS_SPRITE_WIDTH * 3,
                _ => throw new ArgumentOutOfRangeException()
            };

            sprite = new Sprite(spriteSheet, LARGE_CACTUS_TEXTURE_POS_X + offsetX * LARGE_CACTUS_SPRITE_WIDTH,
                LARGE_CACTUS_TEXTURE_POS_Y,
                width, LARGE_CACTUS_SPRITE_HEIGHT);
        }

        return sprite;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        Sprite.Draw(spriteBatch, Position);
    }
}
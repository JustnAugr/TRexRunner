using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TRexRunner.Graphics;

namespace TRexRunner.Entities;

public class Cloud : SkyObject
{
    private const int TEXTURE_COORDS_X = 87;
    private const int TEXTURE_COORDS_Y = 0;
    private const int SPRITE_WIDTH = 46;
    private const int SPRITE_HEIGHT = 17;

    private Sprite _sprite;
    public override float Speed => _trex.Speed * .5f;

    public Cloud(Texture2D spriteSheet, Trex trex, Vector2 position) : base(trex, position)
    {
        _sprite = new Sprite(spriteSheet, TEXTURE_COORDS_X, TEXTURE_COORDS_Y, SPRITE_WIDTH, SPRITE_HEIGHT);
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        _sprite.Draw(spriteBatch, Position);
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TRexRunner.Graphics;

namespace TRexRunner.Entities;

public class GameOverScreen : IGameEntity
{
    private const int GAME_OVER_TEXTURE_POS_X = 655;
    private const int GAME_OVER_TEXTURE_POS_Y = 14;
    private const int GAME_OVER_SPRITE_HEIGHT = 14;
    private const int GAME_OVER_SPRITE_WIDTH = 192;
    private const int BUTTON_TEXTURE_POS_X = 1;
    private const int BUTTON_TEXTURE_POS_Y = 1;
    private const int BUTTON_SPRITE_HEIGHT = 34;
    private const int BUTTON_SPRITE_WIDTH = 38;

    private Texture2D _spriteSheet;
    private Sprite _textSprite;
    private Sprite _buttonSprite;

    public int DrawOrder { get; set; } = 100;
    public bool IsEnabled { get; set; }
    public Vector2 Position { get; set; } //will be drawn middle of screen

    public Vector2 ButtonPosition => Position + //draw below, and in the middle of the "GAME OVER" text
                                     new Vector2(GAME_OVER_SPRITE_WIDTH / 2 - BUTTON_SPRITE_WIDTH / 2,
                                         GAME_OVER_SPRITE_HEIGHT + 20);

    public GameOverScreen(Texture2D spriteSheet)
    {
        _spriteSheet = spriteSheet;
        _textSprite = new Sprite(_spriteSheet, GAME_OVER_TEXTURE_POS_X, GAME_OVER_TEXTURE_POS_Y, GAME_OVER_SPRITE_WIDTH,
            GAME_OVER_SPRITE_HEIGHT);
        _buttonSprite = new Sprite(_spriteSheet, BUTTON_TEXTURE_POS_X, BUTTON_TEXTURE_POS_Y, BUTTON_SPRITE_WIDTH,
            BUTTON_SPRITE_HEIGHT);
    }

    public void Update(GameTime gameTime)
    {
        if (!IsEnabled)
            return;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!IsEnabled)
            return;

        _textSprite.Draw(spriteBatch, Position);
        _buttonSprite.Draw(spriteBatch, ButtonPosition);
    }
}
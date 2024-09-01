using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TRexRunner.Graphics;

namespace TRexRunner.Entities;

public class GameOverScreen : IGameEntity
{
    private const int GAME_OVER_TEXTURE_POS_X = 655;
    private const int GAME_OVER_TEXTURE_POS_Y = 14;
    private const int GAME_OVER_SPRITE_HEIGHT = 14;
    public const int GAME_OVER_SPRITE_WIDTH = 192;
    private const int BUTTON_TEXTURE_POS_X = 1;
    private const int BUTTON_TEXTURE_POS_Y = 1;
    private const int BUTTON_SPRITE_HEIGHT = 34;
    private const int BUTTON_SPRITE_WIDTH = 38;

    private Texture2D _spriteSheet;
    private Sprite _textSprite;
    private Sprite _buttonSprite;
    private Vector2 _position;
    private TRexRunnerGame _game;

    public int DrawOrder { get; set; } = 100;
    public bool IsEnabled { get; set; }

    public Vector2 ButtonPosition =>
        _position + //_position here is the top left of the entire game over text + button so we draw button
        //below it (height + 20), and then get to the middle of the game over text (gameover/2) and then go a bit further left to draw the button (- button/2)
        new Vector2(GAME_OVER_SPRITE_WIDTH / 2 - BUTTON_SPRITE_WIDTH / 2,
            GAME_OVER_SPRITE_HEIGHT + 20);

    public Rectangle ButtonBounds
    {
        get
        {
            //need to scale the positions by the zoom factor
            var buttonPositionPoint = (ButtonPosition * _game.ZoomFactor).ToPoint();
            var buttonSpriteWidth = (int)(BUTTON_SPRITE_WIDTH * _game.ZoomFactor);
            var buttonSpriteHeight = (int)(BUTTON_SPRITE_HEIGHT * _game.ZoomFactor);

            //this constructor takes two points, top left and bottom right
            return new(buttonPositionPoint, new Point(buttonSpriteWidth, buttonSpriteHeight));
        }
    }

    public GameOverScreen(Texture2D spriteSheet, Vector2 position, TRexRunnerGame game)
    {
        _spriteSheet = spriteSheet;
        _textSprite = new Sprite(_spriteSheet, GAME_OVER_TEXTURE_POS_X, GAME_OVER_TEXTURE_POS_Y, GAME_OVER_SPRITE_WIDTH,
            GAME_OVER_SPRITE_HEIGHT);
        _buttonSprite = new Sprite(_spriteSheet, BUTTON_TEXTURE_POS_X, BUTTON_TEXTURE_POS_Y, BUTTON_SPRITE_WIDTH,
            BUTTON_SPRITE_HEIGHT);
        _position = position;
        _game = game;
    }

    public void Update(GameTime gameTime)
    {
        if (!IsEnabled)
            return;

        var mouseState = Mouse.GetState();
        var kbState = Keyboard.GetState();

        if ((mouseState.LeftButton == ButtonState.Pressed && ButtonBounds.Contains(mouseState.Position)) ||
            kbState.IsKeyDown(Keys.Enter))
        {
            //I'd prefer to do this via an event, I think it's cleaner than passing a reference to the Game
            _game.Replay();
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!IsEnabled)
            return;

        _textSprite.Draw(spriteBatch, _position);
        _buttonSprite.Draw(spriteBatch, ButtonPosition);
    }
}
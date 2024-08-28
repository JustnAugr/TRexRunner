using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace TRexRunner.Entities;

public class ScoreBoard : IGameEntity
{
    private const int TEXTURE_COORDS_NUMBER_X = 655;
    private const int TEXTURE_COORDS_NUMBER_Y = 0;

    private const int TEXTURE_COORDS_NUMBER_WIDTH = 10;
    private const int TEXTURE_COORDS_NUMBER_HEIGHT = 13;

    private const byte NUMBER_DIGITS_TO_DRAW = 5;
    private const int SCORE_MARGIN = 70;

    private const int TEXTURE_COORDS_HI_X = 755;
    private const int TEXTURE_COORDS_HI_Y = 0;
    private const int TEXTURE_COORDS_HI_WIDTH = 20;
    private const int TEXTURE_COORDS_HI_HEIGHT = 13;
    private const int HI_TEXT_MARGIN = 28;
    private const float SCORE_INCREMENT_MULTIPLIER = .05f;

    private const float FLASH_ANIMATION_FRAME_LENGTH = 1/3f;
    private const int FLASH_ANIMATION_FLASH_COUNT = 4;

    private Texture2D _texture;
    private bool _isPlayingFlashAnimation;
    private float _flashAnimationTime;

    public double Score { get; set; }
    public int DisplayScore => (int)Math.Floor(Score);
    public int HiScore { get; set; }

    public bool HasHiScore => HiScore > 0;
    private Trex _trex;
    private SoundEffect _scoreSfx;

    public int DrawOrder { get; set; } = 100; //high value for a UI element

    public Vector2 Position { get; set; }

    public ScoreBoard(Texture2D texture2D, Vector2 position, Trex trex, SoundEffect scoreSfx)
    {
        _texture = texture2D;
        Position = position;
        _trex = trex;
        _scoreSfx = scoreSfx;
    }

    public void Update(GameTime gameTime)
    {
        int oldScore = DisplayScore;
        Score += _trex.Speed * SCORE_INCREMENT_MULTIPLIER *
                 gameTime.ElapsedGameTime.TotalSeconds; // score increases by 1/100 the speed per second

        //we flash 4 times every 100 points
        if (!_isPlayingFlashAnimation && (DisplayScore / 100) != (oldScore / 100)) // 199/100 = 1 but 200/100 = 2 so we'd play our animation
        {
            _scoreSfx.Play(.8f, 0, 0); //lowering the volume
            _isPlayingFlashAnimation = true;
            _flashAnimationTime = 0;
        }

        if (_isPlayingFlashAnimation)
        {
            _flashAnimationTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            //so (Flash_Animation_Frame_Length * Flash_Count * 2) represents the total time of playing each 'frame' of the animation
            //that means: flash off, flash on, flash off, flash on
            //because we want it to display the text 4 times, we have to do * 2 or else the text would really just flash twice
            if (_flashAnimationTime >= FLASH_ANIMATION_FRAME_LENGTH * FLASH_ANIMATION_FLASH_COUNT * 2)
            {
                _isPlayingFlashAnimation = false;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (HasHiScore)
        {
            spriteBatch.Draw(_texture, new Vector2(Position.X - HI_TEXT_MARGIN, Position.Y),
                new Rectangle(TEXTURE_COORDS_HI_X, TEXTURE_COORDS_HI_Y, TEXTURE_COORDS_HI_WIDTH,
                    TEXTURE_COORDS_HI_HEIGHT), Color.White);
            DrawScore(spriteBatch, HiScore, Position.X);
        }

        //draw it if we AREN'T playing the animation
        //OR if we're playing and then only display it on every other frame
            //the current total time of the animation divided by the length of the frame will give us the frame we're on
            //then only draw on every other frame
        if (!_isPlayingFlashAnimation || (int)(_flashAnimationTime / FLASH_ANIMATION_FRAME_LENGTH) % 2 != 0)
            DrawScore(spriteBatch, DisplayScore, Position.X + SCORE_MARGIN);
    }

    private void DrawScore(SpriteBatch spriteBatch, int score, float startPosX)
    {
        //DisplayScore: 498 -> 4 9 8
        //we need to split into individual digits and render them to the screen
        int[] scoreDigits = SplitDigits(score);

        float posX = startPosX;

        foreach (var digit in scoreDigits)
        {
            var textureCoords = GetDigitTextureBounds(digit);

            //position on the screen where it's going to be rendered
            var screenPos = new Vector2(posX, Position.Y);
            spriteBatch.Draw(_texture, screenPos, textureCoords, Color.White);

            //we want to draw the numbers one by one next to each other, so we have to move over on the screen
            posX += TEXTURE_COORDS_NUMBER_WIDTH;
        }
    }

    private int[] SplitDigits(int input)
    {
        var inputStr = input.ToString().PadLeft(NUMBER_DIGITS_TO_DRAW, '0');
        var result = new int[inputStr.Length];

        for (var i = 0; i < NUMBER_DIGITS_TO_DRAW; i++)
        {
            result[i] = (int)char.GetNumericValue(inputStr[i]);
        }

        return result;
    }

    private Rectangle GetDigitTextureBounds(int digit)
    {
        if (digit is < 0 or > 9)
            throw new ArgumentOutOfRangeException(nameof(digit), "Must be int 0-9");

        var posX = TEXTURE_COORDS_NUMBER_X + digit * TEXTURE_COORDS_NUMBER_WIDTH;

        return new Rectangle(posX, TEXTURE_COORDS_NUMBER_Y, TEXTURE_COORDS_NUMBER_WIDTH, TEXTURE_COORDS_NUMBER_HEIGHT);
    }
}
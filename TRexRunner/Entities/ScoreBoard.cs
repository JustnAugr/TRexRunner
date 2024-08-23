using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TRexRunner.Entities;

public class ScoreBoard : IGameEntity
{
    private const int TEXTURE_COORDS_NUMBER_X = 655;
    private const int TEXTURE_COORDS_NUMBER_Y = 0;

    private const int TEXTURE_COORDS_NUMBER_WIDTH = 10;
    private const int TEXTURE_COORDS_NUMBER_HEIGHT = 13;

    private const byte NUMBER_DIGITS_TO_DRAW = 5;

    private Texture2D _texture;

    public double Score { get; set; }
    public int DisplayScore => (int)Math.Floor(Score);
    public int HiScore { get; set; }

    public int DrawOrder { get; set; } = 100; //high value for a UI element

    public Vector2 Position { get; set; }

    public ScoreBoard(Texture2D texture2D, Vector2 position)
    {
        _texture = texture2D;
        Position = position;
    }

    public void Update(GameTime gameTime)
    {
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        //DisplayScore: 498 -> 4 9 8
        //we need to split into individual digits and render them to the screen
        int[] scoreDigits = SplitDigits(DisplayScore);

        float posX = Position.X;

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
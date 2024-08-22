using System;

namespace TRexRunner.Graphics;

public class SpriteAnimationFrame
{
    private Sprite _sprite;

    public Sprite Sprite
    {
        get => _sprite;
        set => _sprite = value ?? throw new ArgumentNullException(nameof(value), "The sprite cannot be null!");
    }
    
    public float TimeStamp { get; }

    public SpriteAnimationFrame(Sprite sprite, float timeStamp)
    {
        Sprite = sprite;
        TimeStamp = timeStamp;
    }
}
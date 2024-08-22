using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TRexRunner.Graphics;

public class SpriteAnimation
{
    private List<SpriteAnimationFrame> _frames = new();

    //this is an indexer, allows us to call an instance of SpriteAnimation as if it was an
    //array itself ie spriteAnimation[0] to access the 0th frame via the GetFrame(index) method
    //just a bit easier than doing spriteAnimation.GetFrame(0)
    public SpriteAnimationFrame this[int index] => GetFrame(index);

    public bool IsPlaying { get; private set; }

    public float PlaybackProgress { get; private set; }

    public bool ShouldLoop { get; set; } = true;

    public SpriteAnimationFrame CurrentFrame
    {
        //give us the last frame whose timestamp is <= our playback progress
        //ie if we're playing a total 5 second animation and playback is currently at the 3.5s mark, we need to find the frame that should be playing now
        //if frames were stamped [1,2,3,4] with 1s durations, then that would be the third one
        get { return _frames.Where(f => f.TimeStamp <= PlaybackProgress).MaxBy(f => f.TimeStamp); }
    }

    public float Duration
    {
        get
        {
            if (_frames.Count == 0)
                return 0;

            //what we're doing here is assuming that for a 4 frame animation, we'll actually be given 5 frames - the last
            //being a "dummy" frame with its Timestamp (the timestamp on each frame being when it needs to start playing),
            //actually being a market that the REAL last frame has ended
            //so - the below method gets the highest timestamp (the start of our dummy frame) to tell us how long our animation is
            return _frames.Max(f => f.TimeStamp);
        }
    }

    public void AddFrame(Sprite sprite, float timeStamp)
    {
        SpriteAnimationFrame frame = new SpriteAnimationFrame(sprite, timeStamp);
        _frames.Add(frame);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        SpriteAnimationFrame frame = CurrentFrame;
        frame?.Sprite.Draw(spriteBatch, position);
    }

    public void Update(GameTime gameTime)
    {
        if (IsPlaying)
        {
            //if we're currently playing, increment the PlaybackProgress by how many seconds since last update
            PlaybackProgress += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (PlaybackProgress > Duration)
            {
                //loop the animation by subtracting our duration
                //ie we have a 2 animation, and after this Update call's +=, we're one frame past at 2.1 seconds
                //we -= Duration to bring us back down to .1 seconds into our animation

                //why not just set to 0? not sure...
                if (ShouldLoop)
                    PlaybackProgress -= Duration;
                else
                    Stop();
            }
        }
    }

    public void Play()
    {
        IsPlaying = true;
    }

    public void Stop()
    {
        IsPlaying = false;
        PlaybackProgress = 0;
    }

    public SpriteAnimationFrame GetFrame(int index)
    {
        if (index < 0 || index >= _frames.Count)
            throw new ArgumentOutOfRangeException(nameof(index),
                "A frame with index " + index + " does not exist in this animation.");

        return _frames[index];
    }
}
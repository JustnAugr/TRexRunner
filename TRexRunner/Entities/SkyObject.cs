﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TRexRunner.Entities;

public abstract class SkyObject : IGameEntity
{
    protected Trex _trex;

    public int DrawOrder { get; set; }

    public abstract float Speed { get; }

    public Vector2 Position { get; set; }

    protected SkyObject(Trex trex, Vector2 position)
    {
        _trex = trex;
        Position = position;
    }

    public virtual void Update(GameTime gameTime)
    {
        if (_trex.IsAlive)
            Position = new Vector2(Position.X - Speed * (float)gameTime.ElapsedGameTime.TotalSeconds, Position.Y);
    }

    public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TRexRunner.Graphics;

namespace TRexRunner.Entities;

public abstract class Obstacle : IGameEntity
{
    private Trex _trex;

    public abstract Rectangle CollisionBox { get; }
    public int DrawOrder { get; set; }
    public Vector2 Position { get; private set; }

    //protected instead of public since this is an abstract class
    protected Obstacle(Trex trex, Vector2 position)
    {
        _trex = trex;
        Position = position;
    }

    public void Update(GameTime gameTime)
    {
        //should move in line with Trex speed per second
        var posX = Position.X - _trex.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

        Position = new Vector2(posX, Position.Y);
    }

    public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);
}
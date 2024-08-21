using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TRexRunner.Entities;

public interface IGameEntity
{
    int DrawOrder { get; set; }

    void Update(GameTime gameTime);

    void Draw(SpriteBatch spriteBatch, GameTime gameTime);
}
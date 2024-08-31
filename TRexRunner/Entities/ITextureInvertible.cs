using Microsoft.Xna.Framework.Graphics;

namespace TRexRunner.Entities;

public interface ITextureInvertible : IGameEntity
{
    void UpdateTexture(Texture2D newTexture);
}
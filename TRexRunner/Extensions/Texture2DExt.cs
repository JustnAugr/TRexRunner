using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TRexRunner.Extensions;

public static class Texture2DExt
{
    //extension method and class needs to be static, needs to have first parameter as "this"
    //to allow us to call this method on all Texture2D objects as if this was a real method on the
    //instance, via someText.InvertColors()
    public static Texture2D InvertColors(this Texture2D texture)
    {
        if (texture is null)
            throw new ArgumentNullException(nameof(texture), "Texture can't be null to be inverted!");

        var result = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);

        //how do you invert an RGB color?
        //255 is white, its inverse at black would be 0
        //the inverse of 1 would be 254
        //so inverse of X = 255-X
        //on a 0f->1f scale, this is just saying the inverse of X is 1.0-X

        //get our texture pixel data, store in a 1D array (why not 2d? not sure)
        var pixelData = new Color[texture.Width * texture.Height];
        texture.GetData(pixelData);

        //fancy LINQ method similar to mapcar for our inversion instead of a foreach which would also work, + leave the alpha alone
        //alpha being opacity which we don't need to invert
        Color[] invertedPixelData = pixelData.Select(p => new Color(255 - p.R, 255 - p.G, 255 - p.B, p.A)).ToArray();
        result.SetData(invertedPixelData);

        return result;
    }
}
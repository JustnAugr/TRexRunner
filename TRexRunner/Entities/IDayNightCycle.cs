using Microsoft.Xna.Framework;

namespace TRexRunner.Entities;

public interface IDayNightCycle
{
    int NightCount { get; }
    bool IsNight { get; }
    Color ClearColor { get; }
}
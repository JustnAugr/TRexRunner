using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TRexRunner.Entities;

namespace TRexRunner.System;

public class InputController
{
    private Trex _trex;

    private KeyboardState _previousKeyboardState;

    public InputController(Trex trex)
    {
        _trex = trex;
    }

    public void ProcessControls(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        //only on initial press of the button
        if (!_previousKeyboardState.IsKeyDown(Keys.Up) && keyboardState.IsKeyDown(Keys.Up))
        {
            if (_trex.State != TrexState.Jumping)
                _trex.BeginJump();
        }
        //if we're not pressing the jump button currently, but trex is in a jump animation
        else if (!keyboardState.IsKeyDown(Keys.Up) && _trex.State == TrexState.Jumping)
        {
            _trex.CancelJump();
        }

        //keyboardState is a struct, so upon this assignment it's copied fieldwise
        //won't reference the same instance, it will reference a new struct that is an exact copy
        _previousKeyboardState = keyboardState;
    }
}
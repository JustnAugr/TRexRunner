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
        var isJumpKeyPressed = keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.Up);
        var wasJumpKeyPressed =
            _previousKeyboardState.IsKeyDown(Keys.Space) || _previousKeyboardState.IsKeyDown(Keys.Up);

        //only on initial press of the button
        if (!wasJumpKeyPressed && isJumpKeyPressed)
        {
            if (_trex.State != TrexState.Jumping)
                _trex.BeginJump();
        }
        //if we're not pressing the jump button currently, but trex is in a jump animation
        else if (!isJumpKeyPressed && _trex.State == TrexState.Jumping)
        {
            _trex.CancelJump();
        }
        else if (keyboardState.IsKeyDown(Keys.Down))
        {
            if (_trex.State is TrexState.Jumping or TrexState.Falling)
                _trex.Drop();
            else
                _trex.Duck();
        }
        else if (_trex.State == TrexState.Ducking && !keyboardState.IsKeyDown(Keys.Down))
        {
            _trex.GetUp();
        }

        //keyboardState is a struct, so upon this assignment it's copied fieldwise
        //won't reference the same instance, it will reference a new struct that is an exact copy
        _previousKeyboardState = keyboardState;
    }
}
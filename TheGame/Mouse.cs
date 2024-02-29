using Microsoft.Xna.Framework.Input;

namespace TheGame;

public class Mouse
{
    static MouseState currentMouseState;
    static MouseState previousMouseState;
    static bool movedSinceLastLeftClick = false;
    static bool previouslyMovedSinceLastLeftClick = false;

    public static MouseState GetState()
    {
        previousMouseState = currentMouseState;
        previouslyMovedSinceLastLeftClick =  movedSinceLastLeftClick;

        currentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
        if (previousMouseState.Position != currentMouseState.Position) movedSinceLastLeftClick = true;
        if (LeftClicked()) movedSinceLastLeftClick = false;

        return currentMouseState;
    }

    public static bool LeftDown()
    {
        return currentMouseState.LeftButton == ButtonState.Pressed;
    }

    public static bool LeftClicked()
    {
        return currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
    }
    public static bool LeftDeClicked()
    {
        return currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed;
    }
    public static bool FirstMovementSinceLastLeftClick()
    {
        return movedSinceLastLeftClick && !previouslyMovedSinceLastLeftClick;
    }
    public static bool RightDown()
    {
        return currentMouseState.RightButton == ButtonState.Pressed;
    }

    public static bool RightClicked()
    {
        return currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released;
    }
    public static bool RightDeClicked()
    {
        return currentMouseState.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed;
    }
    public static int Scroll()
    {
        return currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue;
    }
    public static int X()
    {
        return currentMouseState.X;
    }
    public static int Y()
    {
        return currentMouseState.Y;
    }
}
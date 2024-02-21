using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

public class Entity
{
    public Vector2 coordinate;
    public Entity(Vector2 coordinate)
    {
        this.coordinate = coordinate;
    }
}
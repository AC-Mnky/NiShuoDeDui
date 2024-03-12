using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TheGame;

public enum CastType {Independent, Dependent};

public class Cast : ICloneable
{
    public CastType type;
    public Vector2 coordinate = new();
    public Entity subject = null;
    public Vector2 direction = Vector2.Zero;
    public Cast(Vector2 coordinate)
    {
        type = CastType.Independent;
        this.coordinate = coordinate;
    }
    public Cast(Entity subject)
    {
        type = CastType.Dependent;
        this.subject = subject;
    }

    public object Clone()
    {
        if(type == CastType.Independent)
        {
            Cast clone = new(coordinate);
            clone.direction = direction;
            return clone;
        }
        else
        {
            Cast clone = new(subject);
            clone.direction = direction;
            return clone;
        }
    }
}
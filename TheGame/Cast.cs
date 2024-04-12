using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TheGame;

public enum CastType {Independent, Dependent};

public class Cast
{
    public CastType type;
    public Vector2 coordinate = new();
    public Entity subject = null;
    public Vector2 direction = Vector2.Zero;
    public float manaMul = 1;
    public Segment segment = null;
    public float progress = 0;
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
    public Cast(Cast c, bool forceIndependent = false)
    {
        type = forceIndependent ? CastType.Independent : c.type;
        coordinate = forceIndependent ? c.CurrentCoordinate() : c.coordinate;
        subject = forceIndependent ? null : c.subject;
        direction = c.direction;
        manaMul = c.manaMul;
    }
    public Vector2 CurrentCoordinate()
    {
        if (type == CastType.Dependent) return subject.coordinate;
        else return coordinate;
    }

}
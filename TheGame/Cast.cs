using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TheGame;

public enum CastType {Independent, Dependent};

public struct Cast
{
    CastType type;
    long tickBirth;
    CastType dependence;
    Vector2 coordinate;
    Vector2 currentCoordinate()
    {
        if(dependence == CastType.Independent) return coordinate;
        else return game.entities[subjectId].coordinate;
    }
    long subjectId;
    Vector2 direction = Vector2.Zero;
}
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

abstract public class Entity : Thing
{
    public enum Name {Enemy1};
    public Name name;
    public Entity(Game1 game, Name name) : base(game)
    {
        this.name = name;
    }
    protected static Dictionary<Name, Vector2> RenderCoordinateOffset = new() {
        {Name.Enemy1, new Vector2(-16f,-16f)}
    };
    public Vector2 coordinate;
    public void TickUpdateCoordinate() {coordinate += velocity;}
    public Vector2 RenderCoordinate() {return coordinate + RenderCoordinateOffset[name];}
    public Vector2 velocity;
    abstract public void TickUpdateVelocity();
    public double health;
    abstract public void TickAction();
    
}
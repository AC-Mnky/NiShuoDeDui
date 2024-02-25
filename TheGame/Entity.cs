using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

abstract public class Entity : Thing
{
    public Entity(Game1 game, long id, Name name, Vector2 coordinate, Vector2 velocity) : base(game,id,name)
    {
        this.coordinate = coordinate;
        this.velocity = velocity;
    }
    public static Dictionary<Name, Texture2D> Texture = new();
    protected static Dictionary<Name, Vector2> RenderCoordinateOffset = new() {
        {Name.Enemy1, new Vector2(-16f,-16f)},
        {Name.Projectile1, new Vector2(-8f,-8f)}
    };
    public Vector2 coordinate;
    public void TickUpdateCoordinate() {if(alive) coordinate += velocity;}
    public Vector2 RenderCoordinate() {return coordinate + RenderCoordinateOffset[name];}
    public Texture2D RenderTexture() {return Texture[name];}
    public Vector2 velocity;
    public double health;
}
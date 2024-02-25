using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TheGame;
public enum EntityName {Enemy1, Projectile1};

abstract public class Entity : Thing
{
    public EntityName name;
    public Entity(Game1 game, long id, EntityName name, Vector2 coordinate, Vector2 velocity) : base(game,id)
    {
        this.name = name;
        this.coordinate = coordinate;
        this.velocity = velocity;
    }
    public static Dictionary<EntityName, Texture2D> Texture = new();
    protected static Dictionary<EntityName, Vector2> RenderCoordinateOffset = new() {
        {EntityName.Enemy1, new Vector2(-16f,-16f)},
        {EntityName.Projectile1, new Vector2(-8f,-8f)}
    };
    public Vector2 coordinate;
    public void TickUpdateCoordinate() {if(alive) coordinate += velocity;}
    public Vector2 RenderCoordinate() {return coordinate + RenderCoordinateOffset[name];}
    public Texture2D RenderTexture() {return Texture[name];}
    public Vector2 velocity;
    public double health;
}
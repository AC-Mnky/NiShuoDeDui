using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

abstract public class Entity : Thing
{
    protected static Dictionary<Name, Vector2> RenderCoordinateOffset = new() {
        {Name.Enemy1, new Vector2(-16f,-16f)},
        {Name.Projectile1, new Vector2(-8f,-8f)},
        {Name.SquareD6, new()}
    };
    protected static Dictionary<Name, Vector2> Size = new() {
        {Name.Enemy1, new Vector2(32f,32f)},
        {Name.Projectile1, new Vector2(16f,16f)},
        {Name.SquareD6, new Vector2(6*64f,6*64f)}
    };
    protected static Dictionary<Name, double> DefaultHealth = new() {
        {Name.Enemy1, 10d},
        {Name.Projectile1, 1d},
        {Name.SquareD6, 0d}
    };
    public Entity(Game1 game, long id, Name name, Vector2 coordinate, Vector2 velocity) : base(game,id,name)
    {
        this.coordinate = coordinate;
        size = Size[name];
        this.velocity = velocity;
        health = DefaultHealth[name];
    }
    public static Dictionary<Name, Texture2D> Texture = new();
    public Vector2 coordinate;
    public Vector2 size;
    public RectangleF Hitbox()
    {
        return new RectangleF(coordinate.X-size.X/2,coordinate.Y-size.Y/2,size.X,size.Y);
    }
    public ArrayList Collisions() {return game.Collisions(this);}
    public void TickUpdateCoordinate() {if(alive) coordinate += velocity;}
    public Vector2 RenderCoordinate() {return coordinate + RenderCoordinateOffset[name];}
    public Texture2D RenderTexture() {return Texture[name];}
    public Vector2 velocity;
    public double health;
}
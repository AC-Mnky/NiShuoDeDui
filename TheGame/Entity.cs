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
        {Name.EnemyEasy, new Vector2(-16f,-16f)},
        {Name.EnemyFast, new Vector2(-8f,-8f)},
        {Name.EnemyVeryFast, new Vector2(-8f,-8f)},
        {Name.Projectile1, new Vector2(-8f,-8f)},
        {Name.SquareD6, new()}
    };
    protected static Dictionary<Name, Vector2> Size = new() {
        {Name.Enemy1, new Vector2(32f,32f)},
        {Name.EnemyEasy, new Vector2(32f,32f)},
        {Name.EnemyFast, new Vector2(16f,16f)},
        {Name.EnemyVeryFast, new Vector2(16f,16f)},
        {Name.Projectile1, new Vector2(16f,16f)},
        {Name.SquareD6, new Vector2(6*64f,6*64f)}
    };
    protected static Dictionary<Name, float> DefaultSpeed = new() {
        {Name.Enemy1, 1f},
        {Name.EnemyEasy, 5f},
        {Name.EnemyFast, 10f},
        {Name.EnemyVeryFast, 20f}
    };
    protected static Dictionary<Name, double> DefaultHealth = new() {
        {Name.Enemy1, 10d},
        {Name.EnemyEasy, 10d},
        {Name.EnemyFast, 10d},
        {Name.EnemyVeryFast, 10d},
        {Name.Projectile1, 1d},
        {Name.SquareD6, 0d}
    };
    protected static Dictionary<Name, double> DefaultDamage = new() {
        {Name.Enemy1, 1d},
        {Name.EnemyEasy, 1d},
        {Name.EnemyFast, 1d},
        {Name.EnemyVeryFast, 1d},
        {Name.Projectile1, 1d},
        {Name.SquareD6, 0d}
    };
    public static Dictionary<Name, Texture2D> Texture = new();
    public Entity(Game1 game, long id, Name name) : base(game,id,name)
    {
        size = Size[name];
        health = maxhealth = DefaultHealth[name];
        damage = DefaultDamage[name];
        window = new Window(this, WindowType.Entity, Texture[name], true);
    }
    public Vector2 coordinate;
    public Vector2 size;
    public Window window;
    public RectangleF Hitbox()
    {
        return new RectangleF(coordinate.X-size.X/2,coordinate.Y-size.Y/2,size.X,size.Y);
    }
    public ArrayList Collisions() {return game.Collisions(this);}
    abstract public void TickUpdateCoordinate();
    public Vector2 RenderCoordinate() {return Vector2.Round(coordinate + RenderCoordinateOffset[name]);}
    // public Texture2D RenderTexture() {return Texture[name];}
    public Vector2 velocity;
    public double maxhealth;
    public double health;
    public double damage;
}
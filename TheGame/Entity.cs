using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        {Name.Enemy1, new(32f,32f)},
        {Name.EnemyEasy, new(32f,32f)},
        {Name.EnemyFast, new(16f,16f)},
        {Name.EnemyVeryFast, new(16f,16f)},
        {Name.Projectile1, new(16f,16f)},
        {Name.Stone, new(16f,16f)},
        {Name.Arrow, new(16f,16f)},
        {Name.Spike, new(16f,16f)},
        {Name.SquareD6, new(6*64f,6*64f)}
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
        {Name.Stone, 5d},
        {Name.Arrow, 1d},
        {Name.Spike, 1d},
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
        // Damage = DefaultDamage[name];
        window = new Window(this, WindowType.Entity, EntityTexture(), Microsoft.Xna.Framework.Color.Red, true);
    }
    public Vector2 coordinate;
    public Vector2 size;
    public Window window;
    public RectangleF Hitbox()
    {
        return new RectangleF(coordinate.X-size.X/2,coordinate.Y-size.Y/2,size.X,size.Y);
    }
    public ArrayList Collisions() {return game.Collisions(this);}
    virtual public void TickUpdateCoordinate()
    {
        coordinate.X -= MathF.Floor(coordinate.X/Game1.xPeriod) * Game1.xPeriod;
        coordinate.Y -= MathF.Floor(coordinate.Y/Game1.yPeriod) * Game1.yPeriod;
    }
    public Vector2 velocity;
    public double maxhealth;
    public double health;
    public double Damage(Entity e){
        return name switch{
            Name.Stone => 0.01d * (velocity - e.velocity).LengthSquared(),
            Name.Arrow => 0.3d * (velocity - e.velocity).Length(),
            Name.Spike => 0.2d * (velocity - e.velocity).Length() + 1d,
            _ => DefaultDamage[name],
        };
    }
    private Texture2D EntityTexture()
    {
        if(Texture.ContainsKey(name)) return Texture[name];
        else return Texture[Name.Projectile1];
    }
    public Vector2 RenderCoordinate()
    {
        if(Texture.ContainsKey(name)) return Vector2.Round(coordinate + RenderCoordinateOffset[name]);
        else return Vector2.Round(coordinate + RenderCoordinateOffset[Name.Projectile1]);
    }

}
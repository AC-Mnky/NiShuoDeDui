using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace TheGame;

abstract public class Entity : Thing
{
    protected static Dictionary<Name, Vector2> RenderCoordinateOffset = new() {
        {Name.Enemy1, new(-16f,-16f)},
        {Name.EnemyEasy, new(-16f,-16f)},
        {Name.EnemyFast, new(-8f,-8f)},

        {Name.Square1, new(-16f,-16f)},
        {Name.Diamond1, new(-16f,-16f)},
        {Name.Circle1, new(-24f,-24f)},
        {Name.Cross1, new(-6f,-6f)},

        {Name.Projectile1, new(-8f,-8f)},
        {Name.Stone, new(-12f,-12f)},
        {Name.Arrow, new(-8f,-5f)},
        {Name.Spike, new(-8f,-7f)},
        {Name.SquareD6, new(-3*64f,-3*64f)},
        {Name.ExplosionSquareD6, new(-3*64f,-3*64f)},
    };
    protected static Dictionary<Name, Vector2> Size = new() {
        {Name.Enemy1, new(32f,32f)},
        {Name.EnemyEasy, new(32f,32f)},
        {Name.EnemyFast, new(16f,16f)},
        {Name.EnemyVeryFast, new(16f,16f)},

        {Name.Square1, new(32f,32f)},
        {Name.Diamond1, new(32f,32f)},
        {Name.Circle1, new(48f,48f)},
        {Name.Cross1, new(12f,12f)},

        {Name.Projectile1, new(16f,16f)},
        {Name.Stone, new(24f,24f)},
        {Name.Arrow, new(16f,10f)},
        {Name.Spike, new(16f,14f)},
        {Name.SquareD6, new(6*64f,6*64f)},
        {Name.ExplosionSquareD6, new(6*64f,6*64f)}
    };
    protected static Dictionary<Name, Color> DefaultColor = new() {
        {Name.Enemy1, Color.Red},
        {Name.EnemyEasy, Color.Red},
        {Name.EnemyFast, Color.Red},
        {Name.EnemyVeryFast, Color.Red},

        {Name.Square1, Color.Red},
        {Name.Diamond1, Color.OrangeRed},
        {Name.Circle1, Color.SaddleBrown},
        {Name.Cross1, Color.Black},

        {Name.Projectile1, Color.Blue},
        {Name.Stone, Color.DarkGray},
        {Name.Arrow, Color.White},
        {Name.Spike, Color.DarkRed},
        {Name.SquareD6, Color.White*0.05f},
        {Name.ExplosionSquareD6, Color.White*0.5f},
    };
    protected static Dictionary<Name, float> DefaultSpeed = new() {
        {Name.Enemy1, 1f},
        {Name.EnemyEasy, 8f},
        {Name.EnemyFast, 10f},
        {Name.EnemyVeryFast, 20f},

        {Name.Square1, 3f},
        {Name.Diamond1, 8f},
        {Name.Circle1, 1f},
        {Name.Cross1, 5f},

    };
    protected static Dictionary<Name, double> DefaultHealth = new() {
        {Name.Enemy1, 10d},
        {Name.EnemyEasy, 10d},
        {Name.EnemyFast, 10d},
        {Name.EnemyVeryFast, 10d},

        {Name.Square1, 10d},
        {Name.Diamond1, 5d},
        {Name.Circle1, 50d},
        {Name.Cross1, 1d},

        {Name.Projectile1, 1d},
        {Name.Stone, 3d},
        {Name.Arrow, 1d},
        {Name.Spike, 1d},
        {Name.SquareD6, -1d},
        {Name.ExplosionSquareD6, float.MinValue/2},
    };
    protected static Dictionary<Name, double> DefaultDamage = new() {
        {Name.Enemy1, 1d},
        {Name.EnemyEasy, 1d},
        {Name.EnemyFast, 1d},
        {Name.EnemyVeryFast, 1d},

        {Name.Square1, 1d},
        {Name.Diamond1, 5d},
        {Name.Circle1, 0.5d},
        {Name.Cross1, 5d},

        {Name.Projectile1, 1d},
        {Name.SquareD6, 0d},
        {Name.ExplosionSquareD6, 5d},
    };
    protected static Dictionary<Name, int> Money = new() {
        {Name.Enemy1, 1},
        {Name.EnemyEasy, 1},
        {Name.EnemyFast, 1},
        {Name.EnemyVeryFast, 1},

        {Name.Square1, 2},
        {Name.Diamond1, 4},
        {Name.Circle1, 5},
        {Name.Cross1, 1},

    };
    public static Dictionary<Name, int> CardNum = new() {
        {Name.Enemy1, 20},
        {Name.EnemyEasy, 20},
        {Name.EnemyFast, 20},
        {Name.EnemyVeryFast, 20},

        {Name.Square1, 10},
        {Name.Diamond1, 5},
        {Name.Circle1, 4},
        {Name.Cross1, 10},

    };
    public static Dictionary<int, RanDict<Name>> RandomCard = new(){
        {1, new(){
        {Name.Square1, 0},
        {Name.Diamond1, 2},
        {Name.Circle1, 2},
        {Name.Cross1, 1},
            }},
        {2, new(){
        {Name.Square1, 0},
        {Name.Diamond1, 2},
        {Name.Circle1, 2},
        {Name.Cross1, 1},
            }},
        {3, new(){
        {Name.Square1, 0},
        {Name.Diamond1, 2},
        {Name.Circle1, 2},
        {Name.Cross1, 1},
            }},
        {4, new(){
        {Name.Square1, 0},
        {Name.Diamond1, 2},
        {Name.Circle1, 2},
        {Name.Cross1, 1},
            }},
    };

    public static Dictionary<Name, Texture2D> Texture = new();
    public Entity(Name name) : base(name)
    {
        size = Size[name];
        health = maxhealth = DefaultHealth[name] / ((game.gamescene == GameScene.Build && this is Enemy) ? 5 : 1);
        window = new Window(this, WindowType.Entity, EntityTexture(), DefaultColor[name], true);
    }
    public Vector2 coordinate;
    public Vector2 size;
    public Window window;
    public RectangleF hitbox;
    // public List<Entity> Collisions() {return game.Collisions(this);}
    virtual public void TickUpdateCoordinate()
    {
        coordinate.X -= MathF.Floor(coordinate.X/game.xPeriod) * game.xPeriod;
        coordinate.Y -= MathF.Floor(coordinate.Y/game.yPeriod) * game.yPeriod;
        UpdateHitbox();
    }
    public void UpdateHitbox()
    {
        hitbox = new RectangleF(coordinate.X-size.X/2,coordinate.Y-size.Y/2,size.X,size.Y);
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
        Name n = Texture.ContainsKey(name) ? name : Name.Projectile1;
        Vector2 offset = RenderCoordinateOffset[n];
        // offset = new();
        
        return Vector2.Round(coordinate + Vector2.Transform(offset, Matrix.CreateRotationZ(Rotation())));
    }

    public float Rotation()
    {
        if(this is Enemy) return 0;
        else return MathF.Atan2(velocity.Y, velocity.X);
    }
}
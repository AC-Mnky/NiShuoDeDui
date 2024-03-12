using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Security.Cryptography;

namespace TheGame;

public enum Name {Enemy1, EnemyEasy, EnemyFast, EnemyVeryFast, Projectile1, SquareD6,
SummonEnemy, SummonProjectile1, VelocityZero, AddSpeed, Add10Speed, AddXVelocity, AddYVelocity, ReduceXVelocity, ReduceYVelocity, TriggerUponDeath, AimClosestInSquareD6, AimMouse, AimLeft, AimRight, AimUp, AimDown, AimBack, Wait60Ticks};

abstract public class Thing
{
    protected Game1 game;
    public long tickBirth;
    public long id;
    public Name name;
    public bool alive = true;
    public Thing(Game1 game, long id, Name name)
    {
        this.game = game;
        tickBirth = game.tick;
        this.name = name;
        this.id = id;
    }
    abstract public void TickUpdate(); 

    public static Vector2 Closest(Vector2 vector)
    {
        vector.Deconstruct(out float x, out float y);
        return new(x - MathF.Round(x / Game1.xPeriod) * Game1.xPeriod, y - MathF.Round(y / Game1.yPeriod) * Game1.yPeriod);
    }
    public static Vector2 Randomdirection()
    {
        int a = RandomNumberGenerator.GetInt32(2147483647);
        return new(MathF.Cos(a),MathF.Sin(a));
    }
    public static Vector2 Normalized(Vector2 vector) // 不要问我为什么把这无关的玩意放在这里。我想不到更好的地方放了。
    {
        if(vector == Vector2.Zero) return Randomdirection();
        Vector2 v = vector;
        v.Normalize();
        return v;
    }
}
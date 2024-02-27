using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TheGame;

public enum Name {Enemy1, Projectile1, SquareD6,
SummonEnemy1, SummonProjectile1, AddSpeed, Add5Speed, AddXVelocity, AddYVelocity, TriggerUponDeath, AimClosestInSquareD6, Wait60Ticks};

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


    public static Vector2 Normalized(Vector2 vector) // 不要问我为什么把这无关的玩意放在这里。我想不到更好的地方放了。
    {
        if(vector == Vector2.Zero) return Vector2.Zero;
        Vector2 v = vector;
        v.Normalize();
        return v;
    }
}
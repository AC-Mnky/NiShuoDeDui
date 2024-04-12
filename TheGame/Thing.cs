using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Security.Cryptography;
using System.Net.Mail;

namespace TheGame;

public enum Name {Null,


Enemy1, 
EnemyEasy, 
EnemyFast, 
EnemyVeryFast, 



Square1,
Diamond1,
Circle1,
Cross1,

Square2,
Diamond2,
Circle2,
Runner2,
Phasor2,
Crossgen2,
Heal2,
Dark2,
Invin2,
Ghost2,

Runner3,
Phasor3,
SpeedField3,
Circle3,
ShieldField3,
Heal3,
HealField3,
Dark3,
InvinField3,
Ghost3,



Projectile1, 
Stone, 
Arrow, 
Spike, 
SquareD6, 
ExplosionSquareD6,


SummonEnemy, 
SummonProjectile, 
SummonStone, 
SummonArrow, 
SummonSpike, 
VelocityZero, 
AddSpeed, 
Add10Speed, 
DoubleSpeed, 
AddXVelocity, 
AddYVelocity, 
ReduceXVelocity, 
ReduceYVelocity, 
TriggerUponDeath, 
AimClosestInSquareD6, 
AimMouse, 
AimLeft, 
AimRight, 
AimUp, 
AimDown, 
AimBack, 
Wait60Ticks, 
DoubleCast, 
TwiceCast,
CastEveryTick,
CastEvery8Ticks,
CastEvery64Ticks,
RandomAim,
RandomWait,
Aiming,
};

abstract public class Thing : GameObject
{
    public long tickBirth;
    public Name name;
    public bool alive = true;
    public Thing(Name name)
    {
        tickBirth = game.tick;
        this.name = name;
    }
    abstract public void TickUpdate(); 

    public static Vector2 Closest(Vector2 vector)
    {
        vector.Deconstruct(out float x, out float y);
        return new(x - MathF.Round(x / game.xPeriod) * game.xPeriod, y - MathF.Round(y / game.yPeriod) * game.yPeriod);
    }
    public static Vector2 Randomdirection()
    {
        float a = RandomNumberGenerator.GetInt32(360)*MathF.PI/180;
        return new(MathF.Cos(a),MathF.Sin(a));
    }
    public static Vector2 Normalized(Vector2 vector) // 不要问我为什么把这无关的玩意放在这里。我想不到更好的地方放了。
    {
        if(vector == Vector2.Zero) return Randomdirection();
        Vector2 v = vector;
        v.Normalize();
        return v;
    }
    public static Vector2 NormalNerfed(Vector2 vector)
    {
        if(vector.LengthSquared() < 1) return vector;
        Vector2 v = vector;
        v.Normalize();
        return v;
    }
    public static Point Max(Point A, Point B)
    {
        return new((A.X>B.X)?A.X:B.X,(A.Y>B.Y)?A.Y:B.Y);
    }
}
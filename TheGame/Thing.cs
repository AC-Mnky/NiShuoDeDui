using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TheGame;

public enum Name {Enemy1, Projectile1,
SummonEnemy1, SummonProjectile1, AddYVelocity, Wait60Ticks};

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
}
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TheGame;

abstract public class Thing
{
    protected Game1 game;
    public long tickBirth;
    public long id;
    public bool alive = true;
    public Thing(Game1 game, long id)
    {
        this.game = game;
        tickBirth = game.tick;
        this.id = id;
    }
    abstract public void TickUpdate(); 
}
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TheGame;

abstract public class Thing
{
    protected Game1 game;
    public long tickBirth;
    public bool exist = true;
    public long id;
    public Thing(Game1 game)
    {
        this.game = game;
        tickBirth = game.tick;
    }
    abstract public void TickUpdate(); 
}
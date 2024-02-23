using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TheGame;

public class Thing
{
    protected Game1 game;
    public long id;
    public Thing(Game1 game)
    {
        this.game = game;
    }
}
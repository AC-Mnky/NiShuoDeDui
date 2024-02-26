using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

public class Enemy : Entity
{
    public Enemy(Game1 game, long id, Name name, Vector2 coordinate, Vector2 velocity) : base(game, id, name, coordinate, velocity)
    {
    }





    public override void TickUpdate()
    {
        if(health <= 0d)
        {
            alive = false;
        }
    }

}
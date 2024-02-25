using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

public class Enemy : Entity
{
    public Enemy(Game1 game, long id, EntityName name, Vector2 coordinate, Vector2 velocity) : base(game, id, name, coordinate, velocity)
    {
    }





    public override void TickUpdate()
    {
        // throw new System.NotImplementedException();
    }

}
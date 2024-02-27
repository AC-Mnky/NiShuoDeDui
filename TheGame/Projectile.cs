using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

public class Projectile : Entity
{
    public Projectile(Game1 game, long id, Name name, Vector2 coordinate, Vector2 velocity) : base(game,id,name,coordinate,velocity)
    {
    }

    public override void TickUpdate()
    {
        health -= 0.005d;
        if(Collisions().Count > 0)
        {
            health -= 1d;
        }
        if(health <= 0d)
        {
            alive = false;
        }
    }

}
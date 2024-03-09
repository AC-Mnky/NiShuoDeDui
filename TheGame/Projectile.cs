using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

public class Projectile : Entity
{
    public Projectile(Game1 game, long id, Name name, Vector2 coordinate, Vector2 velocity) : base(game,id,name)
    {
        this.coordinate = coordinate;
        this.velocity = velocity;
    }


    public override void TickUpdateCoordinate()
    {
        if(alive) coordinate += velocity;
    }

    public override void TickUpdate()
    {
        health -= 0.005d;
        foreach(Entity e in Collisions())
        {
            if(e is Enemy)
                health -= 1d;
        }
        if(health <= 0d)
        {
            alive = false;
        }
    }

}
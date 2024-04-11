using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

public class Projectile : Entity
{
    public Projectile(Name name, Vector2 coordinate, Vector2 velocity) : base(name)
    {
        this.coordinate = coordinate;
        this.velocity = velocity;
    }


    public override void TickUpdateCoordinate()
    {
        if(alive)
        {
            coordinate += velocity;
        }
        base.TickUpdateCoordinate();
    }

    public override void TickUpdate()
    {
        #region speed and health drain
        switch(name)
        {
            case Name.Projectile1:
                health -= 0.005d;
                break;
            case Name.Stone:
                velocity -= 0.1f * NormalNerfed(velocity);
                if(velocity.Length() < 0.1f) health -= 0.1d;
                break;
            case Name.Arrow:
                velocity -= 0.03f * velocity;
                if(velocity.Length() < 1f) health -= 0.005d * (game.tick-tickBirth);
                break;
            case Name.Spike:
                velocity -= 0.1f * NormalNerfed(velocity);
                health -= 0.001d;
                break;
            case Name.ExplosionSquareD6:
                break;
        }
        #endregion

        foreach(Entity e in game.Collisions(this))
        {
            if(e is Enemy)
                e.health -= Damage(e);
        }
        if(health <= 0d)
        {
            alive = false;
        }
    }

}
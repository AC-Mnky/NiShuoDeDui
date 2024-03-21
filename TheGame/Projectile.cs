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
                velocity -= 0.1f * Normalized(velocity);
                if(velocity.Length() < 0.1f) health -= 0.1d;
                break;
            case Name.Arrow:
                velocity -= 0.03f * velocity;
                if(velocity.Length() < 0.1f) health -= 0.5d;
                break;
            case Name.Spike:
                velocity -= 0.1f * Normalized(velocity);
                health -= 0.005d;
                break;
        }
        #endregion

        foreach(Entity e in Collisions())
        {
            if(e is Enemy)
                health -= e.Damage(this);
        }
        if(health <= 0d)
        {
            alive = false;
        }
    }

}
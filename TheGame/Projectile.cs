using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

public class Projectile : Entity
{
    public Projectile(Game1 game, Name name, Vector2 coordinate, Vector2 velocity) : base(game,name)
    {
        this.coordinate = coordinate;
        this.velocity = velocity;
    }

    public override void TickAction()
    {
        throw new System.NotImplementedException();
    }

    public override void TickUpdateVelocity()
    {
        throw new System.NotImplementedException();
    }
}
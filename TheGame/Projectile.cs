using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

public class Projectile : Entity
{
    public Projectile(Game1 game, EntityName name, Vector2 coordinate, Vector2 velocity) : base(game,name,coordinate,velocity)
    {
    }

    public override void TickUpdate()
    {
        throw new System.NotImplementedException();
    }

}
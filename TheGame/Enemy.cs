using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

public class Enemy : Entity
{
    public Segment segment;
    public float progress;
    public float speed = 1;
    public Enemy(Game1 game, long id, Name name, Segment segment, float progress) : base(game, id, name)
    {
        this.segment = segment;
        this.progress = progress;
        coordinate = segment.CoordinateAtProgress(progress);
    }




    public override void TickUpdateCoordinate()
    {
        if(!alive) return;
        progress += speed;
        while(progress > segment.length)
        {
            progress -= segment.length;
            segment = segment.succ;
        }
        coordinate = segment.CoordinateAtProgress(progress);
    }

    public override void TickUpdate()
    {
        foreach(Entity e in Collisions())
        {
            if(e is Projectile)
                health -= 1d;
        }
        if(health <= 0d)
        {
            alive = false;
        }
    }

}
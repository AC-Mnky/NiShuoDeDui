using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

public class Enemy : Entity
{
    public Segment segment;
    public float progress;
    public float speed;
    public Enemy(Game1 game, long id, Name name, Segment segment, float progress) : base(game, id, name)
    {
        this.segment = segment;
        this.progress = progress;
        coordinate = segment.CoordinateAtProgress(progress);
        speed = DefaultSpeed[name];
    }




    public override void TickUpdateCoordinate()
    {
        if(!alive) return;
        progress += speed;
        while(progress > segment.length)
        {
            progress -= segment.length;
            if(segment == game.Bluedoor) game.Penetrated(1);
            segment = segment.succ;
        }
        coordinate = segment.CoordinateAtProgress(progress);
        base.TickUpdateCoordinate();
    }

    public override void TickUpdate()
    {
        foreach(Entity e in Collisions())
        {
            if(e is Projectile)
                health -= e.damage;
        }
        if(health <= 0d)
        {
            alive = false;
        }
    }

}
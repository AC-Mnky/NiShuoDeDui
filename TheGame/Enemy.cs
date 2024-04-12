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
    public Enemy(Name name, Segment segment, float progress) : base(name)
    {
        this.segment = segment;
        this.progress = progress;
        coordinate = segment.CoordinateAtProgress(progress);
        speed = DefaultSpeed[name];
        UpdateHitbox();
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
        foreach(Entity e in game.Collisions(this))
        {
            if(e is Projectile)
                e.health -= Damage(e);
        }
        if(health <= 0d)
        {
            alive = false;
            if(game.gamescene == GameScene.Battle) game.money += Money[name];
        }
    }

}
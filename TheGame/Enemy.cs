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
    public bool title = false;
    public bool newgame;
    public Enemy(Name name, Segment segment, float progress) : base(name)
    {
        this.segment = segment;
        this.progress = progress;
        if(segment!=null) coordinate = segment.CoordinateAtProgress(progress);
        speed = DefaultSpeed[name];
        UpdateHitbox();
    }

    public static Enemy Title(Vector2 coordinate, bool newgame)
    {
        Enemy e = new(Name.Square1, null, 0)
        {
            coordinate = coordinate,
            speed = 0,
            newgame = newgame,
            title = true,
        };
        e.UpdateHitbox();
        return e;
    }




    public override void TickUpdateCoordinate()
    {
        if(title) return;
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
            if(title)
            {
                if(newgame)
                {
                    game.gamescene = GameScene.Loading;
                    game._hasdrawn = false;
                }
                else
                {
                    game.Exit();
                }
            }
        }
    }

}
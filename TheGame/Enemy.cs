using System;
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
    public int ticksInvin = 0;
    public double healthfix;
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
            if(segment == game.Bluedoor)
            {
                if(game.gamescene == GameScene.Battle)
                    game.Penetrated(1);
                else
                    alive = false;
            }
            segment = segment.succ;
        }
        coordinate = segment.CoordinateAtProgress(progress);
        base.TickUpdateCoordinate();
    }

    public override void TickUpdate()
    {
        hit = false;
        foreach(Entity e in game.Collisions(this))
        {
            if(e is Projectile)
            {
                e.health -= Damage(e);
                hit = true;
            }
        }
        int x,y;
        switch(name)
        {
            case Name.Runner2:
                if(hit) speed = 20f;
                else speed -= (speed-5f)*0.05f;
                break;
            case Name.Runner3:
                if(hit) speed = 40f;
                else speed -= (speed-5f)*0.05f;
                break;
            case Name.Phasor2:
                if(speed > 0) speed = 0;
                if((game.tick-tickBirth)%64==0)
                {
                    speed = 192;
                    healthfix = health;
                    ticksInvin = 1;
                }
                break;
            case Name.Phasor3:
                if(speed > 0) speed = 0;
                if((game.tick-tickBirth)%48==0)
                {
                    speed = 192;
                    healthfix = health;
                    ticksInvin = 1;
                }
                break;
            case Name.Crossgen2:
                if((game.tick-tickBirth)%32==0)
                {
                    game.summoncross1.toCastNextTick.Add(new Cast(new Vector2()){segment = segment, progress = progress});
                }
                break;
            case Name.Heal2:
                health += (maxhealth - health) * 0.002f;
                break;
            case Name.Heal3:
                health += (maxhealth - health) * 0.0025f;
                break;
            case Name.Dark2:
                x = (int)MathF.Floor(coordinate.X/64);
                y = (int)MathF.Floor(coordinate.Y/64);
                game.mana[x,y] = 0;
                break;
            case Name.Dark3:
                x = (int)MathF.Floor(coordinate.X/64);
                y = (int)MathF.Floor(coordinate.Y/64);
                game.mana[x,y] = -game.manaMax;
                break;
            case Name.Invin2:
                if(ticksInvin <= 0 && hit)
                {
                    healthfix = health;
                    ticksInvin = 32;
                }
                break;
            // case Name.InvinField3:
            //     break;
            default:
                break;
        }
        if(--ticksInvin >= 0)
        {
            health = healthfix;
            return;
        }
        ticksInvin = 0;
        healthfix = health;
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
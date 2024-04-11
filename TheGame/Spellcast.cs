using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TheGame;


public class Spellcast : Thing
{
    public Spell spell;
    public Cast cast;
    private int int1;
    public Vector2 CurrentCoordinate()
    {
        return cast.CurrentCoordinate();
    }
    public Spellcast(Spell spell, Cast cast) : base(spell.name)
    {
        this.spell = spell;
        this.cast = cast;
    }





    public override void TickUpdate()
    {
        if(cast.type == CastType.Independent && spell.dependentOnly) alive = false; // 短路
        else if(cast.type == CastType.Dependent && !cast.subject.alive) alive = false; // 分类：亡语
        else
        {
            Entity x;
            switch(spell.name)
            {
                case Name.SummonEnemy:
                    x = game.NewEnemy(spell.summonedEntity, game.Reddoor, 0f);
                    AttemptCastChild(1, new(x));
                    alive = false;
                    break;
                case Name.SummonProjectile:
                    x = game.NewProjectile(spell.summonedEntity, CurrentCoordinate(), Vector2.Zero);
                    AttemptCastChild(1, new(x){direction = cast.direction});
                    alive = false;
                    break;
                case Name.VelocityZero:
                    cast.subject.velocity = Vector2.Zero;
                    alive = false;
                    break;
                case Name.AddSpeed:
                    cast.subject.velocity += 2 * Normalized(cast.direction);
                    alive = false;
                    break;
                case Name.Add10Speed:
                    cast.subject.velocity += 10 * Normalized(cast.direction);
                    alive = false;
                    break;
                case Name.DoubleSpeed:
                    cast.subject.velocity *= 2;
                    alive = false;
                    break;
                case Name.AddXVelocity:
                    cast.subject.velocity.X += 2;
                    alive = false;
                    break;
                case Name.AddYVelocity:
                    cast.subject.velocity.Y += 2;
                    alive = false;
                    break;
                case Name.ReduceXVelocity:
                    cast.subject.velocity.X -= 2;
                    alive = false;
                    break;
                case Name.ReduceYVelocity:
                    cast.subject.velocity.Y -= 2;
                    alive = false;
                    break;
                case Name.AimClosestInSquareD6:
                    x = game.NewProjectile(Name.SquareD6, CurrentCoordinate(), Vector2.Zero);
                    float minDistance = float.PositiveInfinity;
                    foreach(Entity e in game.Collisions(x)) if(e is Enemy)
                    {
                        Vector2 r = Closest(e.coordinate-x.coordinate);
                        float l = r.Length();
                        if(l < minDistance)
                        {
                            minDistance = l;
                            cast.direction = r;
                        }
                    }
                    x.alive = false;
                    alive = false;
                    break;
                case Name.AimBack:
                    cast.direction = -cast.direction;
                    alive = false;
                    break;
                case Name.AimUp:
                    cast.direction = new(0,-1);
                    alive = false;
                    break;
                case Name.AimDown:
                    cast.direction = new(0,1);
                    alive = false;
                    break;
                case Name.AimLeft:
                    cast.direction = new(-1,0);
                    alive = false;
                    break;
                case Name.AimRight:
                    cast.direction = new(1,0);
                    alive = false;
                    break;
                case Name.AimMouse:
                    cast.direction = Closest(game.MouseCoor - CurrentCoordinate()) / 64;
                    alive = false;
                    break;
                case Name.Wait60Ticks:
                    if(game.tick - tickBirth >= 60) alive = false;
                    break;
                case Name.DoubleCast:
                    AttemptCastChild(1);
                    alive = false;
                    break;
                case Name.TwiceCast:
                    AttemptCastChild(0);
                    alive = false;
                    break;
                case Name.CastEveryTick:
                    AttemptCastChild(0);
                    ++int1;
                    if(int1>=64)
                    {
                        alive = false;
                        return;
                    }
                    break;
                case Name.CastEvery8Ticks:
                    if((tickBirth-game.tick)%8==0 && tickBirth!=game.tick)
                    {
                        AttemptCastChild(0);
                        ++int1;
                    }
                    if(int1>=16)
                    {
                        alive = false;
                        return;
                    }
                    break;
                case Name.CastEvery64Ticks:
                    if((tickBirth-game.tick)%64==0 && tickBirth!=game.tick)
                    {
                        AttemptCastChild(0);
                        ++int1;
                    }
                    if(int1>=4)
                    {
                        alive = false;
                        return;
                    }
                    break;
            }
        }
        if(!alive) // 结束后施放后继法术
        {
            if(cast.type == CastType.Dependent && !cast.subject.alive) AttemptCastChild(0, new(cast,true));
            else AttemptCastChild(0);
        }
    }

    private void AttemptCastChild(int i, Cast c = null)
    {
        spell.children[i]?.toCastNextTick.Add(new(c??cast){manaMul = cast.manaMul*spell.childrenManaMul[i]});
    }
}
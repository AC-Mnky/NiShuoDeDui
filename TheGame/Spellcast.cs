using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TheGame;


public class Spellcast : Thing
{
    public Spell spell;
    public Cast cast;
    public Vector2 CurrentCoordinate()
    {
        if (cast.type == CastType.Dependent) return cast.subject.coordinate;
        else return cast.coordinate;
    }
    public Spellcast(Game1 game, long id, Spell spell, Cast cast) : base(game,id,spell.name)
    {
        this.spell = spell;
        this.cast = cast;
    }





    public override void TickUpdate()
    {
        Debug.Assert(!(Spell.dependentOnly[spell.name] && cast.type == CastType.Independent));
        if(cast.type == CastType.Dependent && !cast.subject.alive) // 分类：亡语
            {
                switch(spell.name)
                {
                    case Name.TriggerUponDeath:
                    {
                        spell.children[0]?.toCastNextTick.Add(new Cast(CurrentCoordinate()));
                        break;
                    }
                }
                alive = false;
            }
        else
        {
            switch(spell.name)
            {
                case Name.SummonEnemy1:
                {
                    Entity x = game.NewEnemy(Name.Enemy1, CurrentCoordinate(), Vector2.Zero);
                    spell.children[1]?.toCastNextTick.Add(new Cast(x));
                    alive = false;
                    break;
                }
                case Name.SummonProjectile1:
                {
                    Entity x = game.NewProjectile(Name.Projectile1, CurrentCoordinate(), Vector2.Zero);
                    spell.children[1]?.toCastNextTick.Add(new Cast(x));
                    alive = false;
                    break;
                }
                case Name.AddSpeed:
                {
                    cast.subject.velocity += Normalized(cast.direction);
                    alive = false;
                    break;
                }
                case Name.Add5Speed:
                {
                    cast.subject.velocity += 5 * Normalized(cast.direction);
                    alive = false;
                    break;
                }
                case Name.AddXVelocity:
                {
                    ++cast.subject.velocity.X;
                    alive = false;
                    break;
                }
                case Name.AddYVelocity:
                {
                    ++cast.subject.velocity.Y;
                    alive = false;
                    break;
                }
                case Name.AimClosestInSquareD6:
                {
                    Entity x = game.NewProjectile(Name.SquareD6, CurrentCoordinate(), Vector2.Zero);
                    float minDistance = float.PositiveInfinity;
                    foreach(Entity e in x.Collisions()) if(e is Enemy)
                    {
                        Vector2 r = e.coordinate-x.coordinate;
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
                }
                case Name.Wait60Ticks:
                {
                    if(game.tick - tickBirth >= 60) alive = false;
                    break;
                }
            }
            if(!alive) // 结束后施放后继法术
            {
                spell.children[0]?.toCastNextTick.Add(cast.Clone());
            }
        }
    }
}
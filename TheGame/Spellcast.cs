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
        if(Spell.dependentOnly[spell.name] && cast.type == CastType.Independent) // 这种情况根本不该发生，因为这个施术会失败。
        {
            alive = false;
            Debug.Print("Something is wrong");
        }
        if(cast.type == CastType.Dependent && !cast.subject.alive) // 分类：亡语
            {
                // switch(spell.name)
                // {

                // }
                alive = false;
            }
        else
            switch(spell.name)
            {
                case Name.SummonEnemy1:
                {
                    Entity x = game.NewEnemy(Name.Enemy1, CurrentCoordinate(), Vector2.Zero);
                    spell.children[0]?.toCastNextTick.Add(new Cast(x));
                    alive = false;
                    break;
                }
                case Name.SummonProjectile1:
                {
                    Entity x = game.NewProjectile(Name.Projectile1, CurrentCoordinate(), Vector2.Zero);
                    spell.children[0]?.toCastNextTick.Add(new Cast(x));
                    alive = false;
                    break;
                }
                case Name.AddYVelocity:
                {
                    ++cast.subject.velocity.Y;
                    alive = false;
                    break;
                }
                case Name.Wait60Ticks:
                {
                    if(game.tick - tickBirth >= 60) alive = false;
                    break;
                }
            }
        if(!alive)
        {
            spell.suffix?.toCastNextTick.Add(cast.Clone());
        }
    }
}
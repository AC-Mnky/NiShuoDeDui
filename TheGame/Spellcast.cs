using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TheGame;


public class Spellcast : Thing
{
    public Spell origin;
    public SpellName name;
    public Cast cast;
    public Vector2 CurrentCoordinate()
    {
        if (cast.type != CastType.Independent) return cast.subject.coordinate;
        else return cast.coordinate;
    }
    public Spellcast(Game1 game, Spell origin, Cast cast) : base(game)
    {
        id = game.spellcasts.Count;
        this.origin = origin;
        name = origin.name; 
        this.cast = cast; // 这里直接把引用传过去了，以后可能会出bug。
    }





    public override void TickUpdate()
    {
        if(Spell.dependentOnly[name] && !cast.IsDependent())
        {
            game.spellcasts.Remove(id);
            exist = false;
        }
        if(!exist) return;
        switch(name)
        {
            case SpellName.SummonEnemy1:
            {
                Entity x = game.entities[game.entities.Count] = new Enemy(game, EntityName.Enemy1, CurrentCoordinate(), new Vector2(1,0));
                origin.children[0]?.toCastNextTick.Add(new Cast(x));
                game.spellcasts.Remove(id);
                break;
            }
            case SpellName.AddYSpeed:
            {
                ++cast.subject.velocity.Y;
                game.spellcasts.Remove(id);
                break;
            }
        }
    }
}
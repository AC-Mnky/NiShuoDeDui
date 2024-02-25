using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TheGame;


public class Spellcast : Thing
{
    public Spell origin;
    public SpellName name;
    public Cast cast;
    public long tickBirth;
    public CastType dependence;
    public Vector2 coordinate;
    public Vector2 currentCoordinate()
    {
        if(dependence == CastType.Independent) return coordinate;
        else return game.entities[subjectId].coordinate;
    }
    public long subjectId;
    public Vector2 direction = Vector2.Zero;
    public Spellcast(Game1 game, Spell origin, CastType dependence, Vector2 coordinate, long subjectId) : base(game)
    {
        id = game.spellcasts.Count;
        tickBirth = game.tick;
        this.origin = origin;
        name = origin.name;
        this.dependence = dependence;
        this.coordinate = coordinate;
        this.subjectId = subjectId;
    }





    public void TickUpdate(long tick)
    {
        switch(name)
        {
            case SpellName.SummonEnemy1:
            {
                Entity x = game.entities[game.entities.Count] = new Enemy(game, EntityName.Enemy1, currentCoordinate(), new Vector2(1,0));
                if(origin.children[0] != null)
                    // origin.children[0].Cast(CastType.Dependent,new Vector2(), x.id);
                game.spellcasts.Remove(id);
                break;
            }
            case SpellName.AddYSpeed:
            {
                if(dependence == CastType.Independent || !game.entities.ContainsKey(subjectId)) game.spellcasts.Remove(id);
                else
                {
                    ++game.entities[subjectId].velocity.Y;
                    game.spellcasts.Remove(id);
                }
                break;
            }
        }
    }
}
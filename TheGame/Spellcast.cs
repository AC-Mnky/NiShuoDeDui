using System;
using Microsoft.Xna.Framework;

namespace TheGame;

public class Spellcast : Thing
{
    public Spell.Name name;
    public long tickBirth;
    public enum Dependence {Independent, Dependent};
    public Dependence dependence;
    public Vector2 coordinate;
    public Vector2 currentCoordinate()
    {
        if(dependence == Dependence.Independent) return coordinate;
        else return game.entities[subjectId].coordinate;
    }
    public long subjectId;
    public Vector2 direction = Vector2.Zero;
    public Spellcast(Game1 game, Spell.Name name, Dependence dependence, Vector2 coordinate, long subjectId) : base(game)
    {
        id = game.spellcasts.Count;
        tickBirth = game.tick;
        this.name = name;
        this.dependence = dependence;
        this.coordinate = coordinate;
        this.subjectId = subjectId;
    }

    public void TickUpdate(long tick)
    {
        game.entities[game.entities.Count] = new Enemy(game, Entity.Name.Enemy1, currentCoordinate(), new Vector2(1,0));
        game.spellcasts.Remove(id);
    }
}
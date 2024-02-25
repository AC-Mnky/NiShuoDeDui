using System;
using Microsoft.Xna.Framework;

namespace TheGame;

public enum SpellName {SummonEnemy1, SummonProjectile1};
public enum CastType {Independent, Dependent};


public class Spellcast : Thing
{
    public SpellName name;
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
    public Spellcast(Game1 game, SpellName name, CastType dependence, Vector2 coordinate, long subjectId) : base(game)
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
        switch(name)
        {
            case SpellName.SummonEnemy1:
            {
                game.entities[game.entities.Count] = new Enemy(game, EntityName.Enemy1, currentCoordinate(), new Vector2(1,0));
                game.spellcasts.Remove(id);
                break;
            }
        }
    }
}
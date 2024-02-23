
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace TheGame;

public class Spell : Thing
{
    public enum Name {SummonEnemy1};
    public Name name;
    public enum Affiliation {Desk, Map, Child, Suffix};
    public Affiliation affiliation;
    public int deskIndex;
    public int mapI, mapJ;
    public Vector2 Coordinate() {return new Vector2(mapI*64f+32f, mapJ*64f+32f);}
    public long coolDownMax;
    public long coolDown;
    public Spell parent;
    public Spell[] children;
    public Spell suffix;
    public Spell(Game1 game, Name name, Affiliation affiliation, long coolDownMax) : base(game)
    {
        this.name = name;
        this.affiliation = affiliation;
        this.coolDownMax = coolDownMax;
        coolDown = 0;
    }

    public void TickUpdate(long tick)
    {
        if(affiliation == Affiliation.Map)
        {
            if(coolDown > 1) --coolDown;
            else
            {
                // CAST
                game.spellcasts[game.spellcasts.Count] = new Spellcast(game, name, Spellcast.Dependence.Independent, Coordinate(), -1);
                // game.entities[game.entities.Count] = new Enemy(game, Entity.Name.Enemy1, new Vector2(32,32), new Vector2(1,0));
                coolDown = coolDownMax;
            }
        }
    }
}
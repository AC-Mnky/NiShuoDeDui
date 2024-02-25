
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace TheGame;

public class Spell : Thing
{
    public SpellName name;
    public enum Affiliation {Desk, Map, Child, Suffix}; // 法术是在台面上（未使用），还是在图上（可以直接触发），还是某个法术的子法术，还是某个法术的后继法术
    public Affiliation affiliation;
    public int deskIndex; // 如果在台面上，它的编号
    public int mapI, mapJ; // 如果在图上，它的坐标
    public Vector2 Coordinate() {return new Vector2(mapI*64f+32f, mapJ*64f+32f);}
    public Spell parent; // 如果是子法术或后继法术，那么它挂在哪个法术身上
    public Spell[] children; // 子法术是谁
    public Spell suffix; // 后继法术是谁
    public long coolDownMax;
    public long coolDown;
    public Spell(Game1 game, SpellName name, Affiliation affiliation, long coolDownMax) : base(game)
    {
        id = game.spells.Count;
        this.name = name;
        this.affiliation = affiliation;
        this.coolDownMax = coolDownMax;
        coolDown = 0;
    }





    public void Cast(CastType dependence, Vector2 coordinate, long subjectId)
    {
        game.spellcasts[game.spellcasts.Count] = new Spellcast(game, name, dependence, coordinate, subjectId);
    }
    public void TickUpdate(long tick)
    {
        if(affiliation == Affiliation.Map)
        {
            if(coolDown > 1) --coolDown;
            else
            {
                Cast(CastType.Independent, Coordinate(), -1);
                coolDown = coolDownMax;
            }
        }
    }
}
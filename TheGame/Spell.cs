
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TheGame;

public enum SpellName {SummonEnemy1, SummonProjectile1, AddYVelocity};

public class Spell : Thing
{
    public SpellName name;
    public enum Affiliation {Desk, Map, Child, Suffix, Null}; // 法术是在台面上（未使用），还是在图上（可以直接触发），还是某个法术的子法术，还是某个法术的后继法术
    public Affiliation affiliation = Affiliation.Null;
    public int deskIndex = -1; // 如果在台面上，它的编号
    public int mapI, mapJ; // 如果在地图上，它的坐标
    public Vector2 Coordinate() {return new Vector2(mapI*64f+32f, mapJ*64f+32f);}
    public Spell parent = null; // 如果是子法术或后继法术，那么它挂在哪个法术身上
    public int rank = -1; // 如果是子法术，那么是第几个
    protected static Dictionary<SpellName, int> childrenNumber = new() {
        {SpellName.SummonEnemy1, 1},
        {SpellName.SummonProjectile1, 1},
        {SpellName.AddYVelocity, 0}
    };
    public static Dictionary<SpellName, bool> dependentOnly = new() {
        {SpellName.SummonEnemy1, false},
        {SpellName.SummonProjectile1, false},
        {SpellName.AddYVelocity, true}
    };
    public Spell[] children; // 子法术是谁
    public Spell suffix = null; // 后继法术是谁
    public ArrayList toCastNextTick = new(); // 一个列表，存放下一刻开始时将要进行的施放
    public long coolDownMax;
    public long coolDown;
    public Spell(Game1 game, long id, SpellName name, long coolDownMax) : base(game, id)
    {
        this.name = name;
        this.coolDownMax = coolDownMax;
        children = new Spell[childrenNumber[name]];
    }




    private void Detach()
    {
        switch(affiliation)
        {
            case Affiliation.Desk:
            {
                deskIndex = -1;
                break;
            }
            case Affiliation.Map:
            {
                break;
            }
            case Affiliation.Child:
            {
                parent.children[rank] = null;
                rank = -1;
                parent = null;
                break;
            }
            case Affiliation.Suffix:
            {
                parent.suffix = null;
                parent = null;
                break;
            }
            case Affiliation.Null:
            {
                break;
            }
        }
        affiliation = Affiliation.Null;
    }
    public void AffiliateAsDesk(int deskIndex)
    {
        Detach();
        this.deskIndex = deskIndex;
        affiliation = Affiliation.Desk;
    }
    public void AffiliateAsMap(int mapI, int mapJ)
    {
        Detach();
        this.mapI = mapI;
        this.mapJ = mapJ;
        affiliation = Affiliation.Map;
        coolDown = coolDownMax;
    }
    public void AffiliateAsChild(Spell parent, int rank)
    {
        Detach();
        this.parent = parent;
        this.rank = rank;
        parent.children[rank] = this;
        affiliation = Affiliation.Child;
    }
    public void AffiliateAsSuffix(Spell parent)
    {
        Detach();
        this.parent = parent;
        parent.suffix = this;
        affiliation = Affiliation.Suffix;
    }



    public override void TickUpdate()
    {
        if(affiliation == Affiliation.Map)
        {
            if(coolDown > 1) --coolDown;
            else
            {
                toCastNextTick.Add(new Cast(Coordinate()));
                coolDown = coolDownMax;
            }
        }
    }
    public void TickCast()
    {
        foreach(Cast c in toCastNextTick)
            game.NewSpellcast(this, c); // 其实不一定成功，所以以后要加上if
        toCastNextTick.Clear();
    }

}
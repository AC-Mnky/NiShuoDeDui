
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheGame;


public class Spell : Thing
{
    public static Dictionary<Name, int> childrenNumber = new() {
        {Name.SummonEnemy1, 2},
        {Name.SummonProjectile1, 2},
        {Name.AddSpeed, 1},
        {Name.Add5Speed, 1},
        {Name.AddXVelocity, 1},
        {Name.AddYVelocity, 1},
        {Name.TriggerUponDeath, 1},
        {Name.AimClosestInSquareD6, 1},
        {Name.Wait60Ticks, 1}
    };
    public static Dictionary<Name, bool> dependentOnly = new() {
        {Name.SummonEnemy1, false},
        {Name.SummonProjectile1, false},
        {Name.AddSpeed, true},
        {Name.Add5Speed, true},
        {Name.AddXVelocity, true},
        {Name.AddYVelocity, true},
        {Name.TriggerUponDeath, true},
        {Name.AimClosestInSquareD6, false},
        {Name.Wait60Ticks, false}
    };
    public static Dictionary<Name, Texture2D> TextureUI = new();
    public static Dictionary<Name, Texture2D> TextureIcon = new();
    public enum Affiliation {Desk, Map, Child, Null}; // 法术是在台面上（未使用），还是在图上（可以直接触发），还是某个法术的子法术
    public Affiliation affiliation = Affiliation.Null;
    public int deskIndex = -1; // 如果在台面上，它的编号
    public int mapI, mapJ; // 如果在地图上，它的坐标
    public Vector2 Coordinate() {return new Vector2(mapI*64f+32f, mapJ*64f+32f);}
    public Spell parent = null; // 如果是子法术，那么它挂在哪个法术身上
    public int rank = -1; // 如果是子法术，那么是第几个
    public Spell[] children; // 子法术列表（第零项是后继法术）
    // public Spell suffix = null; // 后继法术是谁
    public ArrayList toCastNextTick = new(); // 一个列表，存放下一刻开始时将要进行的施放
    public long coolDownMax = -1;
    public long coolDown;
    public Window windowIcon;
    public Window windowUI;
    public Window[] windowSlots;
    public bool showUI = false;
    public Spell(Game1 game, long id, Name name) : base(game, id, name)
    {
        children = new Spell[childrenNumber[name]];
        windowIcon = new Window(this, Window.Type.SpellIcon, TextureIcon[name]);
        windowUI = new Window(this, Window.Type.SpellIcon, TextureUI[name]);
        windowSlots = new Window[childrenNumber[name]];
    }




    private void Detach()
    {
        switch(affiliation)
        {
            case Affiliation.Desk:
            {
                game.desk[deskIndex] = null;
                deskIndex = -1;
                break;
            }
            case Affiliation.Map:
            {
                game.spellAt[mapI, mapJ] = null;
                coolDownMax = -1;
                break;
            }
            case Affiliation.Child:
            {
                parent.children[rank] = null;
                rank = -1;
                parent = null;
                break;
            }
            // case Affiliation.Suffix:
            // {
            //     parent.suffix = null;
            //     parent = null;
            //     break;
            // }
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
        game.desk[deskIndex] = this;
        affiliation = Affiliation.Desk;
    }
    public void AffiliateAsMap(int mapI, int mapJ, long coolDownMax)
    {
        Detach();
        this.mapI = mapI;
        this.mapJ = mapJ;
        affiliation = Affiliation.Map;
        game.spellAt[mapI,mapJ] = this;
        this.coolDownMax = coolDownMax;
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
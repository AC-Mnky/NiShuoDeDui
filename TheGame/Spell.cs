
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
        {Name.VelocityZero, 1},
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
        {Name.VelocityZero, true},
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
    public static Dictionary<(Name, int), Texture2D> TextureSlot = new();
    public Attachment attachment = new();
    public Vector2 Coordinate() {return new Vector2(attachment.mapI*64f+32f, attachment.mapJ*64f+32f);}
    public Spell[] children; // 子法术列表（第零项是后继法术）
    public ArrayList toCastNextTick = new(); // 一个列表，存放下一刻开始时将要进行的施放
    public long coolDown;
    public Window windowIcon;
    public Window windowUI;
    public Window[] windowSlots;
    public bool showUI = false;
    public Spell(Game1 game, long id, Name name) : base(game, id, name)
    {
        children = new Spell[childrenNumber[name]];
        windowIcon = new Window(this, Window.Type.SpellIcon, TextureIcon[name]);
        windowUI = new Window(this, Window.Type.SpellUI, TextureUI[name]);
        windowSlots = new Window[childrenNumber[name]];
        for(int r=0;r<childrenNumber[name];++r)
        {
            windowSlots[r] = new Window(this, Window.Type.SpellSlot, TextureSlot[(name,r)]);
            windowSlots[r].rank = r;
        }
    }




    private Attachment Detach()
    {
        Attachment old = attachment;
        switch(attachment.type)
        {
            case Attachment.Type.Desk:
            {
                game.desk[attachment.deskIndex] = null;
                break;
            }
            case Attachment.Type.Map:
            {
                game.spellAt[attachment.mapI, attachment.mapJ] = null;
                break;
            }
            case Attachment.Type.Child:
            {
                attachment.parent.children[attachment.rank] = null;
                break;
            }
            case Attachment.Type.Null:
            {
                break;
            }
        }
        attachment = new();
        return old;
    }
    public Attachment ReAttach(Attachment target)
    {
        Attachment old = Detach();
        attachment = target;
        switch(target.type)
        {
            case Attachment.Type.Desk:
            {
                game.desk[target.deskIndex] = this;
                break;
            }
            case Attachment.Type.Map:
            {
                game.spellAt[target.mapI, target.mapJ] = this;
                coolDown = attachment.coolDownMax;
                break;
            }
            case Attachment.Type.Child:
            {
                target.parent.children[target.rank] = this;
                break;
            }
            case Attachment.Type.Null:
            {
                break;
            }
        }
        return old;
    }


    public override void TickUpdate()
    {
        if(attachment.type == Attachment.Type.Map)
        {
            if(coolDown > 1) --coolDown;
            else
            {
                toCastNextTick.Add(new Cast(Coordinate()));
                coolDown = attachment.coolDownMax;
            }
        }
    }
    public void TickCast()
    {
        foreach(Cast c in toCastNextTick)
            if(!(dependentOnly[name] && c.type == CastType.Independent))
                game.NewSpellcast(this, c); // 其实不一定成功，所以以后要加上if
        toCastNextTick.Clear();
    }

}
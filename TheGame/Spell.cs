
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
    // private static Dictionary<Name, bool> _dependentOnly = new() {
    //     {Name.SummonEnemy, false},
    //     {Name.SummonProjectile, false},
    //     {Name.VelocityZero, true},
    //     {Name.AddSpeed, true},
    //     {Name.Add10Speed, true},
    //     {Name.AddXVelocity, true},
    //     {Name.AddYVelocity, true},
    //     {Name.ReduceXVelocity, true},
    //     {Name.ReduceYVelocity, true},
    //     {Name.TriggerUponDeath, true},
    //     {Name.AimClosestInSquareD6, false},
    //     {Name.AimMouse, false},
    //     {Name.AimUp, false},
    //     {Name.AimDown, false},
    //     {Name.AimLeft, false},
    //     {Name.AimRight, false},
    //     {Name.AimBack, false},
    //     {Name.Wait60Ticks, false}
    // };
    public readonly bool dependentOnly;
    public static Dictionary<Name, Texture2D> TextureUI = new();
    public static Dictionary<Name, Texture2D> TextureIcon = new();
    public static Dictionary<(Name, int), Texture2D> TextureSlot = new();
    public Attachment attachment = new();
    public Vector2 Coordinate() {return attachment.tower.Coordinate();}
    public Spell[] children; // 子法术列表（第零项是后继法术）
    public List<Cast> toCastNextTick = new(); // 一个列表，存放下一刻开始时将要进行的施放
    public long coolDown;
    public Name summonedEntity = Name.Enemy1;
    public Window windowIcon;
    public Window windowUI;
    public Window[] windowSlots;
    // public Point[] slotOffset;
    public bool showUI = false;
    public double showLayer = 0;
    public int price = 2;
    public bool used = false;
    public Spell(Game1 game, Name name) : base(game, name)
    {
        dependentOnly = name switch{Name.VelocityZero or Name.AddSpeed or Name.Add10Speed or Name.AddXVelocity or Name.AddYVelocity or Name.ReduceXVelocity or Name.ReduceYVelocity or Name.TriggerUponDeath => true, _ => false};
        windowIcon = new Window(this, WindowType.SpellIcon, IconTexture(), Color.White);
        windowUI = new Window(this, WindowType.SpellDescription, TextureUI[name], Color.White, clickable: false){
            text = "THIS SPELL",
            textOffset = new(64,14)
        };
        switch(name)
        {
            case Name.SummonEnemy:
            case Name.SummonProjectile:
                children = new Spell[2];
                windowSlots = new Window[2]
                {
                    new (this, WindowType.SpellSlot, TextureSlot[(name, 0)],Color.Aqua){rank = 0, text = "TO CAST UPON EXPIRATION", textOffset = new(64,128+14), textColor = Color.Aqua},
                    new (this, WindowType.SpellSlot, TextureSlot[(name, 1)],Color.BlueViolet){rank = 1, text = "TO CAST ON SUMMONED ENTITY", textOffset = new(128,64+14), textColor = Color.BlueViolet},
                };
                break;
            default:
                children = new Spell[1];
                windowSlots = new Window[1]{
                    new (this, WindowType.SpellSlot, TextureSlot[(name, 0)],Color.Aqua){rank = 0, text = "TO CAST UPON EXPIRATION", textOffset = new(64,64+14), textColor = Color.Aqua},
                };
                break;
        }
    }




    private Attachment Detach()
    {
        Attachment old = attachment;
        switch(attachment.type)
        {
            case Attachment.Type.Inventory:
            {
                if(attachment.index >= 0) game.inventory[attachment.index] = null;
                else game.shop[-attachment.index] = null;
                break;
            }
            case Attachment.Type.Tower:
            {
                // game.spellAt[attachment.tower.MapI(), attachment.tower.MapJ()] = null;
                attachment.tower.spell = null;
                break;
            }
            case Attachment.Type.Child:
            {
                attachment.parent.children[attachment.index] = null;
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
            case Attachment.Type.Inventory:
            {
                if(target.index >= 0) game.inventory[target.index] = this;
                else game.shop[-target.index] = this;
                break;
            }
            case Attachment.Type.Tower:
            {
                // game.spellAt[target.tower.MapI(), target.tower.MapJ()] = this;
                target.tower.spell = this;
                coolDown = attachment.tower.coolDownMax;
                break;
            }
            case Attachment.Type.Child:
            {
                target.parent.children[target.index] = this;
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
        if(attachment.type == Attachment.Type.Tower)
        {
            if(coolDown > 1) --coolDown;
            else
            {
                toCastNextTick.Add(new Cast(Coordinate()));
                coolDown = attachment.tower.coolDownMax;
            }
        }
    }
    public void TickCast()
    {
        foreach(Cast c in toCastNextTick)
            // if(!(dependentOnly[name] && c.type == CastType.Independent))
            {
                game.NewSpellcast(this, c); // 其实不一定成功，所以以后要加上if
                used = true;
            }
        toCastNextTick.Clear();
    }

    private Texture2D IconTexture()
    {
        return TextureIcon[name];
    }
}
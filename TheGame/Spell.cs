
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
    public static Dictionary<Name, Texture2D> _TextureIcon = new();
    public static Dictionary<(Name, int), Texture2D> TextureSlot = new();
    public Attachment attachment = new();
    public Vector2 Coordinate() {return attachment.tower.Coordinate();}
    public Spell[] children; // 子法术列表（第零项是后继法术）
    public List<Cast> toCastNextTick = new(); // 一个列表，存放下一刻开始时将要进行的施放
    public long coolDown;
    public Name summonedEntity;
    public Window windowIcon;
    public Window windowSlot;
    public Window windowUIouter;
    public Window windowUI;
    public Window[] windowSlots;
    public Point UIsize;
    public bool showUI = false;
    public double showLayer = 0;
    public int price = 2;
    public bool used = false;
    public Spell(Game1 game, Name name, Name summonedEntity = Name.Null) : base(game, name)
    {
        this.summonedEntity = summonedEntity;
        dependentOnly = name switch{Name.VelocityZero or Name.AddSpeed or Name.Add10Speed or Name.AddXVelocity or Name.AddYVelocity or Name.ReduceXVelocity or Name.ReduceYVelocity or Name.TriggerUponDeath => true, _ => false};
        windowIcon = new Window(this, WindowType.SpellIcon, IconTexture(), Color.White);
        windowSlot = new Window(this, WindowType.SpellSlot, null, Color.White);
        UIsize = new(64,64);
        windowUIouter = new Window(this, WindowType.SpellDescription, Game1.whiteTexture, Color.White, clickable: false);
        windowUI = new Window(this, WindowType.SpellDescription, Game1.whiteTexture, Color.Black, clickable: false){
            text = name switch{
                Name.SummonProjectile => "SUMMON " + summonedEntity switch{
                    Name.Projectile1 => "AN ORDINARY PROJECTILE",
                    Name.Stone => "A STONE",
                    Name.Arrow => "AN ARROW",
                    Name.Spike => "A SPIKE",
                    _ => "SOMETHING WEIRD",
                },
                Name.VelocityZero => "REMOVE VELOCITY",
                Name.AddSpeed => "ADD SPEED",
                Name.Add10Speed => "ADD MUCH SPEED",
                Name.AddXVelocity => "SPEED EAST",
                Name.AddYVelocity => "SPEED SOUTH",
                Name.ReduceXVelocity => "SPEED WEST",
                Name.ReduceYVelocity => "SPEED NORTH",
                Name.TriggerUponDeath => "CAST SPELL UPON ENTITY EXPIRATION",
                Name.AimClosestInSquareD6 => "AIM AT CLOSEST ENEMY IN A SQUARE RANGE",
                Name.AimMouse => "AIM AT MOUSE",
                Name.AimUp => "AIM NORTH",
                Name.AimDown => "AIM SOUTH",
                Name.AimLeft => "AIM WEST",
                Name.AimRight => "AIM EAST",
                Name.AimBack => "AIM BACK",
                Name.Wait60Ticks => "WAIT A SECOND",
                _ => "IF YOU SEE THIS PLEASE REPORT THIS AS A BUG"
            },
            text2 = "NO MANA COST",
            textOffset = new(64,14),
            text2Offset = new(64,38),
        };
        switch(name)
        {
            case Name.SummonEnemy:
            case Name.SummonProjectile:
                UIsize = Max(UIsize, new(128, 192));
                children = new Spell[2];
                windowSlots = new Window[2]
                {
                    new (this, WindowType.SpellSlots, TextureSlot[(name, 0)],Color.Aqua){rank = 0, text = "TO CAST UPON EXPIRATION", textOffset = new(64,128+14), textColor = Color.Aqua},
                    new (this, WindowType.SpellSlots, TextureSlot[(name, 1)],Color.BlueViolet){rank = 1, text = "TO CAST ON SUMMONED ENTITY", textOffset = new(128,64+14), textColor = Color.BlueViolet},
                };
                break;
            default:
                UIsize = Max(UIsize, new(64, 128));
                children = new Spell[1];
                windowSlots = new Window[1]{
                    new (this, WindowType.SpellSlots, TextureSlot[(name, 0)],Color.Aqua){rank = 0, text = "TO CAST UPON EXPIRATION", textOffset = new(64,64+14), textColor = Color.Aqua},
                };
                break;
        }
        if(windowUI.text != null) UIsize = Max(UIsize, new(64+(int)(game._font.MeasureString(windowUI.text).X*windowUI.textScale)+10,0));
        if(windowUI.text2 != null) UIsize = Max(UIsize, new(64+(int)(game._font.MeasureString(windowUI.text2).X*windowUI.textScale)+10,0));
        foreach(Window ws in windowSlots)
        {
            if(ws.text != null) UIsize = Max(UIsize, new(ws.textOffset.X+(int)(game._font.MeasureString(ws.text).X*ws.textScale)+10,0));
            if(ws.text2 != null) UIsize = Max(UIsize, new(ws.text2Offset.X+(int)(game._font.MeasureString(ws.text2).X*ws.text2Scale)+10,0));
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
                windowSlot.texture = Game1.slotTexture;
                break;
            }
            case Attachment.Type.Tower:
            {
                // game.spellAt[target.tower.MapI(), target.tower.MapJ()] = this;
                target.tower.spell = this;
                coolDown = attachment.tower.coolDownMax;
                windowSlot.texture = Game1.slotTexture;
                break;
            }
            case Attachment.Type.Child:
            {
                target.parent.children[target.index] = this;
                if(target.parent.windowSlots[target.index].textOffset.X == 64)windowSlot.texture = Game1.slotUpTexture;
                else windowSlot.texture = Game1.slotLeftTexture;
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
        return _TextureIcon[name];
    }
}
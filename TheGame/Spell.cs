
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
    public readonly bool dependentOnly;
    public static Dictionary<Name, Texture2D> TextureIcon = new();
    public static Dictionary<(int, int), Texture2D> TextureSlot = new();
    public Attachment attachment = new();
    public Spell[] children; // 子法术列表（第零项是后继法术）
    public float[] childrenManaMul;
    public List<Cast> toCastNextTick = new(); // 一个列表，存放下一刻开始时将要进行的施放
    public Name summonedEntity;
    public Window windowIcon;
    public Window windowSlot;
    public Window windowDescriptionOuter;
    public Window windowDescription;
    public Window[] windowSlots;
    public Point UIsize;
    public bool showUI = false;
    public double showLayer = 0;
    public float manaCost;
    public int price;
    public bool used = false;
    public String manaTextAppendix = "";
    public Name ImplicitName()
    {
        return name == Name.SummonProjectile ? summonedEntity : name;
    }
    public Spell(Name name, Name summonedEntity = Name.Null) : base(name)
    {
        this.summonedEntity = summonedEntity;
        price = 2*Game1.SpellPrice.GetValueOrDefault(ImplicitName());
        manaCost = Game1.SpellCost.GetValueOrDefault(ImplicitName());
        dependentOnly = name switch{
            Name.VelocityZero or 
            Name.AddSpeed or 
            Name.Add10Speed or 
            Name.DoubleSpeed or
            Name.AddXVelocity or 
            Name.AddYVelocity or 
            Name.ReduceXVelocity or 
            Name.ReduceYVelocity or 
            Name.TriggerUponDeath 
            => true, _ => false};
        windowIcon = new Window(this, WindowType.SpellIcon, IconTexture(), Color.White);
        windowSlot = new Window(this, WindowType.SpellSlot, null, Color.White);
        UIsize = new(64,64);
        windowDescriptionOuter = new Window(this, WindowType.SpellDescription, Game1.whiteTexture, Color.White, clickable: false);
        windowDescription = new Window(this, WindowType.SpellDescription, Game1.whiteTexture, Color.Black, clickable: false){
            text = name switch{
                Name.SummonProjectile => "SUMMON " + summonedEntity switch{
                    Name.Projectile1 => "A MAGICAL PROJECTILE",
                    Name.Stone => "A STONE",
                    Name.Arrow => "AN ARROW",
                    Name.Spike => "A SPIKE",
                    Name.ExplosionSquareD6 => "AN EXPLOSION",
                    _ => "SOMETHING WEIRD",
                },
                Name.VelocityZero => "REMOVE SPEED",
                Name.AddSpeed => "ADD SPEED",
                Name.Add10Speed => "ADD MUCH SPEED",
                Name.DoubleSpeed => "DOUBLE SPEED",
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
                Name.DoubleCast => "CAST TWO SPELLS",
                Name.TwiceCast => "CAST A SPELL TWICE",
                Name.CastEveryTick => "CAST A SPELL 64 TIMES",
                Name.CastEvery8Ticks => "SLOWLY CAST A SPELL 16 TIMES",
                Name.CastEvery64Ticks => "VERT SLOWLY CAST A SPELL 4 TIMES",
                Name.RandomAim => "AIM RANDOM DIRECTION",
                Name.RandomWait => "WAIT RANDOM TIME",
                Name.Aiming => "AIM AT CLOSEST ENEMY",
                Name.ScaleUp => "CAST THE SPELL WITH DOUBLE POWER",
                Name.ScaleDown => "CAST THE SPELL WITH HALF POWER",

                _ => "THIS IS A BUG"
            },
            text2 = manaCost.ToString() + "µ",
            textOffset = new(64,14),
            text2Offset = new(64,38),
        };
        switch(name)
        {
            case Name.SummonEnemy:
            case Name.SummonProjectile:
            case Name.DoubleCast:
                UIsize = Max(UIsize, new(128, 192));
                children = new Spell[2];
                windowSlots = new Window[2]
                {
                    new (this, WindowType.SpellSlots, TextureSlot[(2,0)],Color.Aqua){rank = 0, textOffset = new(64,128+14), textColor = Color.Aqua, text = name switch{
                        Name.DoubleCast => "SECOND SPELL",
                        _ => "TO CAST UPON EXPIRATION",
                    }},
                    new (this, WindowType.SpellSlots, TextureSlot[(2,1)],Color.BlueViolet){rank = 1, textOffset = new(128,64+14), textColor = Color.BlueViolet, text = name switch{
                        Name.DoubleCast => "FIRST SPELL",
                        _ => "TO CAST ON SUMMONED ENTITY",
                    }},
                };
                break;
            default:
                UIsize = Max(UIsize, new(64, 128));
                children = new Spell[1];
                windowSlots = new Window[1]{
                    new (this, WindowType.SpellSlots, TextureSlot[(1,0)],Color.Aqua){rank = 0, textOffset = new(64,64+14), textColor = Color.Aqua, text = name switch{
                        Name.TriggerUponDeath => "TO CAST UPON ENTITY EXPIRATION",
                        Name.TwiceCast => "TO CAST TWICE",
                        Name.CastEveryTick or Name.CastEvery8Ticks or Name.CastEvery64Ticks=> "TO CAST REPEATEDLY",
                        _ => "TO CAST UPON EXPIRATION",
                    }},
                };
                break;
        }
        if(windowDescription.text != null) UIsize = Max(UIsize, new(64+(int)(game._font.MeasureString(windowDescription.text).X*windowDescription.textScale)+10,0));
        if(windowDescription.text2 != null) UIsize = Max(UIsize, new(64+(int)(game._font.MeasureString(windowDescription.text2).X*windowDescription.textScale)+10,0));
        foreach(Window ws in windowSlots)
        {
            if(ws.text != null) UIsize = Max(UIsize, new(ws.textOffset.X+(int)(game._font.MeasureString(ws.text).X*ws.textScale)+10,0));
            if(ws.text2 != null) UIsize = Max(UIsize, new(ws.text2Offset.X+(int)(game._font.MeasureString(ws.text2).X*ws.text2Scale)+10,0));
        }
        
        childrenManaMul = new float[children.Length];
        for(int i=0;i<children.Length;++i)
        {
            if(Game1.ManaMul.ContainsKey((ImplicitName(),i)))
            {
                childrenManaMul[i] = Game1.ManaMul[(ImplicitName(),i)];
            }
            else childrenManaMul[i] = 1.1f;
            windowSlots[i].text2 = "µ*" + childrenManaMul[i].ToString();
            windowSlots[i].text2Offset = windowSlots[i].textOffset + new Point(0,24);
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
        UpdateManaTextAppendix();
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
                target.tower.spell = this;
                windowSlot.texture = Game1.slotTexture;
                break;
            }
            case Attachment.Type.Child:
            {
                target.parent.children[target.index] = this;
                if(target.parent.windowSlots[target.index].textOffset.X == 64)windowSlot.texture = Game1.slotUpTexture;
                else windowSlot.texture = Game1.slotLeftTexture;
                UpdateManaTextAppendix();
                break;
            }
            case Attachment.Type.Null:
            {
                break;
            }
        }
        return old;
    }
    public void UpdateManaTextAppendix()
    {
        if(attachment.type == Attachment.Type.Child) manaTextAppendix = "*" + attachment.parent.childrenManaMul[attachment.index] + attachment.parent.manaTextAppendix;
        else manaTextAppendix = "";
        foreach(Spell s in children) s?.UpdateManaTextAppendix();
    }

    public override void TickUpdate()
    {
        throw new Exception("what?");
    }
    public void TickCast()
    {
        bool oldused = used;
        foreach(Cast c in toCastNextTick)
        {
            int x = (int)MathF.Floor(c.CurrentCoordinate().X/64);
            int y = (int)MathF.Floor(c.CurrentCoordinate().Y/64);
            if(game.gamescene != GameScene.Title && game.mana != null)
            {
                if(game.mana[x,y] > manaCost * c.manaMul)
                {
                    game.NewSpellcast(this, c);
                    game.mana[x,y] -= manaCost * c.manaMul;
                    used = true;
                }
                else
                {
                    Debug.Print(game.tick.ToString() + ' ' + x + ' ' + y);
                }
            }
            else
            {
                game.NewSpellcast(this, c);
                used = true;
            }
        }
        toCastNextTick.Clear();
        if(!oldused && used) price /= 2;
    }

    private Texture2D IconTexture()
    {
        return TextureIcon.GetValueOrDefault(ImplicitName());
    }
}
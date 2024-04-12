

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheGame;
public enum WindowType {SpellIcon, SpellSlot, SpellSlots, SpellDescription, NewGame, Title, Win, GameOver, InventorySlot, ShopSlot, Tower, Block, Mana, Entity, Road, Reddoor, Bluedoor, Shop, Money, Inventory, Life};

public class Window : GameObject
{
    public bool clickable;
    public object parent;
    public Texture2D texture;
    // public Rectangle RectRender;
    // public Rectangle RectMouseCatch;
    public Color originalColor = Color.White;
    public Color color;
    public WindowType type;
    public int rank = -1;
    public int manaX, manaY;
    public float rotation = 0;
    public string text = null;
    public string text2 = null;
    public Point textOffset = new(0,0);
    public Point text2Offset = new(0,12);
    public Color textColor = Color.White;
    public Color text2Color = Color.Gray;
    public float textScale = 2;
    public float text2Scale = 2;
    public Window(object parent, WindowType type, Texture2D texture, Color originalColor, bool clickable = true)
    {
        this.parent = parent;
        this.type = type;
        this.texture = texture;
        this.originalColor = originalColor;
        color = originalColor;
        this.clickable = clickable;
    }
    public void Update()
    {
        switch(type)
        {
            case WindowType.Road:
                color = originalColor * (((Road)parent).isPath ? 0.5f : 0.0f);
                break;
            case WindowType.Mana:
                color = originalColor * (game.mana[manaX,manaY] / game.manaMax);
                break;
            case WindowType.Entity:
                rotation = ((Entity)parent).Rotation();
                color = originalColor;
                if(parent is Enemy && game.gamescene == GameScene.Build) color = Color.White * 0.2f;
                color *= (float)(0.5+0.5*((Entity)parent).health/((Entity)parent).maxhealth);
                break;
            case WindowType.SpellDescription:
                text2Color = game.manaColor;
                break;
            case WindowType.SpellIcon:
                color = ((Spell)parent).used ? originalColor : Color.Gold;
                break;
            case WindowType.Life:
                text = "LIFE  " + ((Game1)parent).life.ToString() + 'λ';
                break;
            case WindowType.Money:
                text = "MONEY "+ ((Game1)parent).money.ToString() + '$';
                break;
            case WindowType.ShopSlot:
                if(game.shop[-rank]==null)
                    text = null;
                else
                {
                    text = game.shop[-rank].price.ToString() + '$';
                    textOffset = new(32-(int)game._font.MeasureString(text).X,64);
                    textColor = game.shop[-rank].price>game.money ? Color.Gray : game.shop[-rank].used ? Color.White : Color.Gold;
                }
                break;
        }
    }
}
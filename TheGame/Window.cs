

using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheGame;
public enum WindowType {SpellIcon, SpellSlot, SpellDescription, NewGame, Title, Win, GameOver, InventorySlot, Tower, Block, Entity, Road, Reddoor, Bluedoor, Shop, Money, Inventory, Life};

public class Window
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
    public float rotation = 0;
    public string text = null;
    public Point textOffset = new(0,0);
    public Color textColor = Color.White;
    public float textScale = 2;
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
                color = Color.White * (((Road)parent).isPath ? 0.5f : 0.0f);
                break;
            case WindowType.Entity:
                color = originalColor * (float)(0.5+0.5*((Entity)parent).health/((Entity)parent).maxhealth);
                break;
            case WindowType.Life:
                text = "LIFE " + ((Game1)parent).life.ToString();
                break;
            case WindowType.Money:
                text = "MONEY " + ((Game1)parent).money.ToString();
                break;
        }
    }
}
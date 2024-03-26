

using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheGame;
public enum WindowType {SpellIcon, SpellSlot, SpellUI, NewGame, Title, Tower, Block, Entity, Road, Reddoor, Bluedoor, Shop, Money, Inventory, Life};

public class Window
{
    public bool onMap = true;
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
    public Window(object parent, WindowType type, Texture2D texture, Color originalColor, bool clickable = true, bool onMap = true)
    {
        this.parent = parent;
        this.type = type;
        this.texture = texture;
        this.originalColor = originalColor;
        color = originalColor;
        this.clickable = clickable;
        this.onMap = onMap;
    }
    public void Update()
    {
        switch(type)
        {
            case WindowType.Road:
                color = Color.White * (((Road)parent).isPath ? 0.5f : 0.2f);
                break;
            case WindowType.Entity:
                color = originalColor * (float)(0.5+0.5*((Entity)parent).health/((Entity)parent).maxhealth);
                break;
        }
    }
}
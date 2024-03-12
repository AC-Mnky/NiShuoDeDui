

using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheGame;
public enum WindowType {SpellIcon, SpellSlot, SpellUI, NewGame, Title, Tower, Block, Entity, Road};

public class Window
{
    public bool onMap = true;
    public bool clickable;
    public object parent;
    public Texture2D texture;
    // public Rectangle RectRender;
    // public Rectangle RectMouseCatch;
    public WindowType type;
    public int rank = -1;
    public Window(object parent, WindowType type, Texture2D texture, bool clickable)
    {
        this.parent = parent;
        this.type = type;
        this.texture = texture;
        this.clickable = clickable;
    }
}
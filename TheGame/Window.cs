

using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheGame;

public class Window
{
    public object parent;
    public Texture2D texture;
    public Rectangle RectRender;
    public Rectangle RectMouseCatch;
    public enum Type {SpellIcon, SpellSlot, SpellUI};
    public Type type;
    public int rank = -1;
    public Window(object parent, Type type, Texture2D texture)
    {
        this.parent = parent;
        this.type = type;
        this.texture = texture;
    }
}
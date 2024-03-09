

using Microsoft.Xna.Framework;

namespace TheGame;

abstract public class Segment
{
    public float length;
    public Segment pred = null;
    public Segment succ = null;
    public Block block;
    abstract public Vector2 CoordinateAtProgress(float progress);
}
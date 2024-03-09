using System.Diagnostics;
using Microsoft.Xna.Framework;


namespace TheGame;

public class SegmentLine : Segment
{
    public Vector2 relativeStartPoint;
    public Vector2 relativeEndPoint;
    public override Vector2 CoordinateAtProgress(float progress)
    {
        Debug.Assert(progress <= length);
        float r = progress / length;
        return block.Coordinate() + relativeStartPoint * (1-r) + relativeEndPoint * r;
    }
}
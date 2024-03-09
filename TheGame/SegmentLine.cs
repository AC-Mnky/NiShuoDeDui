using System.Diagnostics;
using Microsoft.Xna.Framework;


namespace TheGame;

public class SegmentLine : Segment
{
    public Vector2 relativeStartPoint;
    public Vector2 relativeEndPoint;
    public override Vector2 CoordinateAtProgress(float progress)
    {
        float r = progress / length;
        Debug.Assert(0 <= r);
        Debug.Assert(r <= 1);
        if(!forward) r = 1-r;
        return block.Coordinate() + relativeStartPoint * (1-r) + relativeEndPoint * r;
    }

    public SegmentLine(Block block, Vector2 relativeStartPoint, Vector2 relativeEndPoint)
    {
        this.block = block;
        this.relativeStartPoint = relativeStartPoint;
        this.relativeEndPoint = relativeEndPoint;
        length = Vector2.Distance(relativeStartPoint, relativeEndPoint);
    }
}
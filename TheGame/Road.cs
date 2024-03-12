using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheGame;

public class Road
{
    public static Dictionary<RoadName, Texture2D> Texture = new();
    public Block block;
    public RoadName name;
    public int door1;
    public int door2;
    public Segment[] segments;
    public bool isPath = false;
    public Window window;
    public static Vector2 CoordDoor(int door)
    {
        return door switch
        {
            0 => new(96, 0),
            1 => new(224, 0),
            2 => new(0, 96),
            3 => new(0, 224),
            4 => new(96, 320),
            5 => new(224, 320),
            6 => new(320, 96),
            7 => new(320, 224),
            _ => throw new System.NotImplementedException(),
        };
    }
    public Road(Block block, RoadName name)
    {
        this.block = block;
        this.name = name;
        window = new Window(this, WindowType.Road, Texture[name], false);
        switch(name)
        {
            case RoadName.A04:
            {
                door1 = 0;
                door2 = 4;
                segments = new Segment[1]{new SegmentLine(block, CoordDoor(0), CoordDoor(4))};
                break;
            }
            case RoadName.A15:
            {
                door1 = 1;
                door2 = 5;
                segments = new Segment[1]{new SegmentLine(block, CoordDoor(1), CoordDoor(5))};
                break;
            }
            case RoadName.A26:
            {
                door1 = 2;
                door2 = 6;
                segments = new Segment[1]{new SegmentLine(block, CoordDoor(2), CoordDoor(6))};
                break;
            }
            case RoadName.A37:
            {
                door1 = 3;
                door2 = 7;
                segments = new Segment[1]{new SegmentLine(block, CoordDoor(3), CoordDoor(7))};
                break;
            }
            case RoadName.B02:
            {
                door1 = 0;
                door2 = 2;
                segments = new Segment[2]{new SegmentLine(block, CoordDoor(0), new(96,96)), new SegmentLine(block, new(96,96), CoordDoor(2))};
                break;
            }
            case RoadName.B16:
            {
                door1 = 1;
                door2 = 6;
                segments = new Segment[2]{new SegmentLine(block, CoordDoor(1), new(224,96)), new SegmentLine(block, new(224,96), CoordDoor(6))};
                break;
            }
            case RoadName.B34:
            {
                door1 = 3;
                door2 = 4;
                segments = new Segment[2]{new SegmentLine(block, CoordDoor(3), new(96,224)), new SegmentLine(block, new(96,224), CoordDoor(4))};
                break;
            }
            case RoadName.B57:
            {
                door1 = 5;
                door2 = 7;
                segments = new Segment[2]{new SegmentLine(block, CoordDoor(5), new(224,224)), new SegmentLine(block, new(224,224), CoordDoor(7))};
                break;
            }
        }

    }
}


using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheGame;

public enum BlockName {A, B};
public enum RoadName {A04, A15, A26, A37, B02, B16, B34, B57};

public class Block
{
    public static Dictionary<BlockName, Texture2D> Texture = new();
    public BlockName name;
    public const int numX = 5;
    public const int numY = 5;
    public const int Dgrid = 5;
    public int x;
    public int y;
    public Road[] road;
    public Road[] roadOfDoor = new Road[8];
    public int[] otherDoor = new int[8];
    public Tower[] tower;
    public Vector2 Coordinate()
    {
        return new Vector2(x*Dgrid*64,y*Dgrid*64);
    }
    public Block(BlockName name, int x, int y)
    {
        this.name = name;
        this.x = x;
        this.y = y;
        switch(name)
        {
            case BlockName.A:
            {
                road = new Road[4]{new(this, RoadName.A04),
                    new(this, RoadName.A15),
                    new(this, RoadName.A26),
                    new(this, RoadName.A37)
                    };
                tower = new Tower[1]{new(this,2,2,15)};
                break;
            }
            case BlockName.B:
            {
                road = new Road[4]{new(this, RoadName.B02),
                    new(this, RoadName.B16),
                    new(this, RoadName.B34),
                    new(this, RoadName.B57)};
                tower = new Tower[1]{new(this,2,2,15)};
                break;
            }
        }
        foreach(Road r in road)
        {
            roadOfDoor[r.door1] = r;
            roadOfDoor[r.door2] = r;
            otherDoor[r.door1] = r.door2;
            otherDoor[r.door2] = r.door1;
        }
    }
}
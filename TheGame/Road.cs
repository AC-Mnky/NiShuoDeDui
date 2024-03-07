using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.Xna.Framework.Graphics;

namespace TheGame;

public class Road
{
    public static Dictionary<RoadName, Texture2D> Texture = new();
    public float length;
    public Road pred = null;
    public Road succ = null;
    public Block block;
    public RoadName name;
    public int door1;
    public int door2;
    public bool isPath = false;
    public Road(Block block, RoadName name)
    {
        this.block = block;
        this.name = name;
        switch(name)
        {
            case RoadName.A04:
            {
                door1 = 0;
                door2 = 4;
                break;
            }
            case RoadName.A15:
            {
                door1 = 1;
                door2 = 5;
                break;
            }
            case RoadName.A26:
            {
                door1 = 2;
                door2 = 6;
                break;
            }
            case RoadName.A37:
            {
                door1 = 3;
                door2 = 7;
                break;
            }
            case RoadName.B02:
            {
                door1 = 0;
                door2 = 2;
                break;
            }
            case RoadName.B16:
            {
                door1 = 1;
                door2 = 6;
                break;
            }
            case RoadName.B34:
            {
                door1 = 3;
                door2 = 4;
                break;
            }
            case RoadName.B57:
            {
                door1 = 5;
                door2 = 7;
                break;
            }
        }

    }
}

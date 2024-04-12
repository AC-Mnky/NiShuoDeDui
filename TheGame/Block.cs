
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheGame;

public enum BlockName {A, B};
public enum RoadName {A04, A15, A26, A37, B02, B16, B34, B57};

public class Block
{
    public bool title = false;
    public static Dictionary<BlockName, Texture2D> Texture = new();
    public static RanDict<int> RandomTowerNumber = new(){
        {1,20},{2,10},{3,5},{4,1}
    };
    const int STANDARD = 8;
    public static Dictionary<BlockName, RanDict<(int,int,int)>> RandomTowerLocation = new(){
        {BlockName.A,new(){{(0,0,STANDARD),1},{(0,2,STANDARD),1},{(0,4,STANDARD),1},{(2,0,STANDARD),1},{(2,2,STANDARD),1},{(2,4,STANDARD),1},{(4,0,STANDARD),1},{(4,2,STANDARD),1},{(4,4,STANDARD),1}}},
        {BlockName.B,new(){{(0,0,STANDARD),1},{(0,2,STANDARD),1},{(0,4,STANDARD),1},{(2,0,STANDARD),1},{(2,2,STANDARD),1},{(2,4,STANDARD),1},{(4,0,STANDARD),1},{(4,2,STANDARD),1},{(4,4,STANDARD),1}}},
    };
    public BlockName name;
    public static int numX;
    public static int numY;
    public const int Dgrid = 5;
    public int x;
    public int y;
    public Road[] road;
    public Road[] roadOfDoor = new Road[8];
    public int[] otherDoor = new int[8];
    public Tower[] tower;
    public Window window;
    public Vector2 titleCoordinate;
    public Vector2 Coordinate()
    {
        if(title) return titleCoordinate;
        return new Vector2(x*Dgrid*64,y*Dgrid*64);
    }
    public Block(bool title, Vector2 Coordinate)
    {
        Debug.Assert(title);
        this.title = true;
        titleCoordinate = Coordinate;
        tower = new Tower[5];
        tower[0] = new(this, 2, 0, STANDARD);
        tower[1] = new(this, 3, 0, STANDARD);
        tower[2] = new(this, 4, 0, STANDARD);
        tower[3] = new(this, 0, 2, STANDARD);
        tower[4] = new(this, 0, 4, STANDARD);
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
                break;
            }
            case BlockName.B:
            {
                road = new Road[4]{new(this, RoadName.B02),
                    new(this, RoadName.B16),
                    new(this, RoadName.B34),
                    new(this, RoadName.B57)};
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
    public void Initialize()
    {
        tower = new Tower[RandomTowerNumber.Next()];
        RandomTowerLocation[name].BeginNonRepeating();
        for(int i=0;i<tower.Length;++i)
        {
            var loc = RandomTowerLocation[name].Next();
            tower[i] = new(this, loc.Item1, loc.Item2, loc.Item3);
        }
        RandomTowerLocation[name].EndNonRepeating();
        
        window = new(this, WindowType.Block, Texture[name], Color.White * 0.0f, false);
    }
}
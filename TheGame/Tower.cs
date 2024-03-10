using Microsoft.Xna.Framework;

namespace TheGame;

public class Tower
{
    public int relativeI;
    public int relativeJ;
    public int coolDownMax;
    public Block block;
    public Window window;
    public Spell spell = null;
    public Vector2 Coordinate()
    {
        return block.Coordinate() + new Vector2(relativeI*64f+32f,relativeJ*64f+32f);
    }
    public int MapI() {return block.x*Block.Dgrid+relativeI;}
    public int MapJ() {return block.y*Block.Dgrid+relativeJ;}
    public Tower(Block block, int relativeI, int relativeJ, int coolDownMax)
    {
        this.block = block;
        this.relativeI = relativeI;
        this.relativeJ = relativeJ;
        this.coolDownMax = coolDownMax;
        window = new Window(this, WindowType.Tower, Game1.towerGUI, true);
    }
}
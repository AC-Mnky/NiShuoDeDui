namespace TheGame;


public class Attachment
{
    public enum Type {Inventory, Tower, Child, Null}; // 法术是在台面上（未使用），还是在图上（可以直接触发），还是某个法术的子法术
    public Type type = Type.Null;
    public int inventoryIndex = -1; // 如果在台面上，它的编号
    // public int mapI, mapJ; // 如果在地图上，它的坐标
    // public long coolDownMax = -1;
    public Tower tower;
    public Spell parent = null; // 如果是子法术，那么它挂在哪个法术身上
    public int rank = -1; // 如果是子法术，那么是第几个

    public Attachment(int inventoryIndex)
    {
        type = Type.Inventory;
        this.inventoryIndex = inventoryIndex;
    }
    public Attachment(Tower tower)
    {
        type = Type.Tower;
        this.tower = tower;
        // this.mapI = mapI;
        // this.mapJ = mapJ;
        // this.coolDownMax = coolDownMax;
    }
    public Attachment(Spell parent, int rank)
    {
        type = Type.Child;
        this.parent = parent;
        this.rank = rank;
    }
    public Attachment()
    {
        type = Type.Null;
    }
}

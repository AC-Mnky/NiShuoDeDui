// 要想理解这个源码的架构，最直观的方式是看看有哪些class，它们有哪些作用。
// 整个游戏是一个Game1对象。游戏中有很多实体Entity，包含敌人Enemy和弹射物Projectile。
// 另外，游戏中有一些法术Spell，但这些是静态的，并不是被施放出的法术。
// 被施放出的法术是Spellcast，它们很快出现和消失。一个Spell每次被施放都会产生一个Spellcast。
// Game1有三个字典entities[]，spells[]，spellcasts[]，存储着游戏里的所有东西。它们十分关键。
// 创造新东西使用NewEnemy()这几个方法，如果要删除的话只要将它标记为alive=false，就会在一个周期内被删除。

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace TheGame;

public enum GameScene {Title, Perk, Build, Battle, Win, Lose, Options, Loading}
public enum GameStatus {Paused, Running};
public class Game1 : Game
{
    public Random rand = new(RandomNumberGenerator.GetInt32(2147483647));
    private double _time;
    private GraphicsDeviceManager _graphics;
    public SpriteFont _font;
    private SpriteBatch _spriteBatch;
    private Effect _mapShader;
    // private Effect _guiShader;
    private Matrix _view = Matrix.Identity;
    private Matrix _zeroview = Matrix.Identity;
    private bool _zoomEnabled = false;
    private Matrix projection = Matrix.CreateOrthographicOffCenter(0, 800, 600, 0, 0, 1);
    public Vector2 MouseCoor = new();
    public Vector2 LeftTop = new();
    public Vector2 RightBottom = new();
    public static float xPeriod, yPeriod;
    public int MouseI = 0, MouseJ = 0;
    int width;
    int height;
    public long tick; // 游戏从开始经过的刻数
    // private long thingCount; // 游戏从开始产生的Entity, Spell, Spellcast总数
    private double timeBank;
    private GameStatus gamestatus; // 是不是暂停
    private GameScene gamescene;
    public int life;
    public int money;
    private int tps; // 每秒多少刻（控制倍速，60刻是一倍速）
    // private LinkedList<Entity> entities;
    private LinkedList<Enemy> enemy;
    private LinkedList<Entity> neutral;
    private LinkedList<Projectile> projectile;
    private LinkedList<Spell> spell; // 十分关键的字典，其中每个spell都有一个唯一的id
    private LinkedList<Spellcast> spellcast; // 十分关键的字典，其中每个spellcast都有一个唯一的id
    private Stack enemyStack;
    private int enemyRate;
    private Block[,] blocks;
    public Segment Reddoor, Bluedoor;
    public Window ReddoorWindow, BluedoorWindow;
    public Vector2 ReddoorCoor, BluedoorCoor;
    public int ReddoorIndex, BluedoorIndex;
    private Window mouseOn;
    private Spell holdingSpell = null;
    private Attachment oldAtt = null;
    public Spell[] inventory;
    public Spell[] shop;
    public Window[] inventorySlot;
    public Window[] shopSlot;
    private Window title, newGame, win, gameover, shopWindow, moneyWindow, inventoryWindow, lifeWindow;
    private int shopWidth, inventoryWidth;
    private bool shopOpen, inventoryOpen, inventoryAvailable;
    public int stage, wave;
    private Spell summonenemy1, summonenemyEasy, summonenemyFast, summonenemyVeryFast;
    public static Texture2D slotTexture, slotLeftTexture, slotUpTexture;
    public static Texture2D doorTexture;
    public static Texture2D defaultTexture;
    public static Texture2D whiteTexture;
    public static Texture2D transparentTexture;

    private bool _predraw = false;
    private bool _hasdrawn;
    private bool _onMap;
    private bool _clearMapFlag = false;



    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            GraphicsProfile = GraphicsProfile.HiDef,
            PreferredBackBufferWidth = 1440,
            PreferredBackBufferHeight = 810,
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        gamescene = GameScene.Title;
    }

    protected override void Initialize()
    {
        // ToggleBorderless(); // 先全屏 // 但是全屏不方便debug所以先关掉了

        // for(int i=0;i<maxI;++i) for(int j=0;j<maxJ;++j)
        // {
        //     isLight[i,j] = rand.Next(2)>0;
        //     // (i+j)%2==0;
        // }

        base.Initialize();
    }

    protected override void LoadContent() // 加载材质
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        #region font
        
        var characters = new List<char>(){' ','0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','？'};

        var glyphBounds = new List<Rectangle>();
        glyphBounds.Add(new());
        for(int i=0;i<10;++i) glyphBounds.Add(new(5*i,9,4,7));
        for(int i=0;i<26;++i) glyphBounds.Add(new(9*i,0,7,7));
        glyphBounds.Add(new(225,27,7,7));
        
        var cropping = new List<Rectangle>();
        cropping.Add(new());
        for(int i=0;i<10;++i) cropping.Add(new(0,0,4,7));
        for(int i=0;i<26;++i) cropping.Add(new(0,0,7,7));
        cropping.Add(new(0,0,7,7));
        
        var kerning = new List<Vector3>();
        kerning.Add(new(0,7,0));
        for(int i=0;i<10;++i) kerning.Add(new(0.5f,4,0.5f));
        for(int i=0;i<26;++i) kerning.Add(new(1,7,1));
        kerning.Add(new(1,7,1));
        
        _font = new SpriteFont(Content.Load<Texture2D>("font"), glyphBounds, cropping, characters, 12, 0, kerning, '？');
        
        #endregion

        #region random shit
        defaultTexture = Content.Load<Texture2D>("default");
        whiteTexture = Content.Load<Texture2D>("white");
        transparentTexture = Content.Load<Texture2D>("transparent");
        
        title = new Window(this, WindowType.Title, transparentTexture, Color.Transparent, clickable: false){
            text = "THE GAME IS A TOWER DEFENSE GAME NAMED THE GAME IS A TOWER DEFENSE GAME NAMED THE GAME IS A TOWER DEFENSE GAME",
            textScale = 4
        };
        newGame = new Window(this, WindowType.NewGame, transparentTexture, Color.Transparent){
            text = "NEW GAME",
            textScale = 2
        };
        win = new Window(this, WindowType.Win, transparentTexture, Color.Transparent, clickable: false){
            text = "YOU WIN",
            textScale = 4,
            textColor = Color.Black
        };
        gameover = new Window(this, WindowType.GameOver, transparentTexture, Color.Transparent, clickable: false){
            text = "GAME OVER",
            textScale = 4
        };
        shopWindow = new Window(this, WindowType.Shop, whiteTexture, Color.BlueViolet, clickable: false);
        inventoryWindow = new Window(this, WindowType.Inventory, whiteTexture, Color.Blue, clickable: false);
        moneyWindow = new Window(this, WindowType.Money, whiteTexture, Color.BlueViolet, clickable: true){
            textScale = 2,
            textOffset = new(15,15)
        };
        lifeWindow = new Window(this, WindowType.Life, whiteTexture, Color.Blue, clickable: true){
            textScale = 2,
            textOffset = new(15,15)
        };
        slotTexture = Content.Load<Texture2D>("slot");
        slotUpTexture = Content.Load<Texture2D>("slotUp");
        slotLeftTexture = Content.Load<Texture2D>("slotLeft");
        doorTexture = Content.Load<Texture2D>("door");
        // _lightgrey = Content.Load<Texture2D>("lightgrey");
        // _darkgrey = Content.Load<Texture2D>("darkgrey");

        #endregion

        #region texture
        Block.Texture[BlockName.A] = Content.Load<Texture2D>("blockdefault");
        Block.Texture[BlockName.B] = Content.Load<Texture2D>("blockdefault");

        Road.Texture[RoadName.A04] = Content.Load<Texture2D>("blockA04");
        Road.Texture[RoadName.A15] = Content.Load<Texture2D>("blockA15");
        Road.Texture[RoadName.A26] = Content.Load<Texture2D>("blockA26");
        Road.Texture[RoadName.A37] = Content.Load<Texture2D>("blockA37");
        Road.Texture[RoadName.B02] = Content.Load<Texture2D>("blockB02");
        Road.Texture[RoadName.B16] = Content.Load<Texture2D>("blockB16");
        Road.Texture[RoadName.B34] = Content.Load<Texture2D>("blockB34");
        Road.Texture[RoadName.B57] = Content.Load<Texture2D>("blockB57");

        Entity.Texture[Name.Enemy1] = Content.Load<Texture2D>("enemy1");
        Entity.Texture[Name.EnemyEasy] = Content.Load<Texture2D>("enemy1");
        Entity.Texture[Name.EnemyFast] = Content.Load<Texture2D>("enemyfast");
        Entity.Texture[Name.EnemyVeryFast] = Content.Load<Texture2D>("enemyveryfast");
        Entity.Texture[Name.Projectile1] = Content.Load<Texture2D>("projectile1");
        Entity.Texture[Name.SquareD6] = whiteTexture;
        Entity.Texture[Name.ExplosionSquareD6] = whiteTexture;

        Spell.TextureIcon[Name.SummonEnemy] = Content.Load<Texture2D>("SummonEnemy1icon");
        Spell.TextureIcon[Name.SummonProjectile] = Content.Load<Texture2D>("SummonProjectile1icon");
        Spell.TextureIcon[Name.VelocityZero] = Content.Load<Texture2D>("velocityzeroicon");
        Spell.TextureIcon[Name.AddSpeed] = Content.Load<Texture2D>("addspeedicon");
        Spell.TextureIcon[Name.Add10Speed] = Content.Load<Texture2D>("add5speedicon");
        Spell.TextureIcon[Name.AddXVelocity] = Content.Load<Texture2D>("addxvelocityicon");
        Spell.TextureIcon[Name.AddYVelocity] = Content.Load<Texture2D>("addyvelocityicon");
        Spell.TextureIcon[Name.ReduceXVelocity] = Content.Load<Texture2D>("reducexvelocityicon");
        Spell.TextureIcon[Name.ReduceYVelocity] = Content.Load<Texture2D>("reduceyvelocityicon");
        Spell.TextureIcon[Name.TriggerUponDeath] = Content.Load<Texture2D>("triggerupondeathicon");
        Spell.TextureIcon[Name.AimClosestInSquareD6] = Content.Load<Texture2D>("aimclosestinsquared6icon");
        Spell.TextureIcon[Name.AimMouse] = Content.Load<Texture2D>("aimmouseicon");
        Spell.TextureIcon[Name.AimUp] = Content.Load<Texture2D>("aimupicon");
        Spell.TextureIcon[Name.AimDown] = Content.Load<Texture2D>("aimdownicon");
        Spell.TextureIcon[Name.AimLeft] = Content.Load<Texture2D>("aimlefticon");
        Spell.TextureIcon[Name.AimRight] = Content.Load<Texture2D>("aimrighticon");
        Spell.TextureIcon[Name.AimBack] = Content.Load<Texture2D>("aimbackicon");
        Spell.TextureIcon[Name.Wait60Ticks] = Content.Load<Texture2D>("wait60ticksicon");

        Spell.TextureSlot[(2,0)] = Content.Load<Texture2D>("spellgui2slot0");
        Spell.TextureSlot[(2,1)] = Content.Load<Texture2D>("spellgui2slot1");
        Spell.TextureSlot[(1,0)] = Content.Load<Texture2D>("spellgui1slot0");
        #endregion

        _mapShader = Content.Load<Effect>("map-shader");
    }



    public Enemy NewEnemy(Name name, Segment segment, float progress)
    {
        return enemy.AddLast(new Enemy(this, name, segment, progress)).Value;
    }
    public Projectile NewProjectile(Name name, Vector2 coordinate, Vector2 velocity)
    {
        return projectile.AddLast(new Projectile(this, name, coordinate, velocity)).Value;
    }
    public Spell NewSpell(Name name, Name summonedEntity = Name.Null)
    {
        return spell.AddLast(new Spell(this, name, summonedEntity)).Value;
    }
    public Spellcast NewSpellcast(Spell spell, Cast cast)
    {
        return spellcast.AddLast(new Spellcast(this, spell, cast)).Value;
    }

    public IEnumerable<Entity> entities()
    {
        foreach(Entity e in enemy) yield return e;
        foreach(Entity e in neutral) yield return e;
        foreach(Entity e in projectile) yield return e;
    }
    public IEnumerable<Entity> Collisions(Entity e) // 简单的碰撞判定算法。之后可能会出现圆形的东西，从而需要修改。另外以后算法上可能会需要优化。
    {
        var E = e.Hitbox();
        var Es = new RectangleF[9];
        for(int i=0;i<9;++i)
        {
            Es[i] = E;
            Es[i].Offset((i/3-1)*xPeriod, (i%3-1)*yPeriod);
        }
        if(e is not Enemy) foreach(Entity f in enemy)
        {
            var F = f.Hitbox();
            for(int i=0;i<9;++i) if(F.IntersectsWith(Es[i]))
            {
                yield return f;
                break;
            }
        }
        foreach(Entity f in neutral) if (f!=e)
        {
            var F = f.Hitbox();
            for(int i=0;i<9;++i) if(F.IntersectsWith(Es[i]))
            {
                yield return f;
                break;
            }
        }
        if(e is not Projectile) foreach(Entity f in projectile)
        {
            var F = f.Hitbox();
            for(int i=0;i<9;++i) if(F.IntersectsWith(Es[i]))
            {
                yield return f;
                break;
            }
        }
    }

    public Block Blocks(int x, int y)
    {
        return blocks[x-Block.numX*(int)MathF.Floor(x / (float)Block.numX), y-Block.numY*(int)MathF.Floor(y / (float)Block.numY)];
    }
    public Block Neighbour(Block b, int door)
    {
        return door switch
        {
            0 or 1 => Blocks(b.x, b.y-1),
            2 or 3 => Blocks(b.x-1, b.y),
            4 or 5 => Blocks(b.x, b.y+1),
            6 or 7 => Blocks(b.x+1, b.y),
            _ => null,
        };

    }
    private int _pathRoadNum = 0;
    public Segment RefindPath(Block block, int doorout)
    {
        _pathRoadNum = 0;
        for(int x=0;x<Block.numX;++x) for(int y=0;y<Block.numY;++y) Debug.Assert(blocks[x,y].x == x && blocks[x,y].y == y);
        foreach(Block bi in blocks) foreach(Road ri in bi.road) ri.isPath = false;
        Block b = block;
        int d = doorout;
        Segment first = null;
        Segment last = null;
        do
        {
            b = Neighbour(b, d);
            Road r = b.roadOfDoor[(d+4)%8];
            r.isPath = true;
            if((d+4)%8 == r.door1)
            {
                if(last == null) first = r.segments[0];
                else last.succ = r.segments[0];
                for(int i=0;i<r.segments.Length;++i)
                {
                    r.segments[i].forward = true;
                    if(i<r.segments.Length-1) r.segments[i].succ = r.segments[i+1];
                }
                last = r.segments.Last();
            }
            else
            {
                if(last == null) first = r.segments.Last();
                else last.succ = r.segments.Last();
                for(int i=0;i<r.segments.Length;++i)
                {
                    r.segments[i].forward = false;
                    if(i>0) r.segments[i].succ = r.segments[i-1];
                }
                last = r.segments[0];

            }
            d = b.otherDoor[(d+4)%8];
            ++_pathRoadNum;
            // Debug.Print(new Vector2(b.x,b.y).ToString());
            // Debug.Print(s.doorin.ToString());
        } while(b != block || d != doorout);
        last.succ = first;
        // Debug.Print(_pathRoadNum.ToString());
        return last;
    }
    public void Penetrated(int i)
    {
        life -= i;
        // Debug.Print("life: " + life.ToString());
    }
    public void GenerateEnemyStack()
    {
        summonenemy1 = NewSpell(Name.SummonEnemy, Name.Enemy1);
        summonenemyEasy = NewSpell(Name.SummonEnemy, Name.EnemyEasy);
        summonenemyFast = NewSpell(Name.SummonEnemy, Name.EnemyFast);
        summonenemyVeryFast = NewSpell(Name.SummonEnemy, Name.EnemyVeryFast);
        for(int i=0;i<20;++i) enemyStack.Push(summonenemyEasy);
    }



    protected void TickUpdate() // 游戏内每刻更新（暂停时不会调用，倍速时会更频繁调用），这里主要负责核心内部机制的计算
    {
        switch(gamescene)
        {
            case GameScene.Build:
                break;
            case GameScene.Battle:
                if(tick == 0) GenerateEnemyStack();
                if(rand.Next(enemyRate) == 0 && enemyStack.Count > 0) NewSpellcast((Spell)enemyStack.Pop(), new(new Vector2()));
                break;
            default:
                return;
        }
        
        // 修改这里的顺序前务必仔细思考，否则可能会出现意想不到的情况
        foreach(Spell s in spell)
            s.TickCast(); // 待施放的法术进行施放
        foreach(Spellcast sc in spellcast)
            sc.TickUpdate(); // 被施法术更新
        #region remove entities // 移除被标记为死亡的实体
        LinkedListNode<Enemy> m1;
        for(var n = enemy.First;n!=null;n=m1)
        {
            m1 = n.Next;
            if(!n.Value.alive) enemy.Remove(n);
        }
        LinkedListNode<Entity> m3;
        for(var n = neutral.First;n!=null;n=m3)
        {
            m3 = n.Next;
            if(!n.Value.alive) neutral.Remove(n);
        }
        LinkedListNode<Projectile> m4;
        for(var n = projectile.First;n!=null;n=m4)
        {
            m4 = n.Next;
            if(!n.Value.alive) projectile.Remove(n);
        }
        #endregion
        foreach(Entity e in entities())
            e.TickUpdateCoordinate(); // 实体移动
        foreach(Entity e in entities())
            e.TickUpdate(); // 实体更新（期间不应该移动！）
        LinkedListNode<Spellcast> m2;
        for(var n = spellcast.First;n!=null;n=m2)
        {
            m2 = n.Next;
            if(!n.Value.alive) spellcast.Remove(n); // 移除被标记为死亡的Spellcast
        }
        foreach(Spell s in spell)
            s.TickUpdate(); // 法术更新（其实只有地图上的法术会发生变化）
        if (gamescene == GameScene.Battle)
        {
            if (life <= 0) BattleEnd(false);
            if (enemyStack.Count == 0 && enemy.Count == 0) BattleEnd(true);
        }
        ++tick;

        // Debug.Print(tick.ToString());
        // Debug.Print(spellcasts.Count.ToString());
        // Debug.Print(entities.Count.ToString());
    }



    private void InitMap()
    {
        ClearMap();
        _view = Matrix.Identity;
        #region blocks
        blocks = new Block[Block.numX,Block.numY];
        do{
            for(int x=0;x<Block.numX;++x) for(int y=0;y<Block.numY;++y)
                blocks[x,y] = new(RandomBlockName.Next(), x,y);
            BluedoorIndex = rand.Next(8);
            Bluedoor = RefindPath(Blocks(rand.Next(Block.numX), rand.Next(Block.numY)),BluedoorIndex);
        } while(_pathRoadNum != 35);
        // } while(_pathRoadNum < 30 || _pathRoadNum > 40);
        foreach(Block b in blocks) b.Initialize();

        Reddoor = Bluedoor.succ;
        ReddoorIndex = (BluedoorIndex + 4) % 8;
        BluedoorWindow = new Window(this, WindowType.Bluedoor, doorTexture, Color.Blue, false)
        {
            rotation = BluedoorIndex switch
            {
                0 or 1 => MathF.PI/2,
                2 or 3 => 0,
                4 or 5 => -MathF.PI/2,
                6 or 7 => MathF.PI,
                _ => throw new ArgumentOutOfRangeException()              
            }
        };
        ReddoorWindow = new Window(this, WindowType.Reddoor, doorTexture, Color.Red, false)
        {
            rotation = ReddoorIndex switch
            {
                0 or 1 => MathF.PI/2,
                2 or 3 => 0,
                4 or 5 => -MathF.PI/2,
                6 or 7 => MathF.PI,
                _ => throw new ArgumentOutOfRangeException()              
            }
        };
        BluedoorCoor = Bluedoor.block.Coordinate() + (BluedoorIndex switch{0 => new(128,0), 1 => new(256,0), 2 => new(0,64), 3 => new(0,192), 4 => new(64,320), 5 => new(192,320), 6 => new(320,128), 7 => new(320,256), _ => throw new ArgumentOutOfRangeException()});
        ReddoorCoor = Reddoor.block.Coordinate() + (ReddoorIndex switch{0 => new(128,0), 1 => new(256,0), 2 => new(0,64), 3 => new(0,192), 4 => new(64,320), 5 => new(192,320), 6 => new(320,128), 7 => new(320,256), _ => throw new ArgumentOutOfRangeException()});
        


        #endregion
        // foreach(Block b in blocks) foreach(Tower t in b.tower)
        // {
        //     RandomNewSpell().ReAttach(new(t));
        // }
    }
    private void InitInventory()
    {
        inventory = new Spell[4];
        inventorySlot = new Window[inventory.Length];
        for(int i=1;i<inventory.Length;++i)
        {
            inventorySlot[i] = new Window(this, WindowType.InventorySlot, slotTexture, Color.White){
                rank = i,
            };
        }
    }
    private void InitShop()
    {
        shop = new Spell[25];
        shopSlot = new Window[shop.Length];
        for(int i=1;i<shop.Length;++i)
        {
            shopSlot[i] = new Window(this, WindowType.ShopSlot, slotTexture, Color.Gold){
                rank = -i,
            };
        }
        for(int i=1;i<shop.Length;++i)
        {
            RandomNewSpell().ReAttach(new(-i));
        }
    }
    private static RanDict<BlockName> RandomBlockName = new(){{BlockName.A,2}, {BlockName.B,1}};
    private static RanDict<Name> RandomProjectileName = new(){
        {Name.Projectile1, 1},
        {Name.Stone, 1},
        {Name.Arrow, 1},
        {Name.Spike, 1},
        {Name.ExplosionSquareD6, 0.5},
    };
    private static RanDict<Name> RandomSpellName = new(){
        {Name.SummonProjectile, 6},
        {Name.Add10Speed, 2},
        {Name.AddSpeed, 1},
        {Name.DoubleSpeed, 1},
        {Name.AddXVelocity, 0.25},
        {Name.AddYVelocity, 0.25},
        {Name.ReduceXVelocity, 0.25},
        {Name.ReduceYVelocity, 0.25},
        {Name.AimClosestInSquareD6, 1},
        {Name.TriggerUponDeath, 1},
        {Name.VelocityZero, 0.5},
        {Name.Wait60Ticks, 1},
        {Name.AimMouse, 1},
        {Name.AimBack, 0.5},
        {Name.AimLeft, 0.25},
        {Name.AimRight, 0.25},
        {Name.AimUp, 0.25},
        {Name.AimDown, 0.25},
        {Name.DoubleCast, 1},
        {Name.TwiceCast, 1},
        {Name.CastEveryTick, 1},
        {Name.CastEvery8Ticks, 1},
        {Name.CastEvery64Ticks, 1},
    };
    public static Dictionary<Name, int> SpellPrice = new(){
        {Name.Projectile1, 1},
        {Name.Stone, 3},
        {Name.Arrow, 2},
        {Name.Spike, 2},
        {Name.ExplosionSquareD6, 10},
        {Name.Add10Speed, 2},
        {Name.AddSpeed, 1},
        {Name.DoubleSpeed, 2},
        {Name.AddXVelocity, 1},
        {Name.AddYVelocity, 1},
        {Name.ReduceXVelocity, 1},
        {Name.ReduceYVelocity, 1},
        {Name.AimClosestInSquareD6, 2},
        {Name.TriggerUponDeath, 2},
        {Name.Wait60Ticks, 2},
        {Name.VelocityZero, 1},
        {Name.AimMouse, 2},
        {Name.AimBack, 1},
        {Name.AimLeft, 1},
        {Name.AimRight, 1},
        {Name.AimUp, 1},
        {Name.AimDown, 1},
        {Name.DoubleCast, 1},
        {Name.TwiceCast, 5},
        {Name.CastEveryTick, 10},
        {Name.CastEvery8Ticks, 8},
        {Name.CastEvery64Ticks, 5},
    };

    private Spell RandomNewSpell()
    {
        Spell spell = NewSpell(RandomSpellName.Next(), RandomProjectileName.Next());
        return spell;
    }


    private void ClearMap()
    {
        _clearMapFlag = true;
        
        tick = 0;

        enemy = new();
        neutral = new();
        projectile = new();

        spellcast = new();
        enemyStack = new();

        if(blocks != null) foreach(Block b in blocks) foreach(Tower t in b.tower) MoveToInventory(t.spell);

        _clearMapFlag = false;
    }
    private void RefreshMap()
    {
        tick = 0;
        timeBank = 0;

        enemy = new();
        neutral = new();
        projectile = new();

        spellcast = new();
        enemyStack = new();
    }
    private void BattleEnd(bool win)
    {
        if(win)
        {
            ++wave;
            if(wave>5)
            {
                wave = 1;
                ++stage;
                if(stage>3)
                {
                    Ending();
                    return;
                }
                StageBegin();
            }
            WaveBegin();
        }
        else
        {
            gamescene = GameScene.Lose;
        }
    }
    private void BattleBegin()
    {
        gamescene = GameScene.Battle;
        inventoryAvailable = false;
        if(inventoryOpen) ToggleInventory();
        if(shopOpen) ToggleShop();
        foreach(Block b in blocks) foreach(Tower t in b.tower) if(t.spell != null)
        {
            t.spell.showUI = false;
            t.spell.showLayer = 0;
        }
        RefreshMap();
    }
    private void WaveBegin()
    {
        gamescene = GameScene.Build;

        InitShop();

        shopWidth = 0;
        if(!shopOpen) ToggleShop();
        inventoryWidth = 0;
        if(!inventoryOpen) ToggleInventory();
        inventoryAvailable = true;

        enemyRate = 120 - 10 * (wave-1) - 20 * (stage-1);

        RefreshMap();
    }
    private void StageBegin()
    {
        InitMap();
    }
    private void GameBegin()
    {
        stage = 1;
        wave = 1;
        spell = new();
        StageBegin();
        WaveBegin();
        InitInventory();
        gamestatus = GameStatus.Running;
        tps = 60;
        life = 20;
        money = 20;
    }
    private void Ending()
    {
        gamescene = GameScene.Win;
    }

    private void BackToTitle()
    {
        ClearMap();
        gamescene = GameScene.Title;
    }

    protected override void Update(GameTime gameTime) // 窗口每帧更新（和暂停或倍速无关），这里主要负责一些输入输出的计算
    {
        #region some shit
        _time = gameTime.TotalGameTime.TotalMilliseconds;
        Keyboard.GetState();
        Mouse.GetState();
        MouseCoor = Vector2.Transform(new Vector2(Mouse.X(), Mouse.Y()), Matrix.Invert(_view));
        MouseI = (int)MathF.Floor(MouseCoor.X / 64f);
        MouseJ = (int)MathF.Floor(MouseCoor.Y / 64f);
        if (Keyboard.HasBeenPressed(Keys.F11))
            ToggleBorderless();
        if (Keyboard.HasBeenPressed(Keys.Escape))
            Exit();
        if (Keyboard.HasBeenPressed(Keys.F12))
        {
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();
        }
        #endregion
        switch (gamescene)
        {
            case GameScene.Build or GameScene.Battle:
                if (Keyboard.HasBeenPressed(Keys.R))
                    _view = Matrix.Identity; // 恢复视角至初始状态
                if (gamescene == GameScene.Build && (Keyboard.IsPressed(Keys.LeftControl) || Keyboard.IsPressed(Keys.RightControl)) && Keyboard.HasBeenPressed(Keys.Enter))
                    BattleBegin();

                // 这部分是鼠标滚轮缩放
                #region zoom
                if(_zoomEnabled)
                {
                    // Debug.Print(new Vector2(Mouse.X(), Mouse.Y()).ToString());
                    Matrix newView = _view * Matrix.CreateTranslation(-Mouse.X(),-Mouse.Y(),0) * Matrix.CreateScale((float)System.Math.Pow(1.1f,Mouse.Scroll()/120f)) * Matrix.CreateTranslation(Mouse.X(),Mouse.Y(),0);
                    Vector3 scale; Vector3 translation;
                    newView.Decompose(out scale, out _, out translation);

                    // if (scale.X<0.95f && scale.X>0.52f) view =  newView;
                    // else if (scale.X<1.05f && scale.X>0.52f) view = Matrix.CreateTranslation(new Vector3(MathF.Round(translation.X),MathF.Round(translation.Y),MathF.Round(translation.Z)));
                    // else if (scale.X<0.95f && scale.X>0.48f) view = Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(new Vector3(MathF.Round(translation.X),MathF.Round(translation.Y),MathF.Round(translation.Z)));

                    if (scale.X<0.95f) _view =  newView;
                    else if (scale.X<1.05f) _view = Matrix.CreateTranslation(new Vector3(MathF.Round(translation.X),MathF.Round(translation.Y),MathF.Round(translation.Z)));
                }
                #endregion

                #region map dragging
                if(Mouse.RightClicked())
                {
                    _zeroview = _view * Matrix.CreateTranslation(-Mouse.X(),-Mouse.Y(),0);
                }
                if(Mouse.RightDown())
                {
                    _view = _zeroview * Matrix.CreateTranslation(Mouse.X(),Mouse.Y(),0);
                }
                #endregion

                #region tickupdate
                if (Keyboard.HasBeenPressed(Keys.T) && gamestatus == GameStatus.Paused)
                    TickUpdate(); // 暂停状态下，按一次T增加一刻
                if (Keyboard.HasBeenPressed(Keys.Space))
                    if (gamestatus == GameStatus.Paused) gamestatus = GameStatus.Running;
                    else gamestatus = GameStatus.Paused;
                if (Keyboard.HasBeenPressed(Keys.OemTilde))
                    tps = 30; // 半速
                if (Keyboard.HasBeenPressed(Keys.D1))
                    tps = 60; // 一倍速
                if (Keyboard.HasBeenPressed(Keys.D2))
                    tps = 120; // 二倍速
                if (Keyboard.HasBeenPressed(Keys.D3))
                    tps = 180; // 三倍速
                if (Keyboard.HasBeenPressed(Keys.D4))
                    tps = 240;
                if (Keyboard.HasBeenPressed(Keys.D5))
                    tps = 300;
                if (Keyboard.HasBeenPressed(Keys.D6))
                    tps = 360;
                if (Keyboard.HasBeenPressed(Keys.D7))
                    tps = 420;
                if (Keyboard.HasBeenPressed(Keys.D8))
                    tps = 480;
                if (Keyboard.HasBeenPressed(Keys.D9))
                    tps = 540;
                if (Keyboard.HasBeenPressed(Keys.D0))
                    tps = 600; // 不是，我为什么要加这么多奇怪的倍速啊[捂脸]
                timeBank += gameTime.ElapsedGameTime.TotalSeconds;
                if (gamestatus == GameStatus.Paused) timeBank = 0d;
                else 
                {
                    int TickUpdateMax = 10; // 采用的倍速机制使得如果游戏卡顿的话，卡顿结束后游戏会加速来补齐原来的时间流动，但不会超过十倍速
                    while(timeBank > 0d && TickUpdateMax > 0)
                    {
                        TickUpdate();
                        --TickUpdateMax;
                        timeBank -= 1d / tps;
                    }
                }
                #endregion
                
                #region spell UI // 此region已成屎山, 能不动就不动
                if(Mouse.LeftClicked())
                {
                    if(mouseOn?.parent is Spell)
                    {
                        Spell s = (Spell)mouseOn.parent;
                        if(s.showUI && mouseOn.type == WindowType.SpellIcon)
                        {
                            s.showUI = false;
                            s.showLayer = 0;
                        }
                        else
                        {
                            s.showUI = true;
                            s.showLayer = _time;
                            while(s.attachment.type==Attachment.Type.Child)
                            {
                                s = s.attachment.parent;
                                s.showLayer = _time;
                            }
                        }


                        s = (Spell)mouseOn.parent;
                        if(mouseOn.type == WindowType.SpellIcon)
                        {
                            holdingSpell = s;
                            if(Keyboard.IsPressed(Keys.LeftControl) && TakeAble(s))
                            {
                                MoveToInventory(s);
                            }
                        }
                    }
                    else if(mouseOn?.type == WindowType.Life && gamescene == GameScene.Build)
                    {
                        ToggleInventory();
                    }
                    else if(mouseOn?.type == WindowType.Money && gamescene == GameScene.Build)
                    {
                        ToggleShop();
                    }
                }
                if(Mouse.LeftDeClicked() && !Keyboard.IsPressed(Keys.LeftControl))
                {
                    if(inventory[0] != null && TakeAble(inventory[0]))
                    {
                        inventory[0].showLayer = _time;

                        if(mouseOn?.parent is Spell && mouseOn.type == WindowType.SpellSlots)
                        {
                            if(((Spell)mouseOn.parent).children[mouseOn.rank] == null)
                                MoveSpell(inventory[0], new Attachment((Spell)mouseOn.parent, mouseOn.rank));
                        }
                        else if (mouseOn?.type == WindowType.Tower)
                        {
                            if(((Tower)mouseOn.parent).spell == null)
                                MoveSpell(inventory[0], new Attachment((Tower)mouseOn.parent));
                        }
                        else if (mouseOn?.type == WindowType.InventorySlot)
                        {
                            if(inventory[mouseOn.rank] == null)
                                MoveSpell(inventory[0], new Attachment(mouseOn.rank));
                        }
                        else if (mouseOn?.type == WindowType.ShopSlot)
                        {
                            if(shop[-mouseOn.rank] == null)
                                MoveSpell(inventory[0], new Attachment(mouseOn.rank));
                        }
                        else
                        {
                            // MoveSpell(inventory[0], oldAtt);
                        }
                    }
                }
                if(Mouse.LeftDown() && Mouse.FirstMovementSinceLastLeftClick())
                {
                    // Debug.Print("First move!");
                    if(holdingSpell != null && inventory[0] == null && TakeAble(holdingSpell))
                    {
                        oldAtt = MoveSpell(holdingSpell, new Attachment(0));
                        holdingSpell.showUI = true;
                    }
                }
                if(!Mouse.LeftDown()) holdingSpell = null;
                #endregion
                
                #region shop inventory
                if(Keyboard.HasBeenPressed(Keys.S) && gamescene == GameScene.Build)
                {
                    ToggleShop();
                }
                if(Keyboard.HasBeenPressed(Keys.I) && gamescene == GameScene.Build)
                {
                    ToggleInventory();
                }
                if(shopOpen && shopWidth < 256)
                {
                    shopWidth += (int)MathF.Ceiling((256-shopWidth)*0.2f);
                }
                if(!shopOpen && shopWidth > 0)
                {
                    shopWidth -= (int)MathF.Ceiling(shopWidth*0.2f);
                }
                if(inventoryOpen && inventoryWidth < 256)
                {
                    inventoryWidth += (int)MathF.Ceiling((256-inventoryWidth)*0.2f);
                }
                if(!inventoryOpen && inventoryWidth > 0)
                {
                    inventoryWidth -= (int)MathF.Ceiling(inventoryWidth*0.2f);
                }
                #endregion
                
                break;
            case GameScene.Title:
                if(Mouse.LeftClicked())
                {
                    if(mouseOn == newGame)
                    {
                        gamescene = GameScene.Loading;
                        _hasdrawn = false;
                    }
                }
                break;
            case GameScene.Loading:
                if(_hasdrawn) GameBegin();
                break;
            case GameScene.Lose or GameScene.Win:
                if(Mouse.LeftClicked())
                {
                    BackToTitle();
                }
                break;
        }
        base.Update(gameTime);
    }
    private bool TakeAble(Spell s)
    {
        if(!inventoryAvailable) return false;
        if(s == null) return false;
        if(s.attachment.type == Attachment.Type.Inventory)
        {
            if(s.attachment.index == 0) return true;
            else if(s.attachment.index < 0)
            {
                if(s.price > money) return false;
            }
        }
        return true;
    }
    private Attachment MoveSpell(Spell s, Attachment a)
    {
        Debug.Assert(_clearMapFlag || TakeAble(s));
        if(s.attachment.type == Attachment.Type.Inventory && s.attachment.index < 0) money -= s.price;
        Attachment old = s.ReAttach(a);
        // if(!s.used && (s.attachment.type != Attachment.Type.Inventory /* || s.attachment.index > 0*/))
        // {
        //     s.used = true;
        //     s.price /= 2;
        // }
        if(s.attachment.type == Attachment.Type.Inventory && s.attachment.index < 0) money += s.price;
        return old;
    }
    private void MoveToInventory(Spell s)
    {
        if(s == null) return;
        int index = 1;
        while(index < inventory.Length && inventory[index] != null) ++index;
        if(index < inventory.Length)
        {
            MoveSpell(s, new(index));
            holdingSpell = null;
            s.showUI = false;
            s.showLayer = 0;
        }
        else MoveToMouse(s);
    }
    private void MoveToMouse(Spell s)
    {
        if(s == null) return;
        if(inventory[0] == null)
        {
            MoveSpell(s, new(0));
            holdingSpell = null;
            s.showUI = true;
            s.showLayer = _time;
        }
        else
        {
            Spell x = inventory[0];
            while(x.children[0] != null) x = x.children[0];
            MoveSpell(s, new(x, 0));
            holdingSpell = null;
            s.showUI = true;
            s.showLayer = _time;
        }
    }
    private void ToggleInventory()
    {
        inventoryOpen ^= true;
        if(!inventoryOpen)
        {
            for(int i=1;i<inventory.Length;++i)
            {
                if(inventory[i] == null) continue;
                inventory[i].showUI = false;
                inventory[i].showLayer = 0;
            }
        }
    }
    private void ToggleShop()
    {
        shopOpen ^= true;
        if(!shopOpen)
        {
            for(int i=1;i<shop.Length;++i)
            {
                if(shop[i] == null) continue;
                shop[i].showUI = false;
                shop[i].showLayer = 0;
            }
        }
    }
    protected override void Draw(GameTime gameTime) // 显示
    {
        width = GraphicsDevice.Viewport.Width;
        height = GraphicsDevice.Viewport.Height;
        if(!_predraw)
        {
            mouseOn = null;

            _predraw = true;
            Draw(gameTime);
            _predraw = false;
        }
        

        if(!_predraw) GraphicsDevice.Clear(Color.Black); // 背景是黑的

        switch(gamescene)
        {
            case GameScene.Build or GameScene.Battle:


                if(!_predraw) _spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, effect: _mapShader, samplerState: SamplerState.PointClamp);
                if(_predraw)
                {
                    LeftTop = Vector2.Transform(new Vector2(0,0), Matrix.Invert(_view));
                    RightBottom = Vector2.Transform(new Vector2(width,height), Matrix.Invert(_view));
                    xPeriod = Block.numX*Block.Dgrid*64f;
                    yPeriod = Block.numY*Block.Dgrid*64f;
                    projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);
                    _mapShader.Parameters["view_projection"].SetValue(_view * projection);
                }
                _onMap = true;

                // 区块
                // foreach(Block b in blocks) DrawWindow(b.window, new(b.Coordinate().ToPoint(),new(Block.Dgrid*64,Block.Dgrid*64)), null);

                // 路
                foreach(Block b in blocks) foreach(Road r in b.road) DrawWindow(r.window, new(b.Coordinate().ToPoint(), new(Block.Dgrid*64,Block.Dgrid*64)), new());

                // 实体
                foreach(Entity e in entities())  if(e.window.texture != null) DrawWindow(e.window, new(e.RenderCoordinate().ToPoint(), e.size.ToPoint()), new());

                // 蓝门红门
                DrawWindow(BluedoorWindow, new(BluedoorCoor.ToPoint(), new(64,64)),null);
                DrawWindow(ReddoorWindow, new(ReddoorCoor.ToPoint(), new(64,64)),null);

                // 塔和法术
                foreach(Block b in blocks) foreach(Tower t in b.tower) DrawWindow(t.window, new((int)t.Coordinate().X-32,(int)t.Coordinate().Y-32,64,64), new((int)t.Coordinate().X-22,(int)t.Coordinate().Y-22,44,44));
                var l = new SortedList<double, (Spell, Point)>(new DuplicateKeyComparer<double>());
                foreach(Block b in blocks) foreach(Tower t in b.tower) if(t.spell != null) l.Add(t.spell.showLayer, (t.spell, new(t.MapI()*64, t.MapJ()*64)));
                foreach((Spell,Point) sv in l.Values) DrawSpellUI(sv.Item1, sv.Item2.X, sv.Item2.Y);

                
                if(!_predraw) _spriteBatch.End();



                if(!_predraw) _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _onMap = false;

                // 物品栏
                DrawWindow(inventoryWindow, new(shopWidth,0,inventoryWidth,height), null);
                DrawWindow(lifeWindow, new(shopWidth+inventoryWidth+20,height-128,216,44), null);
                
                // 物品栏法术
                for(int i=1;i<inventory.Length;++i) DrawWindow(inventorySlot[i], new(new Point(shopWidth+inventoryWidth-256,0)+InventoryOffset(i),new(64,64)), new(new Point(shopWidth+inventoryWidth-256+10,10)+InventoryOffset(i),new(44,44)));
                l = new SortedList<double, (Spell, Point)>(new DuplicateKeyComparer<double>());
                for(int i=1;i<inventory.Length;++i) if(inventory[i] != null) l.Add(inventory[i].showLayer, (inventory[i],new Point(shopWidth+inventoryWidth-256,0)+InventoryOffset(i)));
                foreach((Spell,Point) sv in l.Values) DrawSpellUI(sv.Item1, sv.Item2.X, sv.Item2.Y);
                
                // 商店栏
                DrawWindow(shopWindow, new(0,0,shopWidth,height), null);
                DrawWindow(moneyWindow, new(shopWidth+20,height-64,216,44), null);

                // 商店栏法术
                for(int i=1;i<shop.Length;++i) DrawWindow(shopSlot[i], new(new Point(shopWidth-256,0)+InventoryOffset(i),new(64,64)), new(new Point(shopWidth-256+10,10)+InventoryOffset(i),new(44,44)));
                l = new SortedList<double, (Spell, Point)>(new DuplicateKeyComparer<double>());
                for(int i=1;i<shop.Length;++i) if(shop[i] != null) l.Add(shop[i].showLayer, (shop[i],new Point(shopWidth-256,0)+InventoryOffset(i)));
                foreach((Spell,Point) sv in l.Values) DrawSpellUI(sv.Item1, sv.Item2.X, sv.Item2.Y);

                // 鼠标上的法术
                if(!_predraw) if(inventory[0] != null) DrawWindow(inventory[0].windowIcon, new(Mouse.Pos().ToPoint(),new(36,36)), new());
                // _spriteBatch.Draw(Spell.TextureIcon[inventory[0].name], Mouse.Pos(), Color.Yellow);

                if(!_predraw) _spriteBatch.End();

                
                break;
            case GameScene.Title:

                if(!_predraw) _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _onMap = false;
                DrawStringWindow(title, new(width/2,height/2-50));
                DrawStringWindow(newGame, new(width/2,height/2+20));
                if(!_predraw) _spriteBatch.End();

                break;
            case GameScene.Win:
                if(!_predraw) GraphicsDevice.Clear(Color.White);
                if(!_predraw) _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _onMap = false;
                DrawStringWindow(win, new(width/2, height/2));
                if(!_predraw) _spriteBatch.End();
                break;
            case GameScene.Lose:
                if(!_predraw) GraphicsDevice.Clear(Color.Black);
                if(!_predraw) _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _onMap = false;
                DrawStringWindow(gameover, new(width/2, height/2));
                if(!_predraw) _spriteBatch.End();
                break;
        }


        if(!_predraw) base.Draw(gameTime);
        if(!_predraw) _hasdrawn = true;
    }

    protected Point InventoryOffset(int i)
    {
        const int SPACING = 32;
        return new(SPACING+(i-1)%3*64,SPACING+(i-1)/3*64);
    }
    protected void DrawSpellUI(Spell s, int x, int y)
    {
        if(!s.showUI)
        {
            DrawWindow(s.windowIcon, new(x+14, y+14, 36, 36), new(x+10, y+10, 44, 44));
        }
        else
        {
            DrawWindow(s.windowUIouter, new(new(x-2,y-2), s.UIsize+new Point(4,4)), null);
            DrawWindow(s.windowUI, new(new(x,y), s.UIsize), null);
            DrawWindow(s.windowSlot, new(x, y, 64, 64), new());
            DrawWindow(s.windowIcon, new(x+14, y+14, 36, 36), new(x+10, y+10, 44, 44));
            foreach(Window ws in s.windowSlots) DrawWindow(ws, new(x, y, ws.texture.Width, ws.texture.Height), new(new Point(x,y) + ws.textOffset + new Point(-54,-4), new(44,44)));
            var l = new SortedList<double, (Spell,Point)>(new DuplicateKeyComparer<double>());
            for(int i=0;i<s.children.Length; ++i) if(s.children[i] != null) l.Add(s.children[i].showLayer, (s.children[i], new(x+s.windowSlots[i].textOffset.X-64, y+s.windowSlots[i].textOffset.Y-14)));
            foreach((Spell,Point) sv in l.Values) DrawSpellUI(sv.Item1, sv.Item2.X, sv.Item2.Y);
        }
    }
    protected void DrawStringWindow(Window w, Point position, bool mouseCatch = true)
    {
        DrawWindow(w, new(position - (_font.MeasureString(w.text) * w.textScale / 2).ToPoint(), (_font.MeasureString(w.text)*w.textScale).ToPoint()), mouseCatch ? null : new());
    }
    protected void DrawWindow(Window w, Rectangle RectRender, Rectangle? RectMouseCatch)
    {
        w.Update();
        Rectangle rectMouseCatch = RectMouseCatch??RectRender;

        if(_predraw)
        {
            if(_onMap)
            {
                for(int i = (int)MathF.Ceiling((LeftTop.X-rectMouseCatch.Right)/xPeriod);i<(RightBottom.X-rectMouseCatch.Left)/xPeriod;++i)
                    for(int j = (int)MathF.Ceiling((LeftTop.Y-rectMouseCatch.Bottom)/yPeriod);j<(RightBottom.Y-rectMouseCatch.Top)/yPeriod;++j)
                    {
                        Rectangle r = rectMouseCatch;
                        r.Offset(i*xPeriod, j*yPeriod);
                        if (r.Contains(MouseCoor)) mouseOn = w;
                    }
            }
            else
                if (rectMouseCatch.Contains(Mouse.Pos())) mouseOn = w;
        }
        else
        {
            if(_onMap)
            {
                for(int i = (int)MathF.Ceiling((LeftTop.X-RectRender.Right)/xPeriod);i<(RightBottom.X-RectRender.Left)/xPeriod;++i)
                    for(int j = (int)MathF.Ceiling((LeftTop.Y-RectRender.Bottom)/yPeriod);j<(RightBottom.Y-RectRender.Top)/yPeriod;++j)
                    {
                        Rectangle r = RectRender;
                        r.Offset(i*xPeriod, j*yPeriod);
                        _spriteBatch.Draw(w.texture??defaultTexture, r, null, (w.clickable && mouseOn == w) ? Color.Yellow : w.color, w.rotation, new(), new(), 0);
                        if(w.text != null) _spriteBatch.DrawString(_font, w.text, (r.Location + w.textOffset).ToVector2(), (w.clickable && mouseOn == w) ? Color.Yellow : w.textColor, w.rotation, new(), w.textScale, SpriteEffects.None, 0);
                        if(w.text2 != null) _spriteBatch.DrawString(_font, w.text2, (r.Location + w.text2Offset).ToVector2(), (w.clickable && mouseOn == w) ? Color.Yellow : w.text2Color, w.rotation, new(), w.text2Scale, SpriteEffects.None, 0);
                    }
            }
            else
                _spriteBatch.Draw(w.texture??defaultTexture, RectRender, null, (w.clickable && mouseOn == w) ? Color.Yellow : w.color, w.rotation, new(), new(), 0);
                if(w.text != null) _spriteBatch.DrawString(_font, w.text, (RectRender.Location + w.textOffset).ToVector2(), (w.clickable && mouseOn == w) ? Color.Yellow : w.textColor, w.rotation, new(), w.textScale, SpriteEffects.None, 0);
                if(w.text2 != null) _spriteBatch.DrawString(_font, w.text2, (RectRender.Location + w.text2Offset).ToVector2(), (w.clickable && mouseOn == w) ? Color.Yellow : w.text2Color, w.rotation, new(), w.text2Scale, SpriteEffects.None, 0);
        }
    }

    // 下面是关于全屏显示的东西，不用管
    #region fullscreen

    bool _isFullscreen = false;
    bool _isBorderless = false;
    int _width = 0;
    int _height = 0;

    public void ToggleFullscreen() {
        bool oldIsFullscreen = _isFullscreen;

        if (_isBorderless) {
            _isBorderless = false;
        } else {
            _isFullscreen = !_isFullscreen;
        }

        ApplyFullscreenChange(oldIsFullscreen);
    }
    public void ToggleBorderless() {
        bool oldIsFullscreen = _isFullscreen;

        _isBorderless = !_isBorderless;
        _isFullscreen = _isBorderless;

        ApplyFullscreenChange(oldIsFullscreen);
    }

    private void ApplyFullscreenChange(bool oldIsFullscreen) {
        if (_isFullscreen) {
            if (oldIsFullscreen) {
                ApplyHardwareMode();
            } else {
                SetFullscreen();
            }
        } else {
            UnsetFullscreen();
        }
    }
    private void ApplyHardwareMode() {
        _graphics.HardwareModeSwitch = !_isBorderless;
        _graphics.ApplyChanges();
    }
    private void SetFullscreen() {
        _width = Window.ClientBounds.Width;
        _height = Window.ClientBounds.Height;

        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        _graphics.HardwareModeSwitch = !_isBorderless;

        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
    }
    private void UnsetFullscreen() {
        _graphics.PreferredBackBufferWidth = _width;
        _graphics.PreferredBackBufferHeight = _height;
        _graphics.IsFullScreen = false;
        _graphics.ApplyChanges();
    }
    
    #endregion


}

#region Dupilcate Key
public class DuplicateKeyComparer<TKey>
                :
             IComparer<TKey> where TKey : IComparable
{
    #region IComparer<TKey> Members

    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        if (result == 0)
            return 1; // Handle equality as being greater. Note: this will break Remove(key) or
        else          // IndexOfKey(key) since the comparer never returns 0 to signal key equality
            return result;
    }

    #endregion
}

#endregion
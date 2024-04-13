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
    static bool CHEATALLOWED = true;
    static bool SHOPALWAYSMAX = true;
    public Random rand = new(RandomNumberGenerator.GetInt32(2147483647));
    private double _time;
    private int _exitPower;
    private GraphicsDeviceManager _graphics;
    public SpriteFont _font;
    private SpriteBatch _spriteBatch;
    private Effect _mapShader;
    // private Effect _guiShader;
    private Matrix _view = Matrix.CreateTranslation(720,405,0);
    private Matrix _zeroview = Matrix.Identity;
    private bool _zoomEnabled = false;
    private Matrix projection = Matrix.CreateOrthographicOffCenter(0, 800, 600, 0, 0, 1);
    public Vector2 MouseCoor = new();
    public Vector2 LeftTop = new();
    public Vector2 RightBottom = new();
    public int xGrid, yGrid;
    public float xPeriod, yPeriod;
    public int MouseI = 0, MouseJ = 0;
    int width;
    int height;
    public long tick; // 游戏从开始经过的刻数
    // private long thingCount; // 游戏从开始产生的Entity, Spell, Spellcast总数
    private double timeBank;
    public GameStatus gamestatus = GameStatus.Running; // 是不是暂停
    public GameScene gamescene;
    public int life;
    public int money;
    public int tps = 60; // 每秒多少刻（控制倍速，60刻是一倍速）
    // private LinkedList<Entity> entities;
    public LinkedList<Enemy> enemy;
    private LinkedList<Entity> neutral;
    private LinkedList<Projectile> projectile;
    private LinkedList<Spell> spell; // 十分关键的字典，其中每个spell都有一个唯一的id
    private LinkedList<Spellcast> spellcast; // 十分关键的字典，其中每个spellcast都有一个唯一的id
    private List<Spell> enemyStack;
    private List<Name> cardDeck;
    private int enemyRate;
    private Block[,] blocks;
    public float manaMax;
    public float[,] mana;
    public Window[,] manaWindow;
    public Color manaColor;
    public Segment Reddoor, Bluedoor;
    public Window ReddoorWindow, BluedoorWindow;
    public Vector2 ReddoorCoor, BluedoorCoor, doorCoor;
    public int ReddoorIndex, BluedoorIndex;
    private Window mouseOn;
    private Spell holdingSpell = null;
    private Attachment oldAtt = null;
    public List<Spell> inventory;
    public List<Spell> shop;
    public List<Window> inventorySlot;
    public List<Window> shopSlot;
    private Window title, win, gameover, shopWindow, moneyWindow, inventoryWindow, lifeWindow;
    private Window fullscreen, rightmouse, leftmouse, space, numbers, newgame, quit;
    private Block titleBlock;
    private Window startBattle;
    private Window stageWave, gamespeed, paused; 
    private int shopWidth, inventoryWidth;
    private bool shopOpen, inventoryOpen, inventoryAvailable;
    public int stage, wave;
    // private Spell summonenemy1, summonenemyEasy, summonenemyFast, summonenemyVeryFast;
    private Dictionary<Name, Spell> enemySpell = null;
    public Spell summoncross1;
    public static Texture2D slotTexture, slotLeftTexture, slotUpTexture;
    public static Texture2D doorTexture;
    public static Texture2D defaultTexture;
    public static Texture2D whiteTexture;
    public static Texture2D transparentTexture;

    public static Texture2D invin2ed;

    private bool _predraw = false;
    public bool _hasdrawn;
    private bool _onMap;
    private bool _clearMapFlag = false;

    private long _lastenemyspawntick;
    private int _spawnedenemy;



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
        GameObject.game = this;
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

    #region LoadContent
    protected override void LoadContent() // 加载材质
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        defaultTexture = Content.Load<Texture2D>("default");
        whiteTexture = Content.Load<Texture2D>("white");
        transparentTexture = Content.Load<Texture2D>("transparent");
        slotTexture = Content.Load<Texture2D>("slot");
        slotUpTexture = Content.Load<Texture2D>("slotUp");
        slotLeftTexture = Content.Load<Texture2D>("slotLeft");
        doorTexture = Content.Load<Texture2D>("door");

        #region font
        
        var characters = new List<char>(){
            ' ','!','\"','#','$','%','&','\'','(',')','*','+',',','-','.','/',
            '0','1','2','3','4','5','6','7','8','9',
            '?',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            '[','\\',']','^','_','`',
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            '{','|','}','~','µ','λ',
            '？',};

        var glyphBounds = new List<Rectangle>();
        for(int i=0;i<16;++i) glyphBounds.Add(new(9*i,0,7,7));
        for(int i=0;i<10;++i) glyphBounds.Add(new(9*i,9,7,7));
        for(int i=0;i<1;++i) glyphBounds.Add(new(9*i,18,7,7));
        for(int i=0;i<26;++i) glyphBounds.Add(new(9*i,27,7,7));
        for(int i=0;i<6;++i) glyphBounds.Add(new(9*i,36,7,7));
        for(int i=0;i<26;++i) glyphBounds.Add(new(9*i,27,7,7));
        for(int i=0;i<6;++i) glyphBounds.Add(new(9*i,45,7,7));
        glyphBounds.Add(new(225,27,7,7));
        
        var cropping = new List<Rectangle>
        {
            new(),
            new(0, 0, 1, 7), // !
            new(0, 0, 3, 7), // "
            new(0, 0, 5, 7), // #
            new(0, 0, 7, 7), // $
            new(0, 0, 7, 7), // %
            new(0, 0, 7, 7), // &
            new(0, 0, 1, 7), // '
            new(0, 0, 4, 7), // (
            new(0, 0, 4, 7), // )
            new(0, 0, 5, 7), // *
            new(0, 0, 5, 7), // +
            new(0, 0, 2, 7), // ,
            new(0, 0, 5, 7), // -
            new(0, 0, 2, 7), // .
            new(0, 0, 5, 7), // /
        };
        for(int i=0;i<10;++i) cropping.Add(new(0,0,4,7));
        cropping.AddRange(new List<Rectangle>(){
            new(0, 0, 5, 7), // ?
        });
        for(int i=0;i<26;++i) cropping.Add(new(0,0,7,7));
        cropping.AddRange(new List<Rectangle>(){
            new(0, 0, 4, 7), // [
            new(0, 0, 5, 7), // \
            new(0, 0, 4, 7), // ]
            new(0, 0, 3, 7), // ^
            new(0, 0, 3, 7), // _
            new(0, 0, 2, 7), // `
        });
        for(int i=0;i<26;++i) cropping.Add(new(0,0,7,7));
        cropping.AddRange(new List<Rectangle>(){
            new(0, 0, 4, 7), // {
            new(0, 0, 1, 7), // |
            new(0, 0, 4, 7), // }
            new(0, 0, 5, 7), // ~
            new(0, 0, 7, 7), // µ
            new(0, 0, 7, 7), // λ
        });
        cropping.Add(new(0,0,7,7));
        
        var kerning = new List<Vector3>()
        {
            new(0, 7, 0),
            new(1, 1, 1), // !
            new(1, 3, 1), // "
            new(1, 5, 1), // #
            new(1, 7, 1), // $
            new(1, 7, 1), // %
            new(1, 7, 1), // &
            new(1, 1, 1), // '
            new(1, 4, 1), // (
            new(1, 4, 1), // )
            new(1, 5, 1), // *
            new(1, 5, 1), // +
            new(1, 2, 1), // ,
            new(1, 5, 1), // -
            new(1, 2, 1), // .
            new(1, 5, 1), // /
        };
        for(int i=0;i<10;++i) kerning.Add(new(1,4,1));
        kerning.AddRange(new List<Vector3>(){
            new(1, 5, 1), // ?
        });
        for(int i=0;i<26;++i) kerning.Add(new(1,7,1));
        kerning.AddRange(new List<Vector3>(){
            new(1, 4, 1), // [
            new(1, 5, 1), // \
            new(1, 4, 1), // ]
            new(1, 3, 1), // ^
            new(1, 3, 1), // _
            new(1, 2, 1), // `
        });
        for(int i=0;i<26;++i) kerning.Add(new(1,7,1));
        kerning.AddRange(new List<Vector3>(){
            new(1, 4, 1), // {
            new(1, 1, 1), // |
            new(1, 4, 1), // }
            new(1, 5, 1), // ~
            new(1, 7, 1), // µ
            new(1, 7, 1), // λ
        });
        kerning.Add(new(1,7,1));
        
        _font = new SpriteFont(Content.Load<Texture2D>("font"), glyphBounds, cropping, characters, 12, 0, kerning, '？');
        
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

        Entity.Texture[Name.Square1] = Content.Load<Texture2D>("enemysquare1");
        Entity.Texture[Name.Diamond1] = Content.Load<Texture2D>("enemyDiamond1");
        Entity.Texture[Name.Circle1] = Content.Load<Texture2D>("enemyCircle1");
        Entity.Texture[Name.Cross1] = Content.Load<Texture2D>("enemyCross1");

        Entity.Texture[Name.Square2] = Content.Load<Texture2D>("Square2");
        Entity.Texture[Name.Diamond2] = Content.Load<Texture2D>("Diamond2");
        Entity.Texture[Name.Circle2] = Content.Load<Texture2D>("Circle2");
        Entity.Texture[Name.Runner2] = Content.Load<Texture2D>("Runner2");
        Entity.Texture[Name.Phasor2] = Content.Load<Texture2D>("Phasor2");
        Entity.Texture[Name.Crossgen2] = Content.Load<Texture2D>("Crossgen2");
        Entity.Texture[Name.Heal2] = Content.Load<Texture2D>("Heal2");
        Entity.Texture[Name.Dark2] = Content.Load<Texture2D>("Dark2");
        Entity.Texture[Name.Invin2] = Content.Load<Texture2D>("Invin2");
        invin2ed = Content.Load<Texture2D>("Invin2ed");
        Entity.Texture[Name.Ghost2] = Content.Load<Texture2D>("Ghost2");

        Entity.Texture[Name.Runner3] = Content.Load<Texture2D>("Runner3");
        Entity.Texture[Name.Phasor3] = Content.Load<Texture2D>("Phasor3");
        Entity.Texture[Name.SpeedField3] = Content.Load<Texture2D>("SpeedField3");
        Entity.Texture[Name.Circle3] = Content.Load<Texture2D>("Circle3");
        Entity.Texture[Name.ShieldField3] = Content.Load<Texture2D>("ShieldField3");
        Entity.Texture[Name.Heal3] = Content.Load<Texture2D>("Heal3");
        Entity.Texture[Name.HealField3] = Content.Load<Texture2D>("HealField3");
        Entity.Texture[Name.Dark3] = Content.Load<Texture2D>("Dark3");
        Entity.Texture[Name.InvinField3] = Content.Load<Texture2D>("InvinField3");
        Entity.Texture[Name.Ghost3] = Content.Load<Texture2D>("Ghost3");



        Entity.Texture[Name.Projectile1] = Content.Load<Texture2D>("projectile1");
        Entity.Texture[Name.Stone] = Content.Load<Texture2D>("Stone");
        Entity.Texture[Name.Spike] = Content.Load<Texture2D>("Spike");
        Entity.Texture[Name.Arrow] = Content.Load<Texture2D>("Arrow");
        Entity.Texture[Name.SquareD6] = whiteTexture;
        Entity.Texture[Name.ExplosionSquareD6] = whiteTexture;

        // Spell.TextureIcon[Name.SummonEnemy] = Content.Load<Texture2D>("SummonEnemy1icon");
        // Spell.TextureIcon[Name.SummonProjectile] = Content.Load<Texture2D>("SummonProjectile1icon");
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
        Spell.TextureIcon[Name.DoubleSpeed] = Content.Load<Texture2D>("doublespeed");
        Spell.TextureIcon[Name.DoubleCast] = Content.Load<Texture2D>("DoubleCast");
        Spell.TextureIcon[Name.TwiceCast] = Content.Load<Texture2D>("TwiceCast");
        Spell.TextureIcon[Name.CastEvery64Ticks] = Content.Load<Texture2D>("cast4timesslow");
        Spell.TextureIcon[Name.CastEvery8Ticks] = Content.Load<Texture2D>("cast16timesslow");
        Spell.TextureIcon[Name.CastEveryTick] = Content.Load<Texture2D>("cast64times");
        Spell.TextureIcon[Name.RandomAim] = Content.Load<Texture2D>("RandomAim");
        Spell.TextureIcon[Name.RandomWait] = Content.Load<Texture2D>("RandomWait");
        Spell.TextureIcon[Name.Aiming] = Content.Load<Texture2D>("Aiming");
        Spell.TextureIcon[Name.ScaleUp] = Content.Load<Texture2D>("ScaleUp");
        Spell.TextureIcon[Name.ScaleDown] = Content.Load<Texture2D>("ScaleDown");

        Spell.TextureIcon[Name.Projectile1] = Content.Load<Texture2D>("summonProjectile1");
        Spell.TextureIcon[Name.Stone] = Content.Load<Texture2D>("summonStone");
        Spell.TextureIcon[Name.Spike] = Content.Load<Texture2D>("summonSpike");
        Spell.TextureIcon[Name.Arrow] = Content.Load<Texture2D>("summonArrow");
        Spell.TextureIcon[Name.ExplosionSquareD6] = Content.Load<Texture2D>("summonexplosion");

        Spell.TextureSlot[(2,0)] = Content.Load<Texture2D>("spellgui2slot0");
        Spell.TextureSlot[(2,1)] = Content.Load<Texture2D>("spellgui2slot1");
        Spell.TextureSlot[(1,0)] = Content.Load<Texture2D>("spellgui1slot0");
        #endregion

        #region random shit
        
        title = new Window(this, WindowType.Title, transparentTexture, Color.Transparent, clickable: false){
            text = "THE GAME IS A TOWER DEFENSE GAME NAMED ",
            textScale = 4
        };
        fullscreen = new Window(this, WindowType.Title, transparentTexture, Color.Transparent, clickable: false){
            text = "press f11 to fullscreen",
            textScale = 2
        };
        rightmouse = new Window(this, WindowType.Title, transparentTexture, Color.Transparent, clickable: false){
            text = "use right mouse to move map",
            textScale = 2
        };
        leftmouse = new Window(this, WindowType.Title, transparentTexture, Color.Transparent, clickable: false){
            text = "use left mouse to move spells",
            textScale = 2
        };
        space = new Window(this, WindowType.Title, transparentTexture, Color.Transparent, clickable: false){
            text = "press space to pause",
            textScale = 2
        };
        numbers = new Window(this, WindowType.Title, transparentTexture, Color.Transparent, clickable: false){
            text = "press numbers to change game speed",
            textScale = 2
        };
        newgame = new Window(this, WindowType.Title, transparentTexture, Color.Transparent, clickable: false){
            text = "new game",
            textScale = 2
        };
        quit = new Window(this, WindowType.Title, transparentTexture, Color.Transparent, clickable: false){
            text = "quit",
            textScale = 2
        };
        // newGame = new Window(this, WindowType.NewGame, transparentTexture, Color.Transparent){
        //     text = "new game",
        //     textScale = 2
        // };




        win = new Window(this, WindowType.Win, transparentTexture, Color.Transparent, clickable: false){
            text = "GAME (DEMO VERSION) COMPLETED\n\nTHANKS FOR PLAYING!",
            textScale = 4,
            textColor = Color.Black
        };
        gameover = new Window(this, WindowType.GameOver, transparentTexture, Color.Transparent, clickable: false){
            text = "GAME OVER",
            textScale = 4,
        };
        stageWave = new Window(this, WindowType.StageWave, transparentTexture, Color.Transparent, clickable: false){
            textScale = 3,
        };
        gamespeed = new Window(this, WindowType.GameSpeed, transparentTexture, Color.Transparent, clickable: false){
            textScale = 3,
        };
        paused = new Window(this, WindowType.Paused, transparentTexture, Color.Transparent, clickable: false){
            textScale = 3,
        };
        shopWindow = new Window(this, WindowType.Shop, whiteTexture, Color.SaddleBrown, clickable: false);
        inventoryWindow = new Window(this, WindowType.Inventory, whiteTexture, Color.DarkBlue, clickable: false);
        moneyWindow = new Window(this, WindowType.Money, whiteTexture, Color.SaddleBrown){
            textScale = 2,
            textOffset = new(15,15)
        };
        lifeWindow = new Window(this, WindowType.Life, whiteTexture, Color.DarkBlue){
            textScale = 2,
            textOffset = new(15,15)
        };
        startBattle = new Window(this, WindowType.StartBattle, whiteTexture, Color.Black, clickable: true){
            textScale = 2,
            textOffset = new(15,15)
        };
        // _lightgrey = Content.Load<Texture2D>("lightgrey");
        // _darkgrey = Content.Load<Texture2D>("darkgrey");

        #endregion

        _mapShader = Content.Load<Effect>("map-shader");


        InitTitle();
    }
    #endregion

    public void InitTitle()
    {
        _view = Matrix.CreateTranslation(GraphicsDevice.Viewport.Width/2,GraphicsDevice.Viewport.Height/2,0);
        ClearMap();
        mana = null;
        titleBlock = new(true, new(-160,600));
        blocks = new Block[1,1];
        blocks[0,0] = titleBlock;
        spell = new();
        InitInventory(0);
        inventoryAvailable = true;
        NewSpell(Name.SummonProjectile, Name.Projectile1).ReAttach(new(titleBlock.tower[0]));
        NewSpell(Name.AimLeft).ReAttach(new(titleBlock.tower[1]));
        NewSpell(Name.Add10Speed, Name.Projectile1).ReAttach(new(titleBlock.tower[2]));
        enemy.AddLast(Enemy.Title(new(0,760), true));
        enemy.AddLast(Enemy.Title(new(0,888), false));
    }

    public Enemy NewEnemy(Name name, Segment segment, float progress)
    {
        return enemy.AddLast(new Enemy(name, segment, progress)).Value;
    }
    public Projectile NewProjectile(Name name, Vector2 coordinate, Vector2 velocity)
    {
        return projectile.AddLast(new Projectile(name, coordinate, velocity)).Value;
    }
    public Spell NewSpell(Name name, Name summonedEntity = Name.Null)
    {
        return spell.AddLast(new Spell(name, summonedEntity)).Value;
    }
    public Spellcast NewSpellcast(Spell spell, Cast cast)
    {
        return spellcast.AddLast(new Spellcast(spell, cast)).Value;
    }

    public IEnumerable<Entity> entities()
    {
        foreach(Entity e in projectile) yield return e;
        foreach(Entity e in neutral) yield return e;
        foreach(Entity e in enemy) yield return e;
    }
    public IEnumerable<Entity> Collisions(Entity e) // 简单的碰撞判定算法。之后可能会出现圆形的东西，从而需要修改。另外以后算法上可能会需要优化。
    {
        var E = e.hitbox;
        var Es = new RectangleF[9];
        for(int i=0;i<9;++i)
        {
            Es[i] = E;
            Es[i].Offset((i/3-1)*xPeriod, (i%3-1)*yPeriod);
        }
        if(e is not Enemy) foreach(Entity f in enemy)
        {
            var F = f.hitbox;
            for(int i=0;i<9;++i) if(F.IntersectsWith(Es[i]))
            {
                yield return f;
                break;
            }
        }
        foreach(Entity f in neutral) if (f!=e)
        {
            var F = f.hitbox;
            for(int i=0;i<9;++i) if(F.IntersectsWith(Es[i]))
            {
                yield return f;
                break;
            }
        }
        if(e is not Projectile) foreach(Entity f in projectile)
        {
            var F = f.hitbox;
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
        if(gamescene == GameScene.Battle) life -= i;
        // Debug.Print("life: " + life.ToString());
    }
    public void GenerateCardDeck()
    {
        if(enemySpell == null)
        {
            enemySpell = new()
            {
                {Name.Square1, NewSpell(Name.SummonEnemy, Name.Square1)},
                {Name.Diamond1, NewSpell(Name.SummonEnemy, Name.Diamond1)},
                {Name.Circle1, NewSpell(Name.SummonEnemy, Name.Circle1)},
                {Name.Cross1, NewSpell(Name.SummonEnemy, Name.Cross1)},

                {Name.Square2, NewSpell(Name.SummonEnemy, Name.Square2)},
                {Name.Diamond2, NewSpell(Name.SummonEnemy, Name.Diamond2)},
                {Name.Circle2, NewSpell(Name.SummonEnemy, Name.Circle2)},
                {Name.Runner2, NewSpell(Name.SummonEnemy, Name.Runner2)},
                {Name.Phasor2, NewSpell(Name.SummonEnemy, Name.Phasor2)},
                {Name.Crossgen2, NewSpell(Name.SummonEnemy, Name.Crossgen2)},
                {Name.Heal2, NewSpell(Name.SummonEnemy, Name.Heal2)},
                {Name.Dark2, NewSpell(Name.SummonEnemy, Name.Dark2)},
                {Name.Invin2, NewSpell(Name.SummonEnemy, Name.Invin2)},
                {Name.Ghost2, NewSpell(Name.SummonEnemy, Name.Ghost2)},

                {Name.Runner3, NewSpell(Name.SummonEnemy, Name.Runner3)},
                {Name.Phasor3, NewSpell(Name.SummonEnemy, Name.Phasor3)},
                {Name.SpeedField3, NewSpell(Name.SummonEnemy, Name.SpeedField3)},
                {Name.Circle3, NewSpell(Name.SummonEnemy, Name.Circle3)},
                {Name.ShieldField3, NewSpell(Name.SummonEnemy, Name.ShieldField3)},
                {Name.Heal3, NewSpell(Name.SummonEnemy, Name.Heal3)},
                {Name.HealField3, NewSpell(Name.SummonEnemy, Name.HealField3)},
                {Name.Dark3, NewSpell(Name.SummonEnemy, Name.Dark3)},
                {Name.InvinField3, NewSpell(Name.SummonEnemy, Name.InvinField3)},
                {Name.Ghost3, NewSpell(Name.SummonEnemy, Name.Ghost3)},
            };
            Spell x1 = NewSpell(Name.SummonEnemy, Name.Cross1);
            Spell x2 = NewSpell(Name.SummonEnemy, Name.Cross1);
            Spell x3 = NewSpell(Name.SummonEnemy, Name.Cross1);
            x1.ReAttach(new(enemySpell[Name.Cross1], 0));
            x2.ReAttach(new(x1, 0));
            x3.ReAttach(new(x2, 0));

            summoncross1 = NewSpell(Name.SummonEnemy, Name.Cross1);
        }



        cardDeck = new();
        switch(stage)
        {
            case 1:
                cardDeck.Add(Name.Square1);
                for(int i=1;i<wave switch{1=>1,2=>2,3=>3,4=>5,5=>10,_=>throw new ArgumentOutOfRangeException()};++i)
                {
                    cardDeck.Add(Entity.RandomCard[1].Next());
                }
                break;
            case 2:
                for(int i=0;i<wave switch{1=>1,2=>2,3=>3,4=>5,5=>10,_=>throw new ArgumentOutOfRangeException()};++i)
                {
                    cardDeck.Add(Entity.RandomCard[2].Next());
                }
                break;
            case 3:
                for(int i=0;i<wave switch{1=>1,2=>2,3=>3,4=>5,5=>10,_=>throw new ArgumentOutOfRangeException()};++i)
                {
                    cardDeck.Add(Entity.RandomCard[3].Next());
                }
                break;
            case 4:
                for(int i=0;i<wave switch{1=>1,_=>throw new ArgumentOutOfRangeException()};++i)
                {
                    cardDeck.Add(Entity.RandomCard[4].Next());
                }
                break;
        }
}
    public void GenerateEnemyStack()
    {
        
        enemyStack = new();
        foreach(Name e in cardDeck)
            for(int i=0;i<Entity.CardNum[e];++i)
                enemyStack.Add(enemySpell[e]);
        Shuffle(enemyStack);
    }
    public bool shouldSpawnEnemy()
    {
        if(tick==0) return true;
        long interval = tick - _lastenemyspawntick;
        if(interval < enemyRate / 3) return false;
        if(interval > enemyRate * 2) if(rand.Next(1 + enemyRate) == 0) return true;
        if(tick > enemyRate * _spawnedenemy) if(rand.Next(1 + enemyRate) == 0) return true;
        if(rand.Next(1 + enemyRate * 2 / 3) == 0) return true;
        else return false;
    }

    #region TickUpdate
    protected void TickUpdate() // 游戏内每刻更新（暂停时不会调用，倍速时会更频繁调用），这里主要负责核心内部机制的计算
    {
        if(!_hasdrawn) return;
        switch(gamescene)
        {
            case GameScene.Build:
                // enemyRate *= 2;
                if(enemyStack.Count == 0) GenerateEnemyStack();
                if(enemyStack.Count > 0 && shouldSpawnEnemy())
                {
                    NewSpellcast(enemyStack.Last(), new(new Vector2()));
                    enemyStack.RemoveAt(enemyStack.Count - 1);
                    _lastenemyspawntick = tick;
                    ++_spawnedenemy;
                }
                // enemyRate /= 2;
                break;
            case GameScene.Battle:
                if(tick == 0) GenerateEnemyStack();
                if(enemyStack.Count > 0 && shouldSpawnEnemy())
                {
                    NewSpellcast(enemyStack.Last(), new(new Vector2()));
                    enemyStack.RemoveAt(enemyStack.Count - 1);
                    _lastenemyspawntick = tick;
                    ++_spawnedenemy;
                }
                break;
        }
        
        // 修改这里的顺序前务必仔细思考，否则可能会出现意想不到的情况
        #region mana
        if(gamescene != GameScene.Title && mana != null)
        {
            var oldmana = (float[,])mana.Clone();
            for(int x=0;x<xGrid;++x) for(int y=0;y<yGrid;++y)
            {
                mana[x,y] += 0.125f * (oldmana[(x+1)%xGrid,y] + oldmana[(x+xGrid-1)%xGrid,y] + oldmana[x,(y+1)%yGrid] + oldmana[x,(y+yGrid-1)%yGrid] - 4*oldmana[x,y]); // 法术流动        
                mana[x,y] += 0.005f * (manaMax-oldmana[x,y]); // 法术恢复
            }
        }
        #endregion
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
        for(var n = spellcast.First;n!=null;n=m2) // 移除被标记为死亡的Spellcast
        {
            m2 = n.Next;
            if(!n.Value.alive) spellcast.Remove(n);
        }
        foreach(Block b in blocks) foreach(Tower t in b.tower) if(t.spell != null)
            t.TickUpdate(); // 塔施法
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
    #endregion






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

        _spawnedenemy = 0;
        _lastenemyspawntick = 0;

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

        for(int x=0;x<xGrid;++x) for(int y=0;y<yGrid;++y) mana[x,y] = 0;

        _spawnedenemy = 0;
        _lastenemyspawntick = 0;
    }
    #region InitMap
    private void InitMap(int numX, int numY, Func<int, bool> pathRoadNum, float manaMax, Color manaColor)
    {
        ClearMap();
        _view = Matrix.Identity;
        
        #region blocks
        Block.numX = numX;
        Block.numY = numY;
        blocks = new Block[Block.numX,Block.numY];
        do{
            for(int x=0;x<Block.numX;++x) for(int y=0;y<Block.numY;++y)
                blocks[x,y] = new(RandomBlockName.Next(), x,y);
            BluedoorIndex = rand.Next(8);
            Bluedoor = RefindPath(Blocks(rand.Next(Block.numX), rand.Next(Block.numY)),BluedoorIndex);
        } while(!pathRoadNum(_pathRoadNum));
        // } while(_pathRoadNum != 35);
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
        doorCoor = ReddoorCoor;
        _view = Matrix.CreateTranslation(width/2-doorCoor.X, height/2-doorCoor.Y, 0); // 恢复视角至初始状态


        #endregion
        
        #region mana field
        this.manaMax = manaMax;
        this.manaColor = manaColor;
        xGrid = Block.numX * Block.Dgrid;
        yGrid = Block.numY * Block.Dgrid;
        mana = new float[xGrid, yGrid];
        for(int x=0;x<xGrid;++x) for(int y=0;y<yGrid;++y) mana[x,y] = 0;
        manaWindow = new Window[xGrid, yGrid];
        for(int x=0;x<xGrid;++x) for(int y=0;y<yGrid;++y) manaWindow[x,y] = new(this, WindowType.Mana, whiteTexture, manaColor, clickable:false){manaX = x, manaY = y};

        #endregion
        // foreach(Block b in blocks) foreach(Tower t in b.tower)
        // {
        //     RandomNewSpell().ReAttach(new(t));
        // }
    }
    #endregion
    private void InitInventory(int inventorySize)
    {
        inventory = new(1 + inventorySize);
        inventorySlot = new(1 + inventorySize);
        for(int i=0;i<1 + inventorySize;++i)
        {
            inventory.Add(null);
            inventorySlot.Add(new Window(this, WindowType.InventorySlot, slotTexture, Color.White){
                rank = i,
            });
        }
        if(gamescene == GameScene.Build)
        {    
            NewSpell(Name.SummonProjectile, Name.Projectile1).ReAttach(new(1));
            NewSpell(Name.AimMouse).ReAttach(new(2));
            NewSpell(Name.AddSpeed).ReAttach(new(3));
        }
    }
    private void InitShop(int shopSize)
    {
        shop = new(1 + shopSize);
        shopSlot = new(1 + shopSize);
        for(int i=0;i<1 + shopSize;++i)
        {
            shop.Add(null);
            shopSlot.Add(new Window(this, WindowType.ShopSlot, slotTexture, Color.Gold){
                rank = -i,
            });
        }
        for(int i=1;i<shop.Count;++i)
        {
            RandomNewSpell().ReAttach(new(-i));
        }
    }
    private static readonly RanDict<BlockName> RandomBlockName = new(){{BlockName.A,2}, {BlockName.B,1}};
    private static readonly RanDict<Name> RandomProjectileName = new(){
        {Name.Projectile1, 1},
        {Name.Stone, 1},
        {Name.Arrow, 1},
        {Name.Spike, 1},
        {Name.ExplosionSquareD6, 0.5},
    };
    private static readonly RanDict<Name> RandomSpellName = new(){
        {Name.SummonProjectile, 6},
        {Name.Add10Speed, 3},
        {Name.AddSpeed, 3},
        {Name.DoubleSpeed, 1},
        {Name.AddXVelocity, 0.5},
        {Name.AddYVelocity, 0.5},
        {Name.ReduceXVelocity, 0.5},
        {Name.ReduceYVelocity, 0.5},
        {Name.AimClosestInSquareD6, 1},
        {Name.TriggerUponDeath, 1},
        {Name.VelocityZero, 0.5},
        {Name.Wait60Ticks, 1},
        {Name.AimMouse, 1},
        {Name.AimBack, 0.5},
        {Name.AimLeft, 0.5},
        {Name.AimRight, 0.5},
        {Name.AimUp, 0.5},
        {Name.AimDown, 0.5},
        {Name.DoubleCast, 1},
        {Name.TwiceCast, 1},
        {Name.CastEveryTick, 0.25},
        {Name.CastEvery8Ticks, 0.5},
        {Name.CastEvery64Ticks, 0.5},
        {Name.RandomAim, 1},
        {Name.RandomWait, 1},
        {Name.Aiming, 1},
        {Name.ScaleUp, 0.5},
        {Name.ScaleDown, 0.5},

    };
    public static readonly Dictionary<Name, int> SpellPrice = new(){
        {Name.Projectile1, 1},
        {Name.Stone, 3},
        {Name.Arrow, 2},
        {Name.Spike, 2},
        {Name.ExplosionSquareD6, 10},
        {Name.Add10Speed, 3},
        {Name.AddSpeed, 1},
        {Name.DoubleSpeed, 2},
        {Name.AddXVelocity, 1},
        {Name.AddYVelocity, 1},
        {Name.ReduceXVelocity, 1},
        {Name.ReduceYVelocity, 1},
        {Name.AimClosestInSquareD6, 2},
        {Name.TriggerUponDeath, 4},
        {Name.Wait60Ticks, 8},
        {Name.VelocityZero, 1},
        {Name.AimMouse, 2},
        {Name.AimBack, 1},
        {Name.AimLeft, 1},
        {Name.AimRight, 1},
        {Name.AimUp, 1},
        {Name.AimDown, 1},
        {Name.DoubleCast, 1},
        {Name.TwiceCast, 5},
        {Name.CastEveryTick, 20},
        {Name.CastEvery8Ticks, 10},
        {Name.CastEvery64Ticks, 5},
        {Name.RandomAim, 1},
        {Name.RandomWait, 4},
        {Name.Aiming, 3},
        {Name.ScaleUp, 20},
        {Name.ScaleDown, 20},
    };
    public static readonly Dictionary<Name, float> SpellCost = new(){
        {Name.Projectile1, 30},
        {Name.Stone, 100},
        {Name.Arrow, 80},
        {Name.Spike, 80},
        {Name.ExplosionSquareD6, 1000},
        {Name.Add10Speed, 50},
        {Name.AddSpeed, 10},
        {Name.DoubleSpeed, 80},
        {Name.AddXVelocity, 10},
        {Name.AddYVelocity, 10},
        {Name.ReduceXVelocity, 10},
        {Name.ReduceYVelocity, 10},
        {Name.AimClosestInSquareD6, 5},
        {Name.TriggerUponDeath, 50},
        {Name.Wait60Ticks, 50},
        {Name.VelocityZero, 0},
        {Name.AimMouse, 15},
        {Name.AimBack, 2},
        {Name.AimLeft, 2},
        {Name.AimRight, 2},
        {Name.AimUp, 2},
        {Name.AimDown, 2},
        {Name.DoubleCast, 20},
        {Name.TwiceCast, 50},
        {Name.CastEveryTick, 200},
        {Name.CastEvery8Ticks, 150},
        {Name.CastEvery64Ticks, 100},
        {Name.RandomAim, 0},
        {Name.RandomWait, 20},
        {Name.Aiming, 10},
        {Name.ScaleUp, 50},
        {Name.ScaleDown, 25},
    };
    public static readonly Dictionary<(Name,int), float> ManaMul = new(){
        {(Name.Projectile1, 1), 1.2f},
        {(Name.Stone, 1), 5.0f},
        {(Name.Arrow, 1), 3.0f},
        {(Name.Spike, 1), 3.0f},
        {(Name.ExplosionSquareD6, 1), 1.5f},
        // {(Name.Add10Speed, 50},
        {(Name.AddSpeed, 0), 1.0f},
        {(Name.DoubleSpeed, 0), 5.0f},
        {(Name.AddXVelocity, 0), 1.0f},
        {(Name.AddYVelocity, 0), 1.0f},
        {(Name.ReduceXVelocity, 0), 1.0f},
        {(Name.ReduceYVelocity, 0), 1.0f},
        {(Name.AimClosestInSquareD6, 0), 1.2f},
        {(Name.TriggerUponDeath, 0), 1.5f},
        {(Name.Wait60Ticks, 0), 1.5f},
        {(Name.VelocityZero, 0),  1.0f},
        {(Name.AimMouse, 0), 2.0f},
        {(Name.AimBack, 0), 1.0f},
        {(Name.AimLeft, 0), 1.0f},
        {(Name.AimRight, 0), 1.0f},
        {(Name.AimUp, 0), 1.0f},
        {(Name.AimDown, 0), 1.0f},
        {(Name.DoubleCast, 0), 1.0f},
        {(Name.DoubleCast, 1), 1.0f},
        {(Name.TwiceCast, 0), 1.5f},
        {(Name.CastEveryTick, 0), 3.0f},
        {(Name.CastEvery8Ticks, 0), 2.0f},
        {(Name.CastEvery64Ticks, 0), 1.5f},
        {(Name.RandomAim, 0), 1.0f},
        {(Name.RandomWait, 0), 1.0f},
        {(Name.Aiming, 0), 1.3f},
        {(Name.ScaleUp, 0), 2.0f},
        {(Name.ScaleDown, 0), 0.5f},
   };

    private Spell RandomNewSpell()
    {
        Spell spell = NewSpell(RandomSpellName.Next(), RandomProjectileName.Next());
        return spell;
    }
    private void BattleEnd(bool win)
    {
        if(win)
        {
            ++wave;
            if(stage==4)
            {
                Ending();
                return;
            }
            else if(wave>5)
            {
                wave = 1;
                ++stage;
                if(stage==4)
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

        if(SHOPALWAYSMAX) InitShop(24);
        else InitShop(6 + 1 * (wave - 1)+ 6 * (stage-1));

        shopWidth = 0;
        if(!shopOpen) ToggleShop();
        inventoryWidth = 0;
        if(!inventoryOpen) ToggleInventory();
        inventoryAvailable = true;

        enemyRate = stage switch{
            1 or 2 or 3 => wave switch{
                1 => 120,
                2 => 80,
                3 => 60,
                4 => 40,
                5 => 30,
                _ => throw new ArgumentOutOfRangeException(),
            },
            4 => 1,
            _ => throw new ArgumentOutOfRangeException(),
        };
        GenerateCardDeck();

        RefreshMap();
    }
    private void StageBegin()
    {
        switch(stage)
        {
            case 1:
                InitMap(5, 3, x => x>=10 && x%2 == 1, 512, Color.DimGray);
                break;
            case 2:
                InitMap(6, 4, x => x>=20, 1024, Color.DarkCyan);
                break;
            case 3:
                InitMap(7, 5, x => x>=30, 2048, Color.Purple);
                break;
            case 4:
                InitMap(9, 7, x => x>=50, 4096, Color.Gold);
                break;
        }
    }
    private void GameBegin()
    {
        stage = 1;
        wave = 1;
        spell = new();
        StageBegin();
        WaveBegin();
        InitInventory(24);
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
        InitTitle();
    }
    #region Update
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
        if (Keyboard.IsPressed(Keys.Escape))
            {if(_exitPower++>=10) Exit();}
        else _exitPower = 0;
        if (Keyboard.HasBeenPressed(Keys.F12))
        {
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();
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
            else if(mouseOn?.type == WindowType.StartBattle && gamescene == GameScene.Build)
            {
                BattleBegin();
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

        switch (gamescene)
        {
            case GameScene.Build or GameScene.Battle:
                if (Keyboard.HasBeenPressed(Keys.R))
                    _view = Matrix.CreateTranslation(width/2-doorCoor.X, height/2-doorCoor.Y, 0); // 恢复视角至初始状态
                if (gamescene == GameScene.Build && (Keyboard.IsPressed(Keys.LeftControl) || Keyboard.IsPressed(Keys.RightControl)) && Keyboard.HasBeenPressed(Keys.Enter))
                    BattleBegin();
                #region debug cheat
                if(CHEATALLOWED)
                {
                    if((Keyboard.IsPressed(Keys.LeftControl) || Keyboard.IsPressed(Keys.RightControl)) && Keyboard.HasBeenPressed(Keys.N))
                    {
                        BattleEnd(true);
                    }
                }
                #endregion 

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
                    // if(mouseOn == newGame)
                    // {
                    //     gamescene = GameScene.Loading;
                    //     _hasdrawn = false;
                    // }
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
    #endregion
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
        while(index < inventory.Count && inventory[index] != null) ++index;
        if(index < inventory.Count)
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
            for(int i=1;i<inventory.Count;++i)
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
            for(int i=1;i<shop.Count;++i)
            {
                if(shop[i] == null) continue;
                shop[i].showUI = false;
                shop[i].showLayer = 0;
            }
        }
    }
    #region draw
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

                // mana
                for(int x=0;x<xGrid;++x) for(int y=0;y<yGrid;++y) DrawWindow(manaWindow[x,y], new(x*64,y*64,64,64), new());

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
                foreach(Block b in blocks) foreach(Tower t in b.tower) if(t.spell != null) l.Add(t.spell.showLayer, (t.spell, t.Coordinate().ToPoint()-new Point(32,32)));
                foreach((Spell,Point) sv in l.Values) DrawSpellUI(sv.Item1, sv.Item2.X, sv.Item2.Y);

                
                if(!_predraw) _spriteBatch.End();



                if(!_predraw) _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _onMap = false;

                // 物品栏
                DrawWindow(inventoryWindow, new(shopWidth,0,inventoryWidth,height), null);
                DrawWindow(lifeWindow, new(shopWidth+inventoryWidth+20,height-128,216,44), null);
                DrawWindow(startBattle, new(shopWidth+inventoryWidth+20-256,height-128,216,44), null);
                
                // 物品栏法术
                for(int i=1;i<inventory.Count;++i) DrawWindow(inventorySlot[i], new(new Point(shopWidth+inventoryWidth-256,0)+InventoryOffset(i),new(64,64)), new(new Point(shopWidth+inventoryWidth-256+10,10)+InventoryOffset(i),new(44,44)));
                l = new SortedList<double, (Spell, Point)>(new DuplicateKeyComparer<double>());
                for(int i=1;i<inventory.Count;++i) if(inventory[i] != null) l.Add(inventory[i].showLayer, (inventory[i],new Point(shopWidth+inventoryWidth-256,0)+InventoryOffset(i)));
                foreach((Spell,Point) sv in l.Values) DrawSpellUI(sv.Item1, sv.Item2.X, sv.Item2.Y);
                
                // 商店栏
                DrawWindow(shopWindow, new(0,0,shopWidth,height), null);
                DrawWindow(moneyWindow, new(shopWidth+20,height-64,216,44), null);

                // 商店栏法术
                for(int i=1;i<shop.Count;++i) DrawWindow(shopSlot[i], new(new Point(shopWidth-256,0)+ShopOffset(i),new(64,64)), new(new Point(shopWidth-256+10,10)+ShopOffset(i),new(44,44)));
                l = new SortedList<double, (Spell, Point)>(new DuplicateKeyComparer<double>());
                for(int i=1;i<shop.Count;++i) if(shop[i] != null) l.Add(shop[i].showLayer, (shop[i],new Point(shopWidth-256,0)+ShopOffset(i)));
                foreach((Spell,Point) sv in l.Values) DrawSpellUI(sv.Item1, sv.Item2.X, sv.Item2.Y);

                // 右上文字
                DrawStringWindowRightTop(stageWave, new(width-12, 21), mouseCatch: false);
                DrawStringWindowRightTop(gamespeed, new(width-12, 54), mouseCatch: false);
                DrawStringWindowRightTop(paused, new(width-12, 87), mouseCatch: false);

                // 鼠标上的法术
                if(!_predraw) if(inventory[0] != null) DrawWindow(inventory[0].windowIcon, new(Mouse.Pos().ToPoint(),new(36,36)), new());

                if(!_predraw) _spriteBatch.End();

                
                break;
            case GameScene.Title:

                if(!_predraw) _spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, effect: _mapShader, samplerState: SamplerState.PointClamp);
                if(_predraw)
                {
                    LeftTop = Vector2.Transform(new Vector2(0,0), Matrix.Invert(_view));
                    RightBottom = Vector2.Transform(new Vector2(width,height), Matrix.Invert(_view));
                    xPeriod = 1340;
                    yPeriod = 1080*2;
                    projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);
                    _mapShader.Parameters["view_projection"].SetValue(_view * projection);
                }
                _onMap = true;
                DrawStringWindow(title, new(0,-50));
                DrawStringWindow(fullscreen, new(0,+100));
                DrawStringWindow(rightmouse, new(0,+415));
                DrawStringWindow(leftmouse, new(0,+550));
                DrawStringWindow(space, new(0,+1000));
                DrawStringWindow(numbers, new(0,+1000+48));
                DrawStringWindow(newgame, new(100,760+4));
                DrawStringWindow(quit, new(66,888+4));


                // 实体
                foreach(Entity e in entities())  if(e.window.texture != null) DrawWindow(e.window, new(e.RenderCoordinate().ToPoint(), e.size.ToPoint()), new());

                foreach(Tower t in titleBlock.tower) DrawWindow(t.window, new((int)t.Coordinate().X-32,(int)t.Coordinate().Y-32,64,64), new((int)t.Coordinate().X-22,(int)t.Coordinate().Y-22,44,44));
                l = new SortedList<double, (Spell, Point)>(new DuplicateKeyComparer<double>());
                foreach(Tower t in titleBlock.tower) if(t.spell != null) l.Add(t.spell.showLayer, (t.spell, t.Coordinate().ToPoint()-new Point(32,32)));
                foreach((Spell,Point) sv in l.Values) DrawSpellUI(sv.Item1, sv.Item2.X, sv.Item2.Y);

                // DrawStringWindow(newGame, new(width/2,height/2+20));
                if(!_predraw) _spriteBatch.End();

                if(!_predraw) _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _onMap = false;

                DrawStringWindowRightTop(gamespeed, new(width-12, 54), mouseCatch: false);
                DrawStringWindowRightTop(paused, new(width-12, 87), mouseCatch: false);

                // 鼠标上的法术
                if(!_predraw) if(inventory[0] != null) DrawWindow(inventory[0].windowIcon, new(Mouse.Pos().ToPoint(),new(36,36)), new());

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
    #endregion
    protected Point InventoryOffset(int i)
    {
        const int SPACING = 32;
        return new(SPACING+(i-1)%3*64,SPACING+(i-1)/3*64);
    }
    protected Point ShopOffset(int i)
    {
        const int SPACING = 32;
        const int PRICESPACING = 32;
        return new(SPACING+(i-1)%3*64,SPACING+(i-1)/3*(64+PRICESPACING));
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
        w.Update();
        DrawWindow(w, new(position - (_font.MeasureString(w.text) * w.textScale / 2).ToPoint(), (_font.MeasureString(w.text)*w.textScale).ToPoint()), mouseCatch ? null : new());
    }
    protected void DrawStringWindowRightTop(Window w, Point position, bool mouseCatch = true)
    {
        w.Update();
        DrawWindow(w, new(new(position.X - (_font.MeasureString(w.text) * w.textScale).ToPoint().X,position.Y), (_font.MeasureString(w.text)*w.textScale).ToPoint()), mouseCatch ? null : new());
    }
    protected void DrawWindow(Window w, Rectangle RectRender, Rectangle? RectMouseCatch)
    {
        w.Update();
        Rectangle rectMouseCatch = RectMouseCatch??RectRender;

        Vector2 LeftTop = this.LeftTop - new Vector2(64,64);
        Vector2 RightBottom = this.RightBottom + new Vector2(64,64);

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
        _view *= Matrix.CreateTranslation(-GraphicsDevice.Viewport.Width/2,-GraphicsDevice.Viewport.Height/2,0);

        bool oldIsFullscreen = _isFullscreen;

        _isBorderless = !_isBorderless;
        _isFullscreen = _isBorderless;

        ApplyFullscreenChange(oldIsFullscreen);

        _view *= Matrix.CreateTranslation(GraphicsDevice.Viewport.Width/2,GraphicsDevice.Viewport.Height/2,0);
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

    public void Shuffle<T>(IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rand.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }

    }
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
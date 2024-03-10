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
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

public enum GameScene {Title, Build, Battle, Options}
public enum GameStatus {Paused, Running};
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Effect _mapShader;
    // private Effect _guiShader;
    private Matrix view = Matrix.Identity;
    private Matrix projection = Matrix.CreateOrthographicOffCenter(0, 800, 600, 0, 0, 1);
    public Vector2 MouseCoor = new();
    public int MouseI = 0, MouseJ = 0;
    public long tick = 0; // 游戏从开始经过的刻数
    private long thingCount = 0; // 游戏从开始产生的Entity, Spell, Spellcast总数
    private double timeBank = 0d;
    private GameStatus gamestatus = GameStatus.Running; // 是不是暂停
    private GameScene gamescene = GameScene.Title;
    private int tps = 60; // 每秒多少刻（控制倍速，60刻是一倍速）
    private Dictionary<long, Entity> entities = new(); // 十分关键的字典，其中每个实体都有一个唯一的id
    private Dictionary<long, Spell> spells = new(); // 十分关键的字典，其中每个spell都有一个唯一的id
    private Dictionary<long, Spellcast> spellcasts = new(); // 十分关键的字典，其中每个spellcast都有一个唯一的id
    private Block[,] blocks;
    public Segment Reddoor;
    public Segment Bluedoor;
    private const int maxI = 32;
    private const int maxJ = 20;
    // private bool[,] isLight = new bool[maxI,maxJ];
    // public Spell[,] spellAt = new Spell[maxI,maxJ];
    private Window mouseOn = null;
    private Spell holding = null;
    private Attachment oldAtt = null;
    public Spell[] desk = new Spell[1];
    private Window newGame;
    private Window title;
    private Spell summonenemy1;
    private Spell summonenemyEasy;
    private Spell summonenemyFast;
    private Spell summonenemyVeryFast;
    public static Texture2D towerGUI;



    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            GraphicsProfile = GraphicsProfile.HiDef
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // ToggleBorderless(); // 先全屏 // 但是全屏不方便debug所以先关掉了



        // for(int i=0;i<maxI;++i) for(int j=0;j<maxJ;++j)
        // {
        //     isLight[i,j] = RandomNumberGenerator.GetInt32(2)>0;
        //     // (i+j)%2==0;
        // }

        title = new Window(this, WindowType.Title, Content.Load<Texture2D>("untitled"),false);
        newGame = new Window(this, WindowType.NewGame, Content.Load<Texture2D>("newGame"),true);
        towerGUI = Content.Load<Texture2D>("towergui");


        #region blocks
        blocks = new Block[Block.numX,Block.numY];
        for(int x=0;x<Block.numX;++x) for(int y=0;y<Block.numY;++y)
            blocks[x,y] = new(RandomBlockName(), x,y);
        // blocks = new Block[Block.numX,Block.numY]{
        //     {new(RandomBlockName(), 0,0), new(RandomBlockName(), 0,1), new(RandomBlockName(), 0,2)},
        //     {new(RandomBlockName(), 1,0), new(RandomBlockName(), 1,1), new(RandomBlockName(), 1,2)},
        //     {new(RandomBlockName(), 2,0), new(RandomBlockName(), 2,1), new(RandomBlockName(), 2,2)},
        //     {new(RandomBlockName(), 3,0), new(RandomBlockName(), 3,1), new(RandomBlockName(), 3,2)}
        //     };


        #endregion


        base.Initialize();
    }

    private BlockName RandomBlockName(){
        return RandomNumberGenerator.GetInt32(3) switch
        {
            0 or 1 => BlockName.A,
            2 => BlockName.B,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    protected void TickZero()
    {        
        // 在这里尝试这些法术的效果，可以随意修改
        #region sandbox
        (summonenemy1 = NewSpell(Name.SummonEnemy)).summonedEnemy = Name.Enemy1;
        (summonenemyEasy = NewSpell(Name.SummonEnemy)).summonedEnemy = Name.EnemyEasy;
        (summonenemyFast = NewSpell(Name.SummonEnemy)).summonedEnemy = Name.EnemyFast;
        (summonenemyVeryFast = NewSpell(Name.SummonEnemy)).summonedEnemy = Name.EnemyVeryFast;
        // Spell e0 = NewSpell(Name.SummonEnemy1);
        // Spell e1 = NewSpell(Name.AddXVelocity);
        // e0.ReAttach(new Attachment(blocks[0,0].tower[0]));
        // e1.ReAttach(new Attachment(e0,1));
        Spell s0 = NewSpell(Name.SummonProjectile1);
        Spell s1 = NewSpell(Name.AimClosestInSquareD6);
        Spell s2 = NewSpell(Name.Add10Speed);
        s0.ReAttach(new Attachment(blocks[0,1].tower[0]));
        s1.ReAttach(new Attachment(s0,1));
        s2.ReAttach(new Attachment(s1,0));
        Spell t0 = NewSpell(Name.SummonProjectile1);
        Spell t1 = NewSpell(Name.AimClosestInSquareD6);
        Spell t2 = NewSpell(Name.Add10Speed);
        t0.ReAttach(new Attachment(blocks[1,0].tower[0]));
        t1.ReAttach(new Attachment(t0,1));
        t2.ReAttach(new Attachment(t1,0));
        Spell u0 = NewSpell(Name.SummonProjectile1);
        Spell u1 = NewSpell(Name.AimClosestInSquareD6);
        Spell u2 = NewSpell(Name.Add10Speed);
        u0.ReAttach(new Attachment(blocks[1,1].tower[0]));
        u1.ReAttach(new Attachment(u0,1));
        u2.ReAttach(new Attachment(u1,0));
        // NewSpell(Name.AddSpeed).ReAttach(new Attachment(0,0,60));
        // NewSpell(Name.Add5Speed).ReAttach(new Attachment(1,0,60));
        // NewSpell(Name.AddXVelocity).ReAttach(new Attachment(2,0,60));
        // NewSpell(Name.AddYVelocity).ReAttach(new Attachment(3,0,60));
        // NewSpell(Name.TriggerUponDeath).ReAttach(new Attachment(4,0,60));
        // NewSpell(Name.Wait60Ticks).ReAttach(new Attachment(5,0,60));
        // NewSpell(Name.VelocityZero).ReAttach(new Attachment(6,0,60));
        #endregion
    }
    protected override void LoadContent() // 加载材质
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // _lightgrey = Content.Load<Texture2D>("lightgrey");
        // _darkgrey = Content.Load<Texture2D>("darkgrey");

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
        Entity.Texture[Name.SquareD6] = null;

        Spell.TextureIcon[Name.SummonEnemy] = Content.Load<Texture2D>("SummonEnemy1icon");
        Spell.TextureIcon[Name.SummonProjectile1] = Content.Load<Texture2D>("SummonProjectile1icon");
        Spell.TextureIcon[Name.VelocityZero] = Content.Load<Texture2D>("defaulticon");
        Spell.TextureIcon[Name.AddSpeed] = Content.Load<Texture2D>("addspeedicon");
        Spell.TextureIcon[Name.Add10Speed] = Content.Load<Texture2D>("add5speedicon");
        Spell.TextureIcon[Name.AddXVelocity] = Content.Load<Texture2D>("defaulticon");
        Spell.TextureIcon[Name.AddYVelocity] = Content.Load<Texture2D>("defaulticon");
        Spell.TextureIcon[Name.TriggerUponDeath] = Content.Load<Texture2D>("triggerupondeathicon");
        Spell.TextureIcon[Name.AimClosestInSquareD6] = Content.Load<Texture2D>("aimclosestinsquared6icon");
        Spell.TextureIcon[Name.Wait60Ticks] = Content.Load<Texture2D>("wait60ticksicon");

        Spell.TextureUI[Name.SummonEnemy] = Content.Load<Texture2D>("SpellGUI2");
        Spell.TextureUI[Name.SummonProjectile1] = Content.Load<Texture2D>("SpellGUI2");
        Spell.TextureUI[Name.VelocityZero] = Content.Load<Texture2D>("SpellGUI1");
        Spell.TextureUI[Name.AddSpeed] = Content.Load<Texture2D>("SpellGUI1");
        Spell.TextureUI[Name.Add10Speed] = Content.Load<Texture2D>("SpellGUI1");
        Spell.TextureUI[Name.AddXVelocity] = Content.Load<Texture2D>("SpellGUI1");
        Spell.TextureUI[Name.AddYVelocity] = Content.Load<Texture2D>("SpellGUI1");
        Spell.TextureUI[Name.TriggerUponDeath] = Content.Load<Texture2D>("SpellGUI1");
        Spell.TextureUI[Name.AimClosestInSquareD6] = Content.Load<Texture2D>("SpellGUI1");
        Spell.TextureUI[Name.Wait60Ticks] = Content.Load<Texture2D>("SpellGUI1");

        Spell.TextureSlot[(Name.SummonEnemy,0)] = Content.Load<Texture2D>("spellgui2slot0");
        Spell.TextureSlot[(Name.SummonEnemy,1)] = Content.Load<Texture2D>("spellgui2slot1");
        Spell.TextureSlot[(Name.SummonProjectile1,0)] = Content.Load<Texture2D>("spellgui2slot0");
        Spell.TextureSlot[(Name.SummonProjectile1,1)] = Content.Load<Texture2D>("spellgui2slot1");
        Spell.TextureSlot[(Name.VelocityZero,0)] = Content.Load<Texture2D>("spellgui1slot0");
        Spell.TextureSlot[(Name.AddSpeed,0)] = Content.Load<Texture2D>("spellgui1slot0");
        Spell.TextureSlot[(Name.Add10Speed,0)] = Content.Load<Texture2D>("spellgui1slot0");
        Spell.TextureSlot[(Name.AddXVelocity,0)] = Content.Load<Texture2D>("spellgui1slot0");
        Spell.TextureSlot[(Name.AddYVelocity,0)] = Content.Load<Texture2D>("spellgui1slot0");
        Spell.TextureSlot[(Name.TriggerUponDeath,0)] = Content.Load<Texture2D>("spellgui1slot0");
        Spell.TextureSlot[(Name.AimClosestInSquareD6,0)] = Content.Load<Texture2D>("spellgui1slot0");
        Spell.TextureSlot[(Name.Wait60Ticks,0)] = Content.Load<Texture2D>("spellgui1slot0");

        _mapShader = Content.Load<Effect>("map-shader");
    }



    public Enemy NewEnemy(Name name, Segment segment, float progress)
    {
        Enemy e = (Enemy)(entities[thingCount] = new Enemy(this, thingCount, name, segment, progress));
        ++thingCount;
        return e;
    }
    public Projectile NewProjectile(Name name, Vector2 coordinate, Vector2 velocity)
    {
        Projectile p = (Projectile)(entities[thingCount] = new Projectile(this, thingCount, name, coordinate, velocity));
        ++thingCount;
        return p;
    }
    public Spell NewSpell(Name name)
    {
        Spell s = spells[thingCount] = new Spell(this, thingCount, name);
        ++thingCount;
        return s;
    }
    public Spellcast NewSpellcast(Spell spell, Cast cast)
    {
        Spellcast sc = spellcasts[thingCount] = new Spellcast(this, thingCount, spell, cast);
        ++thingCount;
        return sc;
    }

    public ArrayList Collisions(Entity e) // 简单的碰撞判定算法。之后可能会出现圆形的东西，从而需要修改。另外以后算法上可能会需要优化。
    {
        ArrayList ans = new();
        foreach(Entity f in entities.Values)
        {
            if(f!=e && f.Hitbox().IntersectsWith(e.Hitbox()))
            {
                ans.Add(f);
            }
        }
        return ans;
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
    public Segment RefindPath(Block block, int doorout)
    {
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
            // Debug.Print(new Vector2(b.x,b.y).ToString());
            // Debug.Print(s.doorin.ToString());
        } while(b != block || d != doorout);
        last.succ = first;
        return last;
    }
    public void Penetrated(int i)
    {
        Debug.Print("penetrated: " + i.ToString());
    }

    protected void TickUpdate() // 游戏内每刻更新（暂停时不会调用，倍速时会更频繁调用），这里主要负责核心内部机制的计算
    {
        if(tick==0) TickZero();
        // if(RandomNumberGenerator.GetInt32(60) == 0) NewSpellcast(summonenemy1, new Cast(new Vector2()));
        if(RandomNumberGenerator.GetInt32(120) == 0) NewSpellcast(summonenemyEasy, new Cast(new Vector2()));
        // if(RandomNumberGenerator.GetInt32(240) == 0) NewSpellcast(summonenemyFast, new Cast(new Vector2()));
        // if(RandomNumberGenerator.GetInt32(480) == 0) NewSpellcast(summonenemyVeryFast, new Cast(new Vector2()));
        
        // 修改这里的顺序前务必仔细思考，否则可能会出现意想不到的情况
        foreach(Spell s in spells.Values)
            s.TickCast(); // 待施放的法术进行施放
        foreach(Spellcast sc in spellcasts.Values)
            sc.TickUpdate(); // 被施法术更新
        foreach(Entity e in entities.Values)
            if(!e.alive) entities.Remove(e.id); // 移除被标记为死亡的实体
        foreach(Entity e in entities.Values)
            e.TickUpdateCoordinate(); // 实体移动
        foreach(Entity e in entities.Values)
            e.TickUpdate(); // 实体更新（期间不应该移动！）
        foreach(Spellcast sc in spellcasts.Values)
            if(!sc.alive) spellcasts.Remove(sc.id); // 移除被标记为死亡的Spellcast
        foreach(Spell s in spells.Values)
            s.TickUpdate(); // 法术更新（其实只有地图上的法术会发生变化）
        ++tick;

        // Debug.Print(tick.ToString());
        // Debug.Print(spellcasts.Count.ToString());
        // Debug.Print(entities.Count.ToString());
    }



    protected override void Update(GameTime gameTime) // 窗口每帧更新（和暂停或倍速无关），这里主要负责一些输入输出的计算
    {
        Keyboard.GetState();
        Mouse.GetState();
        MouseCoor = Vector2.Transform(new Vector2(Mouse.X(), Mouse.Y()), Matrix.Invert(view));
        MouseI = (int)MathF.Floor(MouseCoor.X / 64f);
        MouseJ = (int)MathF.Floor(MouseCoor.Y / 64f);
        if (Keyboard.HasBeenPressed(Keys.Escape))
            ToggleBorderless();
        if (Keyboard.HasBeenPressed(Keys.Q))
            Exit();
        switch (gamescene)
        {
            case GameScene.Build or GameScene.Battle:
                if (Keyboard.HasBeenPressed(Keys.R))
                    view = Matrix.Identity; // 恢复视角至初始状态
                if (Keyboard.HasBeenPressed(Keys.T) && gamestatus == GameStatus.Paused)
                    TickUpdate(); // 暂停状态下，按一次T增加一刻
                // if (Keyboard.HasBeenPressed(Keys.D))
                // if (tick > 0)
                Bluedoor = RefindPath(Blocks(2,2),6);
                Reddoor = Bluedoor.succ;

                // 这部分是鼠标滚轮缩放
                #region zoom
                // Debug.Print(new Vector2(Mouse.X(), Mouse.Y()).ToString());
                Matrix newView = view * Matrix.CreateTranslation(-Mouse.X(),-Mouse.Y(),0) * Matrix.CreateScale((float)System.Math.Pow(1.1f,Mouse.Scroll()/120f)) * Matrix.CreateTranslation(Mouse.X(),Mouse.Y(),0);
                Vector3 scale; Vector3 translation;
                newView.Decompose(out scale, out _, out translation);

                // if (scale.X<0.95f && scale.X>0.52f) view =  newView;
                // else if (scale.X<1.05f && scale.X>0.52f) view = Matrix.CreateTranslation(new Vector3(MathF.Round(translation.X),MathF.Round(translation.Y),MathF.Round(translation.Z)));
                // else if (scale.X<0.95f && scale.X>0.48f) view = Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(new Vector3(MathF.Round(translation.X),MathF.Round(translation.Y),MathF.Round(translation.Z)));

                if (scale.X<0.95f) view =  newView;
                else if (scale.X<1.05f) view = Matrix.CreateTranslation(new Vector3(MathF.Round(translation.X),MathF.Round(translation.Y),MathF.Round(translation.Z)));
                
                #endregion

                #region tickupdate
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
                
                #region UI
                // Spell s = (0 <= MouseI && MouseI < maxI && 0 <= MouseJ && MouseJ < maxJ) ? spellAt[MouseI, MouseJ] : null;
                // if(mouseOn is Spell) s = (Spell)mouseOn;
                if(Mouse.LeftClicked())
                {
                    if(mouseOn?.parent is Spell)
                    {
                        ((Spell)mouseOn.parent).showUI ^= true;
                        if(mouseOn.type == WindowType.SpellIcon)
                        {
                            holding = (Spell)mouseOn.parent;
                        }
                    }
                }
                if(Mouse.LeftDeClicked())
                {
                    if(desk[0] != null)
                    {
                        if(mouseOn?.parent is Spell && mouseOn.type == WindowType.SpellSlot)
                        {
                            if(((Spell)mouseOn.parent).children[mouseOn.rank] == null)
                                desk[0].ReAttach(new Attachment((Spell)mouseOn.parent, mouseOn.rank));
                        }
                        else if (mouseOn?.parent is Tower)
                        {
                            if(((Tower)mouseOn.parent).spell == null)
                                desk[0].ReAttach(new Attachment((Tower)mouseOn.parent));
                        }
                        else
                        {
                            desk[0].ReAttach(oldAtt);
                        }
                    }
                }
                if(Mouse.LeftDown() && Mouse.FirstMovementSinceLastLeftClick())
                {
                    // Debug.Print("First move!");
                    if(holding != null && desk[0] == null)
                    {
                        oldAtt = holding.ReAttach(new Attachment(0));
                    }
                }
                #endregion
                break;
            case GameScene.Title:
                if(Mouse.LeftClicked())
                {
                    if(mouseOn == newGame)
                    {
                        gamescene = GameScene.Build;
                        view = Matrix.Identity;
                    }
                }
                break;
        }
        base.Update(gameTime);
    }



    protected override void Draw(GameTime gameTime) // 显示
    {
        int width = GraphicsDevice.Viewport.Width;
        int height = GraphicsDevice.Viewport.Height;
        PreDraw();
        switch(gamescene)
        {
            case GameScene.Build or GameScene.Battle:
                GraphicsDevice.Clear(Color.Black); // 背景是黑的

                projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);
                _mapShader.Parameters["view_projection"].SetValue(view * projection);

                _spriteBatch.Begin(effect: _mapShader);

                // for(int i=0;i<maxI;++i)for(int j=0;j<maxJ;++j) // 画地图
                // {
                //     _spriteBatch.Draw(isLight[i,j] ? _lightgrey : _darkgrey, new Vector2(i*64, j*64), Color.White);
                // }
                foreach(Block b in blocks)
                {
                    _spriteBatch.Draw(Block.Texture[b.name], b.Coordinate(), Color.White);
                    foreach(Road r in b.road)
                        _spriteBatch.Draw(Road.Texture[r.name], b.Coordinate(), Color.White * (r.isPath ? 0.5f : 0.2f));
                }
                foreach(Entity e in entities.Values) // 画实体
                {
                    if(e.RenderTexture()!=null) _spriteBatch.Draw(e.RenderTexture(), e.RenderCoordinate(), Color.White * (float)(0.25+0.75*e.health/e.maxhealth));
                }
                foreach(Block b in blocks) foreach(Tower t in b.tower)
                {
                    DrawWindow(false, MouseCoor, t.window, Color.White);
                }
                foreach(Spell s in spells.Values) // 画法术的UI
                {
                    if(s.attachment.type == Attachment.Type.Tower && !s.showUI) DrawSpellUI(false, s, s.attachment.tower.MapI(), s.attachment.tower.MapJ());
                }
                foreach(Spell s in spells.Values)
                {
                    if(s.attachment.type == Attachment.Type.Tower && s.showUI) DrawSpellUI(false, s, s.attachment.tower.MapI(), s.attachment.tower.MapJ());
                }
                if(desk[0] != null) _spriteBatch.Draw(Spell.TextureIcon[desk[0].name], MouseCoor, Color.Yellow);

                _spriteBatch.End();
                break;
            case GameScene.Title:
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                // _spriteBatch.Draw(_untitled, new((width-192*4)/2,(height-7*4)/2-50,192*4,7*4), Color.White);
                DrawWindow(false, Mouse.Pos(), title, Color.White);
                DrawWindow(false, Mouse.Pos(), newGame, Color.White);
                _spriteBatch.End();
                break;
        }
        base.Draw(gameTime);
    }

    protected void PreDraw() // 显示前需要先获取鼠标状态
    {
        mouseOn = null;
        int width = GraphicsDevice.Viewport.Width;
        int height = GraphicsDevice.Viewport.Height;
        switch(gamescene)
        {
            case GameScene.Build or GameScene.Battle:
                foreach(Block b in blocks) foreach(Tower t in b.tower)
                {
                    Window w = t.window;
                    w.RectRender = w.RectMouseCatch = new((int)t.Coordinate().X-22,(int)t.Coordinate().Y-22,44,44);
                    DrawWindow(true, MouseCoor, w, Color.White);
                }
                foreach(Spell s in spells.Values)
                {
                    if(s.attachment.type == Attachment.Type.Tower && !s.showUI) DrawSpellUI(true, s, s.attachment.tower.MapI(), s.attachment.tower.MapJ());
                }
                foreach(Spell s in spells.Values)
                {
                    if(s.attachment.type == Attachment.Type.Tower && s.showUI) DrawSpellUI(true, s, s.attachment.tower.MapI(), s.attachment.tower.MapJ());
                }
                break;
            case GameScene.Title:
                newGame.RectRender = new((width-68*2)/2,(height-7*2)/2+20,68*2,7*2);
                newGame.RectMouseCatch = new((width-68*2)/2-10,(height-7*2)/2+20-10,68*2+20,7*2+20);
                title.RectRender = title.RectMouseCatch = new((width-192*4)/2,(height-7*4)/2-50,192*4,7*4);
                DrawWindow(true, Mouse.Pos(), title, Color.White);
                DrawWindow(true, Mouse.Pos(), newGame, Color.White);
                break;
        }
    }
    protected void DrawSpellUI(bool preDraw, Spell s, int i, int j)
    {
        if(preDraw)
        {
            s.windowIcon.RectMouseCatch = s.windowIcon.RectRender = new Rectangle(i*64+14, j*64+14, 36, 36); 
            s.windowUI.RectMouseCatch = s.windowUI.RectRender = new Rectangle(i*64, j*64, s.windowUI.texture.Width, s.windowUI.texture.Height);
        }

        if(!s.showUI)
        {
            DrawWindow(preDraw, MouseCoor, s.windowIcon, Color.White);
        }
        else
        {
            DrawWindow(preDraw, MouseCoor, s.windowUI, Color.White);
            DrawWindow(preDraw, MouseCoor, s.windowIcon, Color.White);
            switch(Spell.childrenNumber[s.name])
            {
                case 1:
                {
                    if(preDraw)
                    {
                        s.windowSlots[0].RectRender = new Rectangle(i*64, j*64, s.windowSlots[0].texture.Width, s.windowSlots[0].texture.Height);
                        s.windowSlots[0].RectMouseCatch = new Rectangle(i*64+10, (j+1)*64+10, 44, 44);
                    }
                    DrawWindow(preDraw, MouseCoor, s.windowSlots[0], Color.Aqua);
                    if(s.children[0] != null) DrawSpellUI(preDraw, s.children[0], i, j+1);

                    break;
                }
                case 2:
                {
                    if(preDraw)
                    {
                        s.windowSlots[0].RectRender = new Rectangle(i*64, j*64, s.windowSlots[0].texture.Width, s.windowSlots[0].texture.Height);
                        s.windowSlots[0].RectMouseCatch = new Rectangle(i*64+10, (j+2)*64+10, 44, 44);
                    }
                    DrawWindow(preDraw, MouseCoor, s.windowSlots[0], Color.Aqua);
                    if(s.children[0] != null) DrawSpellUI(preDraw, s.children[0], i, j+2);
                    
                    if(preDraw)
                    {
                        s.windowSlots[1].RectRender = new Rectangle(i*64, j*64, s.windowSlots[1].texture.Width, s.windowSlots[1].texture.Height);
                        s.windowSlots[1].RectMouseCatch = new Rectangle((i+1)*64+10, (j+1)*64+10, 44, 44);
                    }
                    DrawWindow(preDraw, MouseCoor, s.windowSlots[1], Color.BlueViolet);
                    if(s.children[1] != null) DrawSpellUI(preDraw, s.children[1], i+1, j+1);

                    break;
                }
            }
        }
    }
    protected void DrawWindow(bool PreDraw, Vector2 MouseCoor, Window w, Color color)
    {
        if(PreDraw)
        {
            if (w.RectMouseCatch.Contains(MouseCoor)) mouseOn = w;
        }
        else
        _spriteBatch.Draw(w.texture, w.RectRender, (w.clickable && mouseOn == w) ? Color.Yellow : color);
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

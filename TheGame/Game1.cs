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
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

public enum GameStatus {Paused, Running};
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _lightgrey;
    private Texture2D _darkgrey;
    private Effect _mapShader;
    private Matrix view = Matrix.Identity;
    private Matrix projection = Matrix.CreateOrthographicOffCenter(0, 800, 600, 0, 0, 1);
    public long tick = 0; // 游戏从开始经过的刻数
    private long thingCount = 0; // 游戏从开始产生的Entity, Spell, Spellcast总数
    private double timeBank = 0d;
    private GameStatus status = GameStatus.Running; // 是不是暂停
    private int tps = 60; // 每秒多少刻（控制倍速，60刻是一倍速）
    private Dictionary<long, Entity> entities = new(); // 十分关键的字典，其中每个实体都有一个唯一的id
    private Dictionary<long, Spell> spells = new(); // 十分关键的字典，其中每个spell都有一个唯一的id
    private Dictionary<long, Spellcast> spellcasts = new(); // 十分关键的字典，其中每个spellcast都有一个唯一的id
    private const int gridI = 32;
    private const int gridJ = 20;
    private bool[,] isLight = new bool[gridI,gridJ];



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


        // 在这里尝试这些法术的效果，可以随意修改
        #region sandbox
        Spell e0 = NewSpell(Name.SummonEnemy1);
        Spell e1 = NewSpell(Name.AddXVelocity);
        e0.AffiliateAsMap(0,5,100);
        e1.AffiliateAsChild(e0,1);
        Spell s0 = NewSpell(Name.SummonProjectile1);
        Spell s1 = NewSpell(Name.AimClosestInSquareD6);
        Spell s2 = NewSpell(Name.Add5Speed);
        s0.AffiliateAsMap(6,3,15);
        s1.AffiliateAsChild(s0,1);
        s2.AffiliateAsChild(s1,0);
        Spell t0 = NewSpell(Name.SummonProjectile1);
        Spell t1 = NewSpell(Name.AimClosestInSquareD6);
        Spell t2 = NewSpell(Name.Add5Speed);
        t0.AffiliateAsMap(9,7,15);
        t1.AffiliateAsChild(t0,1);
        t2.AffiliateAsChild(t1,0);
        Spell u0 = NewSpell(Name.SummonProjectile1);
        Spell u1 = NewSpell(Name.AimClosestInSquareD6);
        Spell u2 = NewSpell(Name.Add5Speed);
        u0.AffiliateAsMap(15,3,30);
        u1.AffiliateAsChild(u0,1);
        u2.AffiliateAsChild(u1,0);
        #endregion

        for(int i=0;i<gridI;++i) for(int j=0;j<gridJ;++j)
        {
            isLight[i,j] = RandomNumberGenerator.GetInt32(2)>0;
            // (i+j)%2==0;
        }

        base.Initialize();
    }

    protected override void LoadContent() // 加载材质
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _lightgrey = Content.Load<Texture2D>("lightgrey");
        _darkgrey = Content.Load<Texture2D>("darkgrey");
        Entity.Texture[Name.Enemy1] = Content.Load<Texture2D>("enemy1");
        Entity.Texture[Name.Projectile1] = Content.Load<Texture2D>("projectile1");
        Entity.Texture[Name.SquareD6] = null;

        _mapShader = Content.Load<Effect>("map-shader");
    }



    public Enemy NewEnemy(Name name, Vector2 coordinate, Vector2 velocity)
    {
        Enemy e = (Enemy)(entities[thingCount] = new Enemy(this, thingCount, name, coordinate, velocity));
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


    protected void TickUpdate() // 游戏内每刻更新（暂停时不会调用，倍速时会更频繁调用），这里主要负责核心内部机制的计算
    {
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

        if (Keyboard.HasBeenPressed(Keys.Escape))
            ToggleBorderless();
        if (Keyboard.HasBeenPressed(Keys.Q))
            Exit();
        if (Keyboard.HasBeenPressed(Keys.R))
            view = Matrix.Identity; // 恢复视角至初始状态
        if (Keyboard.HasBeenPressed(Keys.T) && status == GameStatus.Paused)
            TickUpdate(); // 暂停状态下，按一次T增加一刻

        // 这部分是鼠标滚轮缩放
        // Debug.Print(new Vector2(Mouse.X(), Mouse.Y()).ToString());
        Matrix newView = view * Matrix.CreateTranslation(-Mouse.X(),-Mouse.Y(),0) * Matrix.CreateScale((float)System.Math.Pow(1.1f,Mouse.Scroll()/120f)) * Matrix.CreateTranslation(Mouse.X(),Mouse.Y(),0);
        Vector3 scale; Vector3 translation;
        newView.Decompose(out scale, out _, out translation);
        if (scale.X<0.95f) view =  newView;
        else if (scale.X<1.05d) view = Matrix.CreateTranslation(new Vector3(MathF.Round(translation.X),MathF.Round(translation.Y),MathF.Round(translation.Z)));

        if (Keyboard.HasBeenPressed(Keys.Space))
            if (status == GameStatus.Paused) status = GameStatus.Running;
            else status = GameStatus.Paused;
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
        TimeBank += gameTime.ElapsedGameTime.TotalSeconds;
        if (status == GameStatus.Paused) TimeBank = 0d;
        else 
        {
            int TickUpdateMax = 10;
            while(TimeBank > 0d && TickUpdateMax > 0)
            {
                TickUpdate();
                --TickUpdateMax;
                TimeBank -= 1d / tps;
            }
        }
        // 采用的倍速使得如果游戏卡顿的话，卡顿结束后游戏会加速来补齐原来的时间流动，但不会超过十倍速

        base.Update(gameTime);
    }



    protected override void Draw(GameTime gameTime) // 显示
    {
        GraphicsDevice.Clear(Color.Black); // 背景是黑的

        projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
        _mapShader.Parameters["view_projection"].SetValue(view * projection);

        _spriteBatch.Begin(effect: _mapShader);

        for(int i=0;i<gridI;++i)for(int j=0;j<gridJ;++j) // 画地图
        {
            _spriteBatch.Draw(isLight[i,j] ? _lightgrey : _darkgrey, new Vector2(i*64, j*64), Color.White);
        }
        foreach(Entity e in entities.Values) // 画实体
        {
            if(e.RenderTexture()!=null) _spriteBatch.Draw(e.RenderTexture(), e.RenderCoordinate(), Color.White);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    // 下面是关于全屏显示的东西，不用管
    #region fullscreen

    bool _isFullscreen = false;
    bool _isBorderless = false;
    int _width = 0;
    int _height = 0;

    public global::System.Double TimeBank { get => TimeBank1; set => TimeBank1 = value; }
    public global::System.Double TimeBank1 { get => timeBank; set => timeBank = value; }

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

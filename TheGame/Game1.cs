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
using System.Reflection.Metadata;
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
    public Vector2 MouseCoor = new();
    public int MouseI = 0, MouseJ = 0;
    public long tick = 0; // 游戏从开始经过的刻数
    private long thingCount = 0; // 游戏从开始产生的Entity, Spell, Spellcast总数
    private double timeBank = 0d;
    private GameStatus status = GameStatus.Running; // 是不是暂停
    private int tps = 60; // 每秒多少刻（控制倍速，60刻是一倍速）
    private Dictionary<long, Entity> entities = new(); // 十分关键的字典，其中每个实体都有一个唯一的id
    private Dictionary<long, Spell> spells = new(); // 十分关键的字典，其中每个spell都有一个唯一的id
    private Dictionary<long, Spellcast> spellcasts = new(); // 十分关键的字典，其中每个spellcast都有一个唯一的id
    private const int maxI = 32;
    private const int maxJ = 20;
    private bool[,] isLight = new bool[maxI,maxJ];
    public Spell[,] spellAt = new Spell[maxI,maxJ];
    private Window mouseOn = null;
    private Spell holding = null;
    private Attachment oldAtt = null;
    public Spell[] desk = new Spell[1];



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



        for(int i=0;i<maxI;++i) for(int j=0;j<maxJ;++j)
        {
            isLight[i,j] = RandomNumberGenerator.GetInt32(2)>0;
            // (i+j)%2==0;
        }

        base.Initialize();
    }

    protected void TickZero()
    {
        // 在这里尝试这些法术的效果，可以随意修改
        #region sandbox
        Spell e0 = NewSpell(Name.SummonEnemy1);
        Spell e1 = NewSpell(Name.AddXVelocity);
        e0.ReAttach(new Attachment(0,5,100));
        e1.ReAttach(new Attachment(e0,1));
        Spell s0 = NewSpell(Name.SummonProjectile1);
        Spell s1 = NewSpell(Name.AimClosestInSquareD6);
        Spell s2 = NewSpell(Name.Add5Speed);
        s0.ReAttach(new Attachment(6,3,15));
        s1.ReAttach(new Attachment(s0,1));
        s2.ReAttach(new Attachment(s1,0));
        Spell t0 = NewSpell(Name.SummonProjectile1);
        Spell t1 = NewSpell(Name.AimClosestInSquareD6);
        Spell t2 = NewSpell(Name.Add5Speed);
        t0.ReAttach(new Attachment(9,7,15));
        t1.ReAttach(new Attachment(t0,1));
        t2.ReAttach(new Attachment(t1,0));
        Spell u0 = NewSpell(Name.SummonProjectile1);
        Spell u1 = NewSpell(Name.AimClosestInSquareD6);
        Spell u2 = NewSpell(Name.Add5Speed);
        u0.ReAttach(new Attachment(15,3,30));
        u1.ReAttach(new Attachment(u0,1));
        u2.ReAttach(new Attachment(u1,0));
        NewSpell(Name.AddSpeed).ReAttach(new Attachment(0,0,60));
        NewSpell(Name.Add5Speed).ReAttach(new Attachment(1,0,60));
        NewSpell(Name.AddXVelocity).ReAttach(new Attachment(2,0,60));
        NewSpell(Name.AddYVelocity).ReAttach(new Attachment(3,0,60));
        NewSpell(Name.TriggerUponDeath).ReAttach(new Attachment(4,0,60));
        NewSpell(Name.Wait60Ticks).ReAttach(new Attachment(5,0,60));
        NewSpell(Name.VelocityZero).ReAttach(new Attachment(6,0,60));
        #endregion
    }
    protected override void LoadContent() // 加载材质
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _lightgrey = Content.Load<Texture2D>("lightgrey");
        _darkgrey = Content.Load<Texture2D>("darkgrey");

        Entity.Texture[Name.Enemy1] = Content.Load<Texture2D>("enemy1");
        Entity.Texture[Name.Projectile1] = Content.Load<Texture2D>("projectile1");
        Entity.Texture[Name.SquareD6] = null;

        Spell.TextureIcon[Name.SummonEnemy1] = Content.Load<Texture2D>("SummonEnemy1icon");
        Spell.TextureIcon[Name.SummonProjectile1] = Content.Load<Texture2D>("SummonProjectile1icon");
        Spell.TextureIcon[Name.VelocityZero] = Content.Load<Texture2D>("defaulticon");
        Spell.TextureIcon[Name.AddSpeed] = Content.Load<Texture2D>("addspeedicon");
        Spell.TextureIcon[Name.Add5Speed] = Content.Load<Texture2D>("add5speedicon");
        Spell.TextureIcon[Name.AddXVelocity] = Content.Load<Texture2D>("defaulticon");
        Spell.TextureIcon[Name.AddYVelocity] = Content.Load<Texture2D>("defaulticon");
        Spell.TextureIcon[Name.TriggerUponDeath] = Content.Load<Texture2D>("triggerupondeathicon");
        Spell.TextureIcon[Name.AimClosestInSquareD6] = Content.Load<Texture2D>("aimclosestinsquared6icon");
        Spell.TextureIcon[Name.Wait60Ticks] = Content.Load<Texture2D>("wait60ticksicon");

        Spell.TextureUI[Name.SummonEnemy1] = Content.Load<Texture2D>("SpellGUI2");
        Spell.TextureUI[Name.SummonProjectile1] = Content.Load<Texture2D>("SpellGUI2");
        Spell.TextureUI[Name.VelocityZero] = Content.Load<Texture2D>("SpellGUI1");
        Spell.TextureUI[Name.AddSpeed] = Content.Load<Texture2D>("SpellGUI1");
        Spell.TextureUI[Name.Add5Speed] = Content.Load<Texture2D>("SpellGUI1");
        Spell.TextureUI[Name.AddXVelocity] = Content.Load<Texture2D>("SpellGUI1");
        Spell.TextureUI[Name.AddYVelocity] = Content.Load<Texture2D>("SpellGUI1");
        Spell.TextureUI[Name.TriggerUponDeath] = Content.Load<Texture2D>("SpellGUI1");
        Spell.TextureUI[Name.AimClosestInSquareD6] = Content.Load<Texture2D>("SpellGUI1");
        Spell.TextureUI[Name.Wait60Ticks] = Content.Load<Texture2D>("SpellGUI1");

        Spell.TextureSlot[(Name.SummonEnemy1,0)] = Content.Load<Texture2D>("spellgui2slot0");
        Spell.TextureSlot[(Name.SummonEnemy1,1)] = Content.Load<Texture2D>("spellgui2slot1");
        Spell.TextureSlot[(Name.SummonProjectile1,0)] = Content.Load<Texture2D>("spellgui2slot0");
        Spell.TextureSlot[(Name.SummonProjectile1,1)] = Content.Load<Texture2D>("spellgui2slot1");
        Spell.TextureSlot[(Name.VelocityZero,0)] = Content.Load<Texture2D>("spellgui1slot0");
        Spell.TextureSlot[(Name.AddSpeed,0)] = Content.Load<Texture2D>("spellgui1slot0");
        Spell.TextureSlot[(Name.Add5Speed,0)] = Content.Load<Texture2D>("spellgui1slot0");
        Spell.TextureSlot[(Name.AddXVelocity,0)] = Content.Load<Texture2D>("spellgui1slot0");
        Spell.TextureSlot[(Name.AddYVelocity,0)] = Content.Load<Texture2D>("spellgui1slot0");
        Spell.TextureSlot[(Name.TriggerUponDeath,0)] = Content.Load<Texture2D>("spellgui1slot0");
        Spell.TextureSlot[(Name.AimClosestInSquareD6,0)] = Content.Load<Texture2D>("spellgui1slot0");
        Spell.TextureSlot[(Name.Wait60Ticks,0)] = Content.Load<Texture2D>("spellgui1slot0");

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
        if(tick==0) TickZero();
        
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
        // Debug.Print(MousePos.ToString());

        if (Keyboard.HasBeenPressed(Keys.Escape))
            ToggleBorderless();
        if (Keyboard.HasBeenPressed(Keys.Q))
            Exit();
        if (Keyboard.HasBeenPressed(Keys.R))
            view = Matrix.Identity; // 恢复视角至初始状态
        if (Keyboard.HasBeenPressed(Keys.T) && status == GameStatus.Paused)
            TickUpdate(); // 暂停状态下，按一次T增加一刻

        // 这部分是鼠标滚轮缩放
        #region zoom
        // Debug.Print(new Vector2(Mouse.X(), Mouse.Y()).ToString());
        Matrix newView = view * Matrix.CreateTranslation(-Mouse.X(),-Mouse.Y(),0) * Matrix.CreateScale((float)System.Math.Pow(1.1f,Mouse.Scroll()/120f)) * Matrix.CreateTranslation(Mouse.X(),Mouse.Y(),0);
        Vector3 scale; Vector3 translation;
        newView.Decompose(out scale, out _, out translation);
        if (scale.X<0.95f) view =  newView;
        else if (scale.X<1.05d) view = Matrix.CreateTranslation(new Vector3(MathF.Round(translation.X),MathF.Round(translation.Y),MathF.Round(translation.Z)));
        #endregion

        #region tickupdate
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
            int TickUpdateMax = 10; // 采用的倍速机制使得如果游戏卡顿的话，卡顿结束后游戏会加速来补齐原来的时间流动，但不会超过十倍速
            while(TimeBank > 0d && TickUpdateMax > 0)
            {
                TickUpdate();
                --TickUpdateMax;
                TimeBank -= 1d / tps;
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
                if(mouseOn.type == TheGame.Window.Type.SpellIcon)
                {
                    holding = (Spell)mouseOn.parent;
                }
            }
        }
        if(Mouse.LeftDeClicked())
        {
            if(desk[0] != null)
            {
                if(mouseOn?.parent is Spell && mouseOn.type == TheGame.Window.Type.SpellSlot)
                {
                    if(((Spell)mouseOn.parent).children[mouseOn.rank] == null)
                        desk[0].ReAttach(new Attachment((Spell)mouseOn.parent, mouseOn.rank));
                }
                else
                {
                    desk[0].ReAttach(oldAtt);
                }
            }
        }
        if(Mouse.LeftDown() && Mouse.FirstMovementSinceLastLeftClick())
        {
            Debug.Print("First move!");
            if(holding != null && desk[0] == null)
            {
                oldAtt = holding.ReAttach(new Attachment(0));
            }
        }
        #endregion

        base.Update(gameTime);
    }



    protected override void Draw(GameTime gameTime) // 显示
    {
        PreDraw();
        GraphicsDevice.Clear(Color.Black); // 背景是黑的

        projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
        _mapShader.Parameters["view_projection"].SetValue(view * projection);

        _spriteBatch.Begin(effect: _mapShader);

        for(int i=0;i<maxI;++i)for(int j=0;j<maxJ;++j) // 画地图
        {
            _spriteBatch.Draw(isLight[i,j] ? _lightgrey : _darkgrey, new Vector2(i*64, j*64), Color.White);
        }
        foreach(Entity e in entities.Values) // 画实体
        {
            if(e.RenderTexture()!=null) _spriteBatch.Draw(e.RenderTexture(), e.RenderCoordinate(), Color.White);
        }
        foreach(Spell s in spells.Values) // 画法术的UI
        {
            if(s.attachment.type == Attachment.Type.Map) DrawSpellUI(false, s, s.attachment.mapI, s.attachment.mapJ);
        }
        if(desk[0] != null) _spriteBatch.Draw(Spell.TextureIcon[desk[0].name], MouseCoor, Color.Yellow);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected void PreDraw() // 显示前需要先获取鼠标状态
    {
        mouseOn = null;
        foreach(Spell s in spells.Values)
        {
            if(s.attachment.type == Attachment.Type.Map) DrawSpellUI(true, s, s.attachment.mapI, s.attachment.mapJ);
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
            DrawWindow(preDraw, s.windowIcon, Color.White);
        }
        else
        {
            DrawWindow(preDraw, s.windowUI, Color.White);
            DrawWindow(preDraw, s.windowIcon, Color.White);
            switch(Spell.childrenNumber[s.name])
            {
                case 1:
                {
                    if(preDraw)
                    {
                        s.windowSlots[0].RectRender = new Rectangle(i*64, j*64, s.windowSlots[0].texture.Width, s.windowSlots[0].texture.Height);
                        s.windowSlots[0].RectMouseCatch = new Rectangle(i*64+10, (j+1)*64+10, 44, 44);
                    }
                    DrawWindow(preDraw, s.windowSlots[0], Color.Aqua);
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
                    DrawWindow(preDraw, s.windowSlots[0], Color.Aqua);
                    if(s.children[0] != null) DrawSpellUI(preDraw, s.children[0], i, j+2);
                    
                    if(preDraw)
                    {
                        s.windowSlots[1].RectRender = new Rectangle(i*64, j*64, s.windowSlots[1].texture.Width, s.windowSlots[1].texture.Height);
                        s.windowSlots[1].RectMouseCatch = new Rectangle((i+1)*64+10, (j+1)*64+10, 44, 44);
                    }
                    DrawWindow(preDraw, s.windowSlots[1], Color.BlueViolet);
                    if(s.children[1] != null) DrawSpellUI(preDraw, s.children[1], i+1, j+1);

                    break;
                }
            }
        }
    }
    protected void DrawWindow(bool PreDraw, Window w, Color color)
    {
        if(PreDraw)
        {
            if (w.RectMouseCatch.Contains(MouseCoor)) mouseOn = w;
        }
        else
        _spriteBatch.Draw(w.texture, w.RectRender, mouseOn == w ? Color.Yellow : color);
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

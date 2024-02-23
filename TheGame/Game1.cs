using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TheGame;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _testimage;
    private Texture2D _lightgrey;
    private Texture2D _darkgrey;
    private Texture2D _enemy1;
    private Effect _mapShader;
    private Matrix view = Matrix.Identity;
    private Matrix projection = Matrix.CreateOrthographicOffCenter(0, 800, 600, 0, 0, 1);
    public long tick = 0;
    private double timeBank = 0d;
    private enum Status {Paused, Running};
    private Status status = Status.Running;
    private int tps = 60;
    public Dictionary<long, Entity> entities = new();
    public Dictionary<long, Spell> spells = new();
    public Dictionary<long, Spellcast> spellcasts = new();
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
        ToggleBorderless();
        entities[0] = new Enemy(this, Entity.Name.Enemy1, new Vector2(32,32+64), new Vector2(1,0));
        spells[0] = new Spell(this, Spell.Name.SummonEnemy1, Spell.Affiliation.Map, 60);

        for(int i=0;i<gridI;++i) for(int j=0;j<gridJ;++j)
        {
            isLight[i,j] = RandomNumberGenerator.GetInt32(2)>0;
            // (i+j)%2==0;
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _testimage = Content.Load<Texture2D>("testimage");
        _lightgrey = Content.Load<Texture2D>("lightgrey");
        _darkgrey = Content.Load<Texture2D>("darkgrey");
        _enemy1 = Content.Load<Texture2D>("enemy1");
        _mapShader = Content.Load<Effect>("map-shader");
    }

    protected void TickUpdate()
    {
        foreach(Entity e in entities.Values)
            e.TickUpdateVelocity();
        foreach(Entity e in entities.Values)
            e.TickUpdateCoordinate();
        foreach(Spellcast sc in spellcasts.Values)
            sc.TickUpdate(tick);
        foreach(Spell s in spells.Values)
            s.TickUpdate(tick);
        ++tick;
        // Debug.Print(tick.ToString());
        // Debug.Print(spellcasts.Count.ToString());
    }
    protected override void Update(GameTime gameTime)
    {
        Keyboard.GetState();
        Mouse.GetState();

        if (Keyboard.HasBeenPressed(Keys.Escape))
            ToggleBorderless();
        if (Keyboard.HasBeenPressed(Keys.Q))
            Exit();
        if (Keyboard.HasBeenPressed(Keys.R))
            view = Matrix.Identity;
        if (Keyboard.HasBeenPressed(Keys.T) && status == Status.Paused)
            TickUpdate();

        // Debug.Print(new Vector2(Mouse.X(), Mouse.Y()).ToString());
        Matrix newView = view * Matrix.CreateTranslation(-Mouse.X(),-Mouse.Y(),0) * Matrix.CreateScale((float)System.Math.Pow(1.1f,Mouse.Scroll()/120f)) * Matrix.CreateTranslation(Mouse.X(),Mouse.Y(),0);
        Vector3 scale; Vector3 translation;
        newView.Decompose(out scale, out _, out translation);
        if (scale.X<0.95f) view =  newView;
        else if (scale.X<1.05d) view = Matrix.CreateTranslation(new Vector3(MathF.Round(translation.X),MathF.Round(translation.Y),MathF.Round(translation.Z)));

        if (Keyboard.HasBeenPressed(Keys.Space))
            if (status == Status.Paused) status = Status.Running;
            else status = Status.Paused;
        if (Keyboard.HasBeenPressed(Keys.OemTilde))
            tps = 30;
        if (Keyboard.HasBeenPressed(Keys.D1))
            tps = 60;
        if (Keyboard.HasBeenPressed(Keys.D2))
            tps = 120;
        if (Keyboard.HasBeenPressed(Keys.D3))
            tps = 180;
        timeBank += gameTime.ElapsedGameTime.TotalSeconds;
        if (status == Status.Paused) timeBank = 0d;
        else while(timeBank > 0d)
        {
            TickUpdate();
            timeBank -= 1d / tps;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        int width = GraphicsDevice.Viewport.Width;
        int height = GraphicsDevice.Viewport.Height;
        projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);

        _mapShader.Parameters["view_projection"].SetValue(view * projection);

        _spriteBatch.Begin(effect: _mapShader);
        for(int i=0;i<gridI;++i)for(int j=0;j<gridJ;++j)
        {
            _spriteBatch.Draw(isLight[i,j] ? _lightgrey : _darkgrey, new Vector2(i*64, j*64), Color.White);
        }
        foreach(int id in entities.Keys)
        {
            _spriteBatch.Draw(_enemy1, entities[id].RenderCoordinate(), Color.White);
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }

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

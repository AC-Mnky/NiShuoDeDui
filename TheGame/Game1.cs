using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    private long tick = 0;
    private double timeBank = 0d;
    private enum Status {Paused = 0, Half = 30, Normal = 60, Double = 120, Triple = 180};
    private Status status = Status.Normal;
    private Status oldStatus = Status.Normal;
    private Dictionary<int, Entity> entities = new() {
        {1,new Entity(new Vector2(16,16))}
    };


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
        entities[1].coordinate.X += 1;
        ++tick;
        Debug.Print(tick.ToString());
    }
    protected override void Update(GameTime gameTime)
    {
        Keyboard.GetState();
        Mouse.GetState();

        if (Keyboard.HasBeenPressed(Keys.Escape))
            ToggleBorderless();
        if (Keyboard.HasBeenPressed(Keys.Q))
            Exit();
        if (Keyboard.HasBeenPressed(Keys.Space))
        {
            if (status == Status.Paused)
                status = oldStatus;
            else
            {
            oldStatus = status;
            status = Status.Paused;
            }
        }
        if (Keyboard.HasBeenPressed(Keys.OemTilde))
            status = Status.Half;
        if (Keyboard.HasBeenPressed(Keys.D1))
            status = Status.Normal;
        if (Keyboard.HasBeenPressed(Keys.D2))
            status = Status.Double;
        if (Keyboard.HasBeenPressed(Keys.D3))
            status = Status.Triple;
            // TickUpdate();

        // Debug.Print(Vector2.Transform(new Vector2(Mouse.X(), Mouse.Y()), Matrix.Invert(view * projection)).ToString());
        // Debug.Print(new Vector2(Mouse.X(), Mouse.Y()).ToString());
        view *=  Matrix.CreateTranslation(-Mouse.X(),-Mouse.Y(),0) * Matrix.CreateScale((float)System.Math.Pow(1.1f,Mouse.Scroll()/120f)) * Matrix.CreateTranslation(Mouse.X(),Mouse.Y(),0);

        timeBank += gameTime.ElapsedGameTime.TotalSeconds;
        if (status == Status.Paused) timeBank = 0d;
        else while(timeBank > 0d)
        {
            TickUpdate();
            timeBank -= 1d / (int)status;
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
        for(int i=0;i<10;++i)for(int j=0;j<10;++j)
        {
            _spriteBatch.Draw(((i+j)%2==0) ? _lightgrey : _darkgrey, new Vector2(i*64, j*64), Color.White);
        }
        foreach(int entityid in entities.Keys)
        {
            _spriteBatch.Draw(_enemy1, entities[entityid].coordinate, Color.White);
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

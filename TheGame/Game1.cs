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
    private Effect _mapShader;
    Matrix view = Matrix.Identity;
    Matrix projection = Matrix.CreateOrthographicOffCenter(0, 800, 600, 0, 0, 1);


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
    _mapShader = Content.Load<Effect>("map-shader");
}

    protected override void Update(GameTime gameTime)
    {
        Keyboard.GetState();
        Mouse.GetState();

        if (Keyboard.HasBeenPressed(Keys.Escape))
            ToggleBorderless();
            // Exit();

        Debug.Print(Vector2.Transform(new Vector2(Mouse.X(), Mouse.Y()), Matrix.Invert(view * projection)).ToString());
        Debug.Print(new Vector2(Mouse.X(), Mouse.Y()).ToString());
        view *=  Matrix.CreateTranslation(-Mouse.X(),-Mouse.Y(),0) * Matrix.CreateScale((float)System.Math.Pow(1.1f,Mouse.Scroll()/120f)) * Matrix.CreateTranslation(Mouse.X(),Mouse.Y(),0);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        int width = GraphicsDevice.Viewport.Width;
        int height = GraphicsDevice.Viewport.Height;
        projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);

        _mapShader.Parameters["view_projection"].SetValue(view * projection);

        _spriteBatch.Begin(effect: _mapShader);
        _spriteBatch.Draw(_testimage, new Vector2(0, 0), Color.White);
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
    
    #endregion fullscreen


}

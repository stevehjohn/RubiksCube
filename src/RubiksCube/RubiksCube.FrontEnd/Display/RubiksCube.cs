using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RubiksCube.FrontEnd.Display;

public sealed class RubiksCube : Game
{
    // ReSharper disable once NotAccessedField.Local
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly GraphicsDeviceManager _graphics;

    private BasicEffect _effect;
    private Matrix _view;
    private Matrix _projection;

    private float _yaw = 0.7f;
    private float _pitch = -0.45f;

    private readonly Color[] _faceColors =
    {
        Color.White,
        Color.Yellow,
        Color.Red,
        Color.Orange,
        Color.Blue,
        Color.Green
    };

    public RubiksCube()
    {
        _graphics = new GraphicsDeviceManager(this);

        IsMouseVisible = true;
        
        _graphics.PreferMultiSampling = true;
    }

    protected override void LoadContent()
    {
        _effect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false
        };

        _view = Matrix.CreateLookAt(new Vector3(5, 5, 7), Vector3.Zero, Vector3.Up);

        _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);
    }

    protected override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();

        if (kb.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (kb.IsKeyDown(Keys.Left))
        {
            _yaw -= 0.02f;
        }

        if (kb.IsKeyDown(Keys.Right))
        {
            _yaw += 0.02f;
        }

        if (kb.IsKeyDown(Keys.Up))
        {
            _pitch -= 0.02f;
        }

        if (kb.IsKeyDown(Keys.Down))
        {
            _pitch += 0.02f;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        var world = Matrix.CreateRotationX(_pitch) * Matrix.CreateRotationY(_yaw);

        _effect.World = world;
        
        _effect.View = _view;
        
        _effect.Projection = _projection;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            DrawRubiksCube();
        }

        base.Draw(gameTime);
    }

    private void DrawRubiksCube()
    {
        const float cubieSize = 0.92f;
        const float spacing = 1.05f;

        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                for (var z = -1; z <= 1; z++)
                {
                    var centre = new Vector3(x * spacing, y * spacing, z * spacing);

                    DrawCubie(centre, cubieSize, x, y, z);
                }
            }
        }
    }

    private void DrawCubie(Vector3 c, float s, int x, int y, int z)
    {
        var h = s / 2f;

        DrawBox(c, h, Color.Black);

        const float stickerInset = 0.08f;
        
        const float stickerOffset = 0.01f;
        
        var stickerHalf = h - stickerInset;

        if (y == 1)
        {
            DrawFace(c + new Vector3(0, h + stickerOffset, 0), Face.Up, stickerHalf, _faceColors[0]);
        }

        if (y == -1)
        {
            DrawFace(c + new Vector3(0, -h - stickerOffset, 0), Face.Down, stickerHalf, _faceColors[1]);
        }

        if (z == 1)
        {
            DrawFace(c + new Vector3(0, 0, h + stickerOffset), Face.Front, stickerHalf, _faceColors[2]);
        }

        if (z == -1)
        {
            DrawFace(c + new Vector3(0, 0, -h - stickerOffset), Face.Back, stickerHalf, _faceColors[3]);
        }

        if (x == -1)
        {
            DrawFace(c + new Vector3(-h - stickerOffset, 0, 0), Face.Left, stickerHalf, _faceColors[4]);
        }

        if (x == 1)
        {
            DrawFace(c + new Vector3(h + stickerOffset, 0, 0), Face.Right, stickerHalf, _faceColors[5]);
        }
    }

    private void DrawFace(Vector3 c, Face face, float h, Color color)
    {
        Vector3 a, b, d, e;

        switch (face)
        {
            case Face.Up:
            case Face.Down:
                a = new Vector3(-h, 0, -h);
                b = new Vector3(h, 0, -h);
                d = new Vector3(h, 0, h);
                e = new Vector3(-h, 0, h);
                break;

            case Face.Front:
            case Face.Back:
                a = new Vector3(-h, -h, 0);
                b = new Vector3(h, -h, 0);
                d = new Vector3(h, h, 0);
                e = new Vector3(-h, h, 0);
                break;

            default:
                a = new Vector3(0, -h, -h);
                b = new Vector3(0, -h, h);
                d = new Vector3(0, h, h);
                e = new Vector3(0, h, -h);
                break;
        }

        var vertices = new[]
        {
            new VertexPositionColor(c + a, color),
            new VertexPositionColor(c + b, color),
            new VertexPositionColor(c + d, color),
            new VertexPositionColor(c + a, color),
            new VertexPositionColor(c + d, color),
            new VertexPositionColor(c + e, color),
        };

        GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2);
    }

    private void DrawBox(Vector3 c, float h, Color color)
    {
        DrawFace(c + new Vector3(0, h, 0), Face.Up, h, color);
        
        DrawFace(c + new Vector3(0, -h, 0), Face.Down, h, color);
        
        DrawFace(c + new Vector3(0, 0, h), Face.Front, h, color);
        
        DrawFace(c + new Vector3(0, 0, -h), Face.Back, h, color);
        
        DrawFace(c + new Vector3(-h, 0, 0), Face.Left, h, color);
        
        DrawFace(c + new Vector3(h, 0, 0), Face.Right, h, color);
    }
}
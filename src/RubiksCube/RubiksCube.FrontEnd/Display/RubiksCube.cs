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
        GraphicsDevice.Clear(Color.FromNonPremultiplied(70, 70, 70, 255));

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

    private void DrawBox(Vector3 c, float h, Color color)
    {
        DrawBox(c, h, h, h, color);
    }

    private void DrawCubie(Vector3 c, float s, int x, int y, int z)
    {
        var h = s / 2f;

        DrawBox(c, h, Color.Black);

        const float stickerInset = 0.08f;

        const float stickerOffset = 0.015f;

        const float stickerThickness = 0.025f;

        var stickerHalf = h - stickerInset;

        switch (y)
        {
            case 1:
                DrawSticker(c + new Vector3(0, h + stickerOffset, 0), Face.Up, stickerHalf, stickerThickness, _faceColors[0]);
                break;
            case -1:
                DrawSticker(c + new Vector3(0, -h - stickerOffset, 0), Face.Down, stickerHalf, stickerThickness, _faceColors[1]);
                break;
        }

        switch (z)
        {
            case 1:
                DrawSticker(c + new Vector3(0, 0, h + stickerOffset), Face.Front, stickerHalf, stickerThickness, _faceColors[2]);
                break;
            case -1:
                DrawSticker(c + new Vector3(0, 0, -h - stickerOffset), Face.Back, stickerHalf, stickerThickness, _faceColors[3]);
                break;
        }

        switch (x)
        {
            case -1:
                DrawSticker(c + new Vector3(-h - stickerOffset, 0, 0), Face.Left, stickerHalf, stickerThickness, _faceColors[4]);
                break;
            case 1:
                DrawSticker(c + new Vector3(h + stickerOffset, 0, 0), Face.Right, stickerHalf, stickerThickness, _faceColors[5]);
                break;
        }
    }

    private void DrawBox(Vector3 c, float hx, float hy, float hz, Color color)
    {
        DrawQuad(
            new Vector3(c.X - hx, c.Y + hy, c.Z - hz),
            new Vector3(c.X + hx, c.Y + hy, c.Z - hz),
            new Vector3(c.X + hx, c.Y + hy, c.Z + hz),
            new Vector3(c.X - hx, c.Y + hy, c.Z + hz),
            color);

        DrawQuad(
            new Vector3(c.X - hx, c.Y - hy, c.Z + hz),
            new Vector3(c.X + hx, c.Y - hy, c.Z + hz),
            new Vector3(c.X + hx, c.Y - hy, c.Z - hz),
            new Vector3(c.X - hx, c.Y - hy, c.Z - hz),
            color);

        DrawQuad(
            new Vector3(c.X - hx, c.Y - hy, c.Z + hz),
            new Vector3(c.X + hx, c.Y - hy, c.Z + hz),
            new Vector3(c.X + hx, c.Y + hy, c.Z + hz),
            new Vector3(c.X - hx, c.Y + hy, c.Z + hz),
            color);

        DrawQuad(
            new Vector3(c.X + hx, c.Y - hy, c.Z - hz),
            new Vector3(c.X - hx, c.Y - hy, c.Z - hz),
            new Vector3(c.X - hx, c.Y + hy, c.Z - hz),
            new Vector3(c.X + hx, c.Y + hy, c.Z - hz),
            color);

        DrawQuad(
            new Vector3(c.X - hx, c.Y - hy, c.Z - hz),
            new Vector3(c.X - hx, c.Y - hy, c.Z + hz),
            new Vector3(c.X - hx, c.Y + hy, c.Z + hz),
            new Vector3(c.X - hx, c.Y + hy, c.Z - hz),
            color);

        DrawQuad(
            new Vector3(c.X + hx, c.Y - hy, c.Z + hz),
            new Vector3(c.X + hx, c.Y - hy, c.Z - hz),
            new Vector3(c.X + hx, c.Y + hy, c.Z - hz),
            new Vector3(c.X + hx, c.Y + hy, c.Z + hz),
            color);
    }

    private void DrawSticker(Vector3 c, Face face, float half, float thickness, Color color)
    {
        var t = thickness / 2f;

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (face)
        {
            case Face.Up:
                DrawBox(c + new Vector3(0, t, 0), half, t, half, color);
                break;

            case Face.Down:
                DrawBox(c + new Vector3(0, -t, 0), half, t, half, color);
                break;

            case Face.Front:
                DrawBox(c + new Vector3(0, 0, t), half, half, t, color);
                break;

            case Face.Back:
                DrawBox(c + new Vector3(0, 0, -t), half, half, t, color);
                break;

            case Face.Left:
                DrawBox(c + new Vector3(-t, 0, 0), t, half, half, color);
                break;

            case Face.Right:
                DrawBox(c + new Vector3(t, 0, 0), t, half, half, color);
                break;
        }
    }

    private void DrawQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
    {
        var vertices = new[]
        {
            new VertexPositionColor(a, color),
            new VertexPositionColor(b, color),
            new VertexPositionColor(c, color),

            new VertexPositionColor(a, color),
            new VertexPositionColor(c, color),
            new VertexPositionColor(d, color)
        };

        GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2);
    }
}
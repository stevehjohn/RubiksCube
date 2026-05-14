using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RubiksCube.FrontEnd.Display;

public sealed class RubiksCube : Game
{
    // ReSharper disable once NotAccessedField.Local
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly GraphicsDeviceManager _graphics;

    private readonly List<Cubie> _cubies = [];

    private BasicEffect _effect;
    
    private Matrix _view;
    
    private Matrix _projection;

    private Matrix _primitiveTransform = Matrix.Identity;

    private KeyboardState _previousKeyboard;

    private MouseState _previousMouse;

    private FaceRotation? _activeRotation;

    private float _yaw = -0.179993838f;
    
    private float _pitch = -1.59999871f;

    private float _cameraDistance = 9.95f;

    private const float CubieSize = 0.92f;

    private const float Spacing = 1.05f;

    private const float QuarterTurn = MathHelper.PiOver2;

    private const float RotationDuration = 0.25f;

    private const float MouseRotationScale = 0.01f;

    private const float MouseZoomScale = 0.01f;

    private const float MinCameraDistance = 5f;

    private const float MaxCameraDistance = 18f;

    private readonly Color[] _faceColors =
    [
        Color.White,
        Color.Yellow,
        Color.Red,
        Color.Orange,
        Color.Blue,
        Color.Green
    ];

    public RubiksCube()
    {
        _graphics = new GraphicsDeviceManager(this);

        IsMouseVisible = true;

        _graphics.PreferMultiSampling = true;
    }

    protected override void Initialize()
    {
        Window.Title = "Rubiks Cube";

        CreateSolvedCube();

        _previousKeyboard = Keyboard.GetState();
        _previousMouse = Mouse.GetState();
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _effect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false
        };

        UpdateView();

        _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        if (keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (keyboard.IsKeyDown(Keys.Left))
        {
            _yaw -= 0.02f;
        }

        if (keyboard.IsKeyDown(Keys.Right))
        {
            _yaw += 0.02f;
        }

        if (keyboard.IsKeyDown(Keys.Up))
        {
            _pitch -= 0.02f;
        }

        if (keyboard.IsKeyDown(Keys.Down))
        {
            _pitch += 0.02f;
        }

        UpdateMouseControls(mouse);
        UpdateActiveRotation(gameTime);
        TryStartFaceRotation(keyboard);

        _previousKeyboard = keyboard;
        _previousMouse = mouse;

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

    private void UpdateMouseControls(MouseState mouse)
    {
        if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Pressed)
        {
            _yaw += (mouse.X - _previousMouse.X) * MouseRotationScale;
            _pitch += (mouse.Y - _previousMouse.Y) * MouseRotationScale;
        }

        var scrollDelta = mouse.ScrollWheelValue - _previousMouse.ScrollWheelValue;

        if (scrollDelta == 0)
        {
            return;
        }

        _cameraDistance = MathHelper.Clamp(
            _cameraDistance - scrollDelta * MouseZoomScale,
            MinCameraDistance,
            MaxCameraDistance);

        UpdateView();
    }

    private void UpdateView()
    {
        var cameraDirection = Vector3.Normalize(new Vector3(5, 5, 7));
        _view = Matrix.CreateLookAt(cameraDirection * _cameraDistance, Vector3.Zero, Vector3.Up);
    }

    private void CreateSolvedCube()
    {
        _cubies.Clear();

        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                for (var z = -1; z <= 1; z++)
                {
                    var cubie = new Cubie(new Vector3(x, y, z));

                    if (y == 1)
                    {
                        cubie.Stickers.Add(new Sticker(Face.Up, Vector3.Up, _faceColors[0]));
                    }

                    if (y == -1)
                    {
                        cubie.Stickers.Add(new Sticker(Face.Down, Vector3.Down, _faceColors[1]));
                    }

                    if (z == 1)
                    {
                        cubie.Stickers.Add(new Sticker(Face.Front, new Vector3(0, 0, 1), _faceColors[2]));
                    }

                    if (z == -1)
                    {
                        cubie.Stickers.Add(new Sticker(Face.Back, new Vector3(0, 0, -1), _faceColors[3]));
                    }

                    if (x == -1)
                    {
                        cubie.Stickers.Add(new Sticker(Face.Left, Vector3.Left, _faceColors[4]));
                    }

                    if (x == 1)
                    {
                        cubie.Stickers.Add(new Sticker(Face.Right, Vector3.Right, _faceColors[5]));
                    }

                    _cubies.Add(cubie);
                }
            }
        }
    }

    private void UpdateActiveRotation(GameTime gameTime)
    {
        if (_activeRotation is not { } rotation)
        {
            return;
        }

        rotation.Elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (rotation.Elapsed < RotationDuration)
        {
            return;
        }

        CompleteFaceRotation(rotation);
        _activeRotation = null;
    }

    private void TryStartFaceRotation(KeyboardState keyboard)
    {
        if (_activeRotation is not null)
        {
            return;
        }

        var counterClockwise = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

        if (WasKeyPressed(keyboard, Keys.U))
        {
            _activeRotation = new FaceRotation(Face.Up, !counterClockwise);
        }
        else if (WasKeyPressed(keyboard, Keys.D))
        {
            _activeRotation = new FaceRotation(Face.Down, !counterClockwise);
        }
        else if (WasKeyPressed(keyboard, Keys.F))
        {
            _activeRotation = new FaceRotation(Face.Front, !counterClockwise);
        }
        else if (WasKeyPressed(keyboard, Keys.B))
        {
            _activeRotation = new FaceRotation(Face.Back, !counterClockwise);
        }
        else if (WasKeyPressed(keyboard, Keys.L))
        {
            _activeRotation = new FaceRotation(Face.Left, !counterClockwise);
        }
        else if (WasKeyPressed(keyboard, Keys.R))
        {
            _activeRotation = new FaceRotation(Face.Right, !counterClockwise);
        }
    }

    private bool WasKeyPressed(KeyboardState keyboard, Keys key)
    {
        return keyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
    }

    private void CompleteFaceRotation(FaceRotation rotation)
    {
        var turn = CreateTurnMatrix(rotation.Face, rotation.Clockwise, QuarterTurn);

        foreach (var cubie in _cubies.Where(cubie => IsCubieOnFace(cubie, rotation.Face)))
        {
            cubie.Position = RoundToGrid(Vector3.Transform(cubie.Position, turn));

            foreach (var sticker in cubie.Stickers)
            {
                sticker.Normal = RoundToGrid(Vector3.TransformNormal(sticker.Normal, turn));
                sticker.Face = FaceFromNormal(sticker.Normal);
            }
        }
    }

    private void DrawRubiksCube()
    {
        foreach (var cubie in _cubies)
        {
            _primitiveTransform = GetCubieAnimationTransform(cubie);
            DrawCubie(cubie);
        }

        _primitiveTransform = Matrix.Identity;
    }

    private Matrix GetCubieAnimationTransform(Cubie cubie)
    {
        if (_activeRotation is not { } rotation || !IsCubieOnFace(cubie, rotation.Face))
        {
            return Matrix.Identity;
        }

        var progress = MathHelper.Clamp(rotation.Elapsed / RotationDuration, 0f, 1f);
        var easedProgress = 1f - MathF.Pow(1f - progress, 3f);

        return CreateTurnMatrix(rotation.Face, rotation.Clockwise, easedProgress * QuarterTurn);
    }

    private static Matrix CreateTurnMatrix(Face face, bool clockwise, float angle)
    {
        var outwardNormal = NormalForFace(face);
        var signedAngle = (clockwise ? -angle : angle) * AxisSign(outwardNormal);

        return Matrix.CreateFromAxisAngle(AbsAxis(outwardNormal), signedAngle);
    }

    private static bool IsCubieOnFace(Cubie cubie, Face face)
    {
        return face switch
        {
            Face.Up => cubie.Position.Y == 1,
            Face.Down => cubie.Position.Y == -1,
            Face.Front => cubie.Position.Z == 1,
            Face.Back => cubie.Position.Z == -1,
            Face.Left => cubie.Position.X == -1,
            Face.Right => cubie.Position.X == 1,
            _ => false
        };
    }

    private static Vector3 NormalForFace(Face face)
    {
        return face switch
        {
            Face.Up => Vector3.Up,
            Face.Down => Vector3.Down,
            Face.Front => new Vector3(0, 0, 1),
            Face.Back => new Vector3(0, 0, -1),
            Face.Left => Vector3.Left,
            Face.Right => Vector3.Right,
            _ => Vector3.Zero
        };
    }

    private static Face FaceFromNormal(Vector3 normal)
    {
        if (normal == Vector3.Up)
        {
            return Face.Up;
        }

        if (normal == Vector3.Down)
        {
            return Face.Down;
        }

        if (normal == new Vector3(0, 0, 1))
        {
            return Face.Front;
        }

        if (normal == new Vector3(0, 0, -1))
        {
            return Face.Back;
        }

        if (normal == Vector3.Left)
        {
            return Face.Left;
        }

        return Face.Right;
    }

    private static Vector3 AbsAxis(Vector3 normal)
    {
        return new Vector3(MathF.Abs(normal.X), MathF.Abs(normal.Y), MathF.Abs(normal.Z));
    }

    private static float AxisSign(Vector3 normal)
    {
        return normal.X + normal.Y + normal.Z;
    }

    private static Vector3 RoundToGrid(Vector3 value)
    {
        return new Vector3(MathF.Round(value.X), MathF.Round(value.Y), MathF.Round(value.Z));
    }

    private void DrawBox(Vector3 c, float h, Color color)
    {
        DrawBox(c, h, h, h, color);
    }

    private void DrawCubie(Cubie cubie)
    {
        var centre = cubie.Position * Spacing;
        var h = CubieSize / 2f;

        DrawBox(centre, h, Color.Black);

        const float stickerInset = 0.08f;

        const float stickerOffset = 0.015f;

        const float stickerThickness = 0.025f;

        var stickerHalf = h - stickerInset;

        foreach (var sticker in cubie.Stickers)
        {
            DrawSticker(
                centre + sticker.Normal * (h + stickerOffset),
                sticker.Face,
                stickerHalf,
                stickerThickness,
                sticker.Color);
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
            new VertexPositionColor(Vector3.Transform(a, _primitiveTransform), color),
            new VertexPositionColor(Vector3.Transform(b, _primitiveTransform), color),
            new VertexPositionColor(Vector3.Transform(c, _primitiveTransform), color),

            new VertexPositionColor(Vector3.Transform(a, _primitiveTransform), color),
            new VertexPositionColor(Vector3.Transform(c, _primitiveTransform), color),
            new VertexPositionColor(Vector3.Transform(d, _primitiveTransform), color)
        };

        GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2);
    }

    private sealed class Cubie(Vector3 position)
    {
        public Vector3 Position { get; set; } = position;

        public List<Sticker> Stickers { get; } = [];
    }

    private sealed class Sticker(Face face, Vector3 normal, Color color)
    {
        public Face Face { get; set; } = face;

        public Vector3 Normal { get; set; } = normal;

        public Color Color { get; } = color;
    }

    private sealed class FaceRotation(Face face, bool clockwise)
    {
        public Face Face { get; } = face;

        public bool Clockwise { get; } = clockwise;

        public float Elapsed { get; set; }
    }
}

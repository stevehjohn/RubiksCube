using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RubiksCube.FrontEnd.Display;

public sealed class Cubie(Vector3 position)
{
    public Vector3 Position { get; set; } = position;

    public List<Sticker> Stickers { get; } = [];
}

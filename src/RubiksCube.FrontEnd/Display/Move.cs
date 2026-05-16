using RubiksCube.Core.Models;

namespace RubiksCube.FrontEnd.Display;

public readonly record struct Move(Face Face, bool Clockwise);
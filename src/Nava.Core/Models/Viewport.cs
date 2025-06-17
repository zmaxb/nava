// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Nava.Core.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class Viewport(int width, int height)
{
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;
}
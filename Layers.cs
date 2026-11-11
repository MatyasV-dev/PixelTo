using System;
using System.Security.Cryptography.X509Certificates;

public class Layer
{
    public string Name { get; set; }
    public bool IsVisible { get; set; } = true;
    public Color[,] Pixels { get; set; }
    public int Width { get; }
    public int Height { get; }

    public Layer (string name, int width, int height)
    {
        Name = name;
        Width = width;
        Height = height;
        Pixels = new Color [Width, Height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Pixels[x, y] = Color.Transparent;
    }

}

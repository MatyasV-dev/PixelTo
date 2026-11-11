
using System;

public class Canvas
{
    public int Width { get; }
    public int Height { get; }
    public List<Layer> Layers { get; } = new();
    public int ActiveLayerIndex { get; set; } = 0;

    public Layer ActiveLayer => Layers[ActiveLayerIndex];

    public Canvas(int width, int height)
    {
        Width = width;
        Height = height;
        Layers.Add(new Layer("Vrstva 1", width, height));
    }
}

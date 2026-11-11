namespace PixelTo
{
    public class ProjectData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<LayerData> Layers { get; set; } = new();
    }

    public class LayerData
    {
        public string Name { get; set; } = "";
        public bool IsVisible { get; set; } = true;
        public List<PixelData> Pixels { get; set; } = new();
    }

    public class PixelData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Color { get; set; } = "";
    }
}
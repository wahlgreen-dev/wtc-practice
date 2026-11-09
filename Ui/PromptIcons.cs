using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace WtcPractice;

public class PromptIcons
{
    private const string ResourcePrefix = "Icons.";
    private const string ResourceExtension = ".png";
    private const float MaxChannel = 255f;

    private readonly Dictionary<string, GUIStyle> styles = new();

    public GUIStyle Get(string name)
    {
        if (styles.TryGetValue(name, out GUIStyle existing))
            return existing;

        GUIStyle style = new();
        style.normal.background = Load(name);
        styles[name] = style;
        return style;
    }

    private static Texture2D Load(string name)
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourcePrefix + name + ResourceExtension);
        using MemoryStream file = new();
        stream.CopyTo(file);

        byte[] rgba = PngReader.ReadRgba(file.ToArray(), out int width, out int height);

        Texture2D texture = new(width, height, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int pixel = (y * width + x) * 4;
                Color color = new(rgba[pixel] / MaxChannel, rgba[pixel + 1] / MaxChannel, rgba[pixel + 2] / MaxChannel, rgba[pixel + 3] / MaxChannel);
                texture.SetPixel(x, height - 1 - y, color);
            }
        }

        texture.Apply();
        return texture;
    }
}

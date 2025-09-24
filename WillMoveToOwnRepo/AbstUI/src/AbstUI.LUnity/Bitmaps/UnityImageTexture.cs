using System;
using AbstUI.Bitmaps;
using AbstUI.Primitives;
using UnityEngine;
using Unity.Collections;
using AbstUI.Tools;

namespace AbstUI.LUnity.Bitmaps;

public class UnityTexture2D : AbstBaseTexture2D<Texture2D>
{
    public Texture2D? Texture { get; private set; }

    public UnityTexture2D(Texture2D texture, string name = "") : base(name)
    {
        Texture = texture;
    }

    public override int Width => Texture?.width ?? 0;
    public override int Height => Texture?.height ?? 0;

    public Sprite? ToSprite()
    {
        if (Texture == null)
            return null;
        return Sprite.Create(Texture, new Rect(0, 0, Texture.width, Texture.height), new Vector2(0.5f, 0.5f));
    }

    protected override void DisposeTexture()
    {
        if (Texture != null)
        {
            UnityEngine.Object.Destroy(Texture);
            Texture = null;
        }
    }

    public override byte[] GetPixels()
    {
        if (Texture == null)
            return Array.Empty<byte>();

#if UNITY_2021_1_OR_NEWER
        var data = Texture.GetRawTextureData<byte>();
        return data.ToArray();
#else
        var colors = Texture.GetPixels32();
        var buffer = new byte[colors.Length * 4];
        for (int i = 0; i < colors.Length; i++)
        {
            int index = i * 4;
            buffer[index] = colors[i].r;
            buffer[index + 1] = colors[i].g;
            buffer[index + 2] = colors[i].b;
            buffer[index + 3] = colors[i].a;
        }
        return buffer;
#endif
    }

    public override void SetARGBPixels(byte[] argbPixels)
    {
        if (Texture == null)
            return;
        if (argbPixels == null || argbPixels.Length != Width * Height * 4)
            throw new ArgumentException("Expected ARGB8888 buffer with Width*Height*4 bytes.", nameof(argbPixels));

        var buffer = (byte[])argbPixels.Clone();
        APixel.ToRGBA(buffer);
        SetRGBAPixels(buffer);
    }

    public override void SetRGBAPixels(byte[] rgbaPixels)
    {
        if (Texture == null)
            return;
        if (rgbaPixels == null || rgbaPixels.Length != Width * Height * 4)
            throw new ArgumentException("Expected RGBA8888 buffer with Width*Height*4 bytes.", nameof(rgbaPixels));

#if UNITY_2021_1_OR_NEWER
        Texture.LoadRawTextureData(rgbaPixels);
        Texture.Apply();
#else
        var colors = new Color32[Width * Height];
        for (int i = 0; i < colors.Length; i++)
        {
            int index = i * 4;
            colors[i] = new Color32(rgbaPixels[index], rgbaPixels[index + 1], rgbaPixels[index + 2], rgbaPixels[index + 3]);
        }
        Texture.SetPixels32(colors);
        Texture.Apply();
#endif
    }

    public override IAbstTexture2D Clone()
    {
        if (Texture == null)
            throw new InvalidOperationException("Cannot clone a disposed texture.");

        var clone = new Texture2D(Texture.width, Texture.height, Texture.format, Texture.mipmapCount > 0)
        {
            filterMode = Texture.filterMode,
            wrapMode = Texture.wrapMode,
        };

#if UNITY_2021_1_OR_NEWER
        clone.LoadRawTextureData(GetPixels());
        clone.Apply();
#else
        clone.SetPixels32(Texture.GetPixels32());
        clone.Apply();
#endif

        return new UnityTexture2D(clone, Name + "_Clone");
    }
}

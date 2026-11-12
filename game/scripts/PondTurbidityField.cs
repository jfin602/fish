#nullable enable
using Godot;

namespace Fish;

/// <summary>Deterministic presentation density over the existing baked Pond; no bake or gameplay state.</summary>
public sealed class PondTurbidityField
{
    public const int TextureWidth = 64;
    public const int TextureHeight = 16;
    public const float VolumeDensity = 0.11f;
    private readonly GeneratedBasin _basin;
    private readonly FastNoiseLite _broad;
    private readonly FastNoiseLite _detail;
    public Vector3 Center { get; }
    public Vector3 Size { get; }
    public float SurfaceY { get; }

    public PondTurbidityField(GeneratedBasin basin, float surfaceY)
    {
        _basin = basin;
        SurfaceY = surfaceY;
        var minX = basin.ShoreMin.X - 8f;
        var maxX = basin.ShoreMax.X + 8f;
        var minZ = basin.ShoreMin.Y - 8f;
        var maxZ = basin.ShoreMax.Y + 8f;
        var bottom = basin.DeepFloorY - 2f;
        var top = surfaceY + 1f;
        Center = new Vector3((minX + maxX) * .5f, (bottom + top) * .5f, (minZ + maxZ) * .5f);
        Size = new Vector3(maxX - minX, top - bottom, maxZ - minZ);
        var seed = unchecked((int)(basin.Seed ^ (basin.Seed >> 32)));
        _broad = new FastNoiseLite { Seed = seed ^ 0x354c, Frequency = .031f,
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth };
        _detail = new FastNoiseLite { Seed = seed ^ 0x7a31, Frequency = .073f,
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth };
    }

    /// <summary>Finite normalized sediment density; sunlight and shadow never enter this calculation.</summary>
    public float Sample(Vector3 world)
    {
        var shore = _basin.DistanceToShore(world.X, world.Z);
        if (shore <= 0f || world.Y >= SurfaceY) return 0f;
        var bed = _basin.HeightAt(world.X, world.Z);
        var waterDepth = SurfaceY - bed;
        if (world.Y <= bed || waterDepth <= 0f) return 0f;
        var broad = _broad.GetNoise3D(world.X, world.Y * .65f, world.Z);
        var detail = _detail.GetNoise3D(world.X, world.Y * .45f, world.Z);
        var shallowOpen = 1f - Smooth(2.5f, 6f, waterDepth);
        var nearBed = 1f - Smooth(.25f, 2.2f, world.Y - bed);
        var shelter = Smooth(.10f, 4f, shore) * (1f - Smooth(8f, 19f, shore));
        var density = .31f + .25f * broad + .11f * detail - .15f * shallowOpen
            + .34f * nearBed * (.65f + .35f * broad) + .08f * shelter * (.5f + .5f * detail);
        var waterMask = Smooth(0f, .7f, SurfaceY - world.Y) *
            Smooth(0f, .35f, world.Y - bed) * Smooth(0f, 2f, shore);
        return Mathf.Clamp(density, .10f, .88f) * waterMask;
    }

    public ImageTexture3D CreateTexture()
    {
        var slices = new Godot.Collections.Array<Image>();
        for (var z = 0; z < TextureWidth; z++)
        {
            var image = Image.CreateEmpty(TextureWidth, TextureHeight, false, Image.Format.R8);
            for (var y = 0; y < TextureHeight; y++)
            for (var x = 0; x < TextureWidth; x++)
            {
                var world = Center + new Vector3(
                    ((x + .5f) / TextureWidth - .5f) * Size.X,
                    ((y + .5f) / TextureHeight - .5f) * Size.Y,
                    ((z + .5f) / TextureWidth - .5f) * Size.Z);
                var sample = Sample(world);
                image.SetPixel(x, y, new Color(sample, sample, sample));
            }
            slices.Add(image);
        }
        var texture = new ImageTexture3D();
        if (texture.Create(Image.Format.R8, TextureWidth, TextureHeight, TextureWidth, false, slices) != Error.Ok)
            throw new System.InvalidOperationException("Pond turbidity texture creation failed");
        return texture;
    }

    private static float Smooth(float start, float end, float value)
    {
        var t = Mathf.Clamp((value - start) / (end - start), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
}

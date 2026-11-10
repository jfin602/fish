#nullable enable
using Godot;
using System;

namespace Fish;

/// <summary>Owns normal-play P2 rendering only. No fish, input, camera, or clock writes.</summary>
public partial class UnderwaterAtmosphere : Node3D
{
    [Export] public Color BackgroundWater { get; set; } = new(0.035f, 0.17f, 0.22f);
    [Export] public Color AmbientWater { get; set; } = new(0.56f, 0.73f, 0.76f);
    [Export] public Color DistanceFogColor { get; set; } = new(0.10f, 0.37f, 0.42f);
    [Export] public Color VolumetricWaterColor { get; set; } = new(0.40f, 0.72f, 0.76f);
    [Export] public Color SunColor { get; set; } = new(0.78f, 0.92f, 0.90f);
    [Export] public Color CausticColor { get; set; } = new(0.68f, 0.88f, 0.78f);
    [Export] public Color MoteColor { get; set; } = new(0.73f, 0.91f, 0.82f, 0.33f);
    [Export(PropertyHint.Range, "0.001,0.06,0.001")] public float VolumetricDensity { get; set; } = 0.024f;
    [Export(PropertyHint.Range, "0,0.02,0.0005")] public float DistanceFogDensity { get; set; } = 0.0035f;
    [Export(PropertyHint.Range, "0,0.05,0.001")] public float LowerWaterFog { get; set; } = 0.018f;
    [Export(PropertyHint.Range, "-8,8,0.1")] public float LowerFogStartHeight { get; set; } = -1.0f;
    [Export(PropertyHint.Range, "20,90,1")] public float FogLength { get; set; } = 64.0f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float AmbientEnergy { get; set; } = 0.42f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float VolumetricAmbientInject { get; set; } = 0.34f;
    [Export(PropertyHint.Range, "-0.5,0.5,0.01")] public float VolumetricAnisotropy { get; set; } = 0.18f;
    [Export(PropertyHint.Range, "0,3,0.01")] public float SunEnergy { get; set; } = 1.25f;
    [Export(PropertyHint.Range, "-1.57,-0.2,0.01")] public float SunPitch { get; set; } = -1.05f;
    [Export(PropertyHint.Range, "-3.14,3.14,0.01")] public float SunYaw { get; set; } = -0.48f;
    [Export(PropertyHint.Range, "0,0.8,0.01")] public float CausticEnergy { get; set; } = 0.32f;
    [Export(PropertyHint.Range, "0,0.5,0.01")] public float CausticDrift { get; set; } = 0.22f;
    [Export(PropertyHint.Range, "0,1.5,0.01")] public float CausticTravel { get; set; } = 0.8f;
    [Export(PropertyHint.Range, "0.25,1.2,0.01")] public float SurfaceWaveHeight { get; set; } = 1.0f;
    [Export(PropertyHint.Range, "0.25,2,0.01")] public float SurfaceWaveSpeed { get; set; } = 1.0f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float SurfaceWaveDetail { get; set; } = 0.8f;
    [Export(PropertyHint.Range, "0,8,1")] public int ShaftCount { get; set; } = 6;
    [Export(PropertyHint.Range, "0,2,0.01")] public float ShaftBrightness { get; set; } = 1.0f;
    [Export(PropertyHint.Range, "0,0.2,0.01")] public float ScreenWarpPixels { get; set; } = 0.10f;
    [Export(PropertyHint.Range, "0,0.04,0.001")] public float ScreenColorVariation { get; set; } = 0.007f;
    [Export(PropertyHint.File, "*.gdshader")] public string ScreenShaderPath { get; set; } = "res://shaders/underwater_screen.gdshader";
    [Export(PropertyHint.Range, "0,160,1")] public int MoteCount { get; set; } = 128;
    [Export(PropertyHint.Range, "4,30,0.5")] public float MoteLifetime { get; set; } = 18.0f;

    private Godot.Environment? _environment;
    private ShaderMaterial? _skyMaterial;
    private ShaderMaterial? _surfaceMaterial;
    private ShaderMaterial? _shaftMaterial;
    private ShaderMaterial? _screenMaterial;
    private MeshInstance3D? _surface;
    private MeshInstance3D? _shafts;
    private float _surfaceHeight;
    private float _deepReferenceHeight;
    private float _lastDepth = -1f;
    private DirectionalLight3D? _sun;
    private Node3D? _effects;
    private CanvasLayer? _screenLayer;
    private ColorRect? _screenRect;
    private GpuParticles3D? _motes;
    private readonly Decal[] _caustics = new Decal[1];
    private bool _enabled = true;
    private double _presentationSeconds;

    public bool Enabled => _enabled;
    public GpuParticles3D? Motes => _motes;
    public CanvasLayer? ScreenLayer => _screenLayer;
    public Godot.Environment? WaterEnvironment => _environment;
    public float SurfaceHeight => _surfaceHeight;
    public float DeepReferenceHeight => _deepReferenceHeight;
    public float EffectiveDepth01 { get; private set; }
    public float EffectiveUsefulRange { get; private set; }

    public LightingSettings CaptureLighting() => new()
    {
        SurfaceWaveHeight = SurfaceWaveHeight, SurfaceWaveSpeed = SurfaceWaveSpeed,
        SurfaceWaveDetail = SurfaceWaveDetail, CausticEnergy = CausticEnergy,
        CausticDrift = CausticDrift, CausticTravel = CausticTravel,
        ShaftCount = ShaftCount, ShaftBrightness = ShaftBrightness,
        SunEnergy = SunEnergy, AmbientEnergy = AmbientEnergy,
        VolumetricDensity = VolumetricDensity, DistanceFogDensity = DistanceFogDensity,
        MoteCount = MoteCount, ScreenWarpPixels = ScreenWarpPixels,
    };

    public void ApplyLighting(LightingSettings settings)
    {
        if (!settings.IsValid()) throw new ArgumentException("Lighting values are out of range", nameof(settings));
        SurfaceWaveHeight = settings.SurfaceWaveHeight;
        SurfaceWaveSpeed = settings.SurfaceWaveSpeed;
        SurfaceWaveDetail = settings.SurfaceWaveDetail;
        CausticEnergy = settings.CausticEnergy;
        CausticDrift = settings.CausticDrift;
        CausticTravel = settings.CausticTravel;
        ShaftCount = settings.ShaftCount;
        ShaftBrightness = settings.ShaftBrightness;
        SunEnergy = settings.SunEnergy;
        AmbientEnergy = settings.AmbientEnergy;
        VolumetricDensity = settings.VolumetricDensity;
        DistanceFogDensity = settings.DistanceFogDensity;
        MoteCount = settings.MoteCount;
        ScreenWarpPixels = settings.ScreenWarpPixels;
        _surfaceMaterial?.SetShaderParameter("wave_height", SurfaceWaveHeight);
        _surfaceMaterial?.SetShaderParameter("wave_speed", SurfaceWaveSpeed);
        _surfaceMaterial?.SetShaderParameter("wave_detail", SurfaceWaveDetail);
        _shaftMaterial?.SetShaderParameter("shaft_count", ShaftCount);
        _shaftMaterial?.SetShaderParameter("shaft_brightness", ShaftBrightness);
        _screenMaterial?.SetShaderParameter("warp_pixels", ScreenWarpPixels);
        RefreshDepth(true);
        foreach (var decal in _caustics)
            if (decal is not null) decal.EmissionEnergy = CausticEnergy;
        if (_motes is not null) _motes.Amount = Mathf.Max(1, MoteCount);
        if (_motes is not null) _motes.Visible = MoteCount > 0;
    }

    public void Initialize(Node3D main)
    {
        if (RenderingServer.GetCurrentRenderingMethod() != "forward_plus")
        {
            GD.PushError("FISH_ATMOSPHERE_REQUIRES_FORWARD_PLUS");
            return;
        }
        if (!ParametersAreFinite())
        {
            GD.PushError("FISH_ATMOSPHERE_INVALID_PARAMETERS");
            return;
        }

        _environment = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            BackgroundColor = BackgroundWater,
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            AmbientLightColor = AmbientWater,
            AmbientLightEnergy = AmbientEnergy,
            FogEnabled = true,
            FogMode = Godot.Environment.FogModeEnum.Exponential,
            FogLightColor = DistanceFogColor,
            FogDensity = DistanceFogDensity,
            FogHeight = LowerFogStartHeight,
            FogHeightDensity = LowerWaterFog,
            FogSunScatter = 0.08f,
            VolumetricFogEnabled = true,
            VolumetricFogDensity = VolumetricDensity,
            VolumetricFogAlbedo = VolumetricWaterColor,
            VolumetricFogAmbientInject = VolumetricAmbientInject,
            VolumetricFogAnisotropy = VolumetricAnisotropy,
            VolumetricFogLength = FogLength,
        };
        main.GetNode<WorldEnvironment>("WorldEnvironment").Environment = _environment;
        _sun = main.GetNode<DirectionalLight3D>("Sun");
        _sun.Rotation = new Vector3(SunPitch, SunYaw, 0);
        _sun.LightColor = SunColor;
        _sun.LightEnergy = SunEnergy;
        _sun.LightVolumetricFogEnergy = 0.8f;

        _effects = new Node3D { Name = "WaterEffects" };
        AddChild(_effects);
        AddSky();
        AddSurface(main);
        var seabed = main.GetNodeOrNull<SeabedSurface>("Habitat/Seabed");
        if (seabed is null || _surface is null)
        {
            GD.PushError("FISH_ATMOSPHERE_MISSING_DEPTH_ANCHOR");
            return;
        }
        // A camera two metres above the authored deep shelf reaches full extinction.
        _deepReferenceHeight = seabed.ToGlobal(new Vector3(0,
            seabed.GetHeightAt(0, SeabedSurface.DeepShelfReferenceZ) + 2f,
            SeabedSurface.DeepShelfReferenceZ)).Y;
        if (_deepReferenceHeight >= _surfaceHeight || !float.IsFinite(_deepReferenceHeight))
        {
            GD.PushError("FISH_ATMOSPHERE_INVALID_DEPTH_ANCHOR");
            return;
        }
        seabed.SetWaterColumn(_surfaceHeight, _deepReferenceHeight);
        // Continuous height fog affects imported reef meshes without replacing their materials.
        _environment.FogHeight = _surfaceHeight - 0.60f * (_surfaceHeight - _deepReferenceHeight)
            + LowerFogStartHeight;
        AddCaustics();
        AddShafts(main);
        AddMotes();
        AddScreenTreatment();
        RefreshDepth(true);
        if (OS.GetCmdlineUserArgs().Contains("--atmosphere=off")) SetEnabled(false);
        GD.Print($"FISH_ATMOSPHERE_READY method=forward_plus volumetric={VolumetricDensity:F3} fog={DistanceFogDensity:F4} surface={(_surface is null ? "missing" : "ready")} shafts={ShaftCount} shaft_surfaces={(_shafts?.Mesh?.GetSurfaceCount() ?? 0)} caustics=1 motes={MoteCount} screen={(_screenLayer is null ? "missing" : "ready")}");
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        if (_environment is not null)
        {
            _environment.FogEnabled = enabled;
            _environment.VolumetricFogEnabled = enabled;
            _environment.BackgroundMode = enabled && _skyMaterial is not null
                ? Godot.Environment.BGMode.Sky : Godot.Environment.BGMode.Color;
            if (!enabled) _environment.BackgroundColor = new Color(0.025f, 0.19f, 0.24f);
            _environment.AmbientLightSource = enabled
                ? Godot.Environment.AmbientSource.Color : Godot.Environment.AmbientSource.Bg;
            if (!enabled)
            {
                _environment.AmbientLightColor = Colors.White;
                _environment.AmbientLightEnergy = 1.0f;
            }
        }
        if (_sun is not null)
        {
            _sun.LightColor = enabled ? SunColor : Colors.White;
            if (!enabled) _sun.LightEnergy = 1.4f;
            _sun.LightVolumetricFogEnergy = enabled ? 0.8f : 0;
        }
        if (_effects is not null) _effects.Visible = enabled;
        if (_motes is not null) _motes.Emitting = enabled;
        if (_screenLayer is not null) _screenLayer.Visible = enabled;
        if (enabled) RefreshDepth(true);
    }

    public override void _Process(double delta)
    {
        if (!_enabled || _effects is null) return;
        RefreshDepth();
        _presentationSeconds += delta;
        _surfaceMaterial?.SetShaderParameter("surface_time", (float)_presentationSeconds);
        _shaftMaterial?.SetShaderParameter("shaft_time", (float)_presentationSeconds);
        _screenMaterial?.SetShaderParameter("screen_time", (float)_presentationSeconds);
        // Slow projected movement is world anchored. Pause stops this node's process.
        for (var index = 0; index < _caustics.Length; index++)
        {
            if (_caustics[index] is not { } decal) continue;
            var phase = (float)_presentationSeconds * CausticDrift;
            decal.Position = new Vector3(CausticTravel * (Mathf.Sin(phase) + 0.18f * Mathf.Sin(phase * 1.71f)), 3,
                CausticTravel * (Mathf.Cos(phase * 0.73f) + 0.15f * Mathf.Sin(phase * 1.37f)));
            if (_shafts is not null) _shafts.Position = new Vector3(decal.Position.X, 0, decal.Position.Z);
        }
    }

    /// <summary>Presentation-only read of the active viewport camera. Base settings never change here.</summary>
    public void RefreshDepth(bool force = false)
    {
        if (!_enabled || _environment is null || _sun is null || _deepReferenceHeight >= _surfaceHeight)
            return;
        var camera = GetViewport().GetCamera3D();
        if (camera is null) return;
        var raw = Mathf.Clamp((_surfaceHeight - camera.GlobalPosition.Y) /
            (_surfaceHeight - _deepReferenceHeight), 0f, 1f);
        var depth = raw * raw * (3f - 2f * raw);
        if (!force && Mathf.Abs(depth - _lastDepth) < 0.001f) return;
        _lastDepth = depth;
        EffectiveDepth01 = depth;
        _environment.FogDensity = Mathf.Min(0.02f, DistanceFogDensity * (1f + 2.6f * depth));
        _environment.VolumetricFogDensity = Mathf.Min(0.06f, VolumetricDensity * (1f + 1.25f * depth));
        _environment.VolumetricFogLength = Mathf.Max(20f, FogLength * (1f - 0.32f * depth));
        EffectiveUsefulRange = _environment.VolumetricFogLength;
        _environment.FogLightColor = DistanceFogColor.Lerp(new Color(0.055f, 0.18f, 0.30f), depth);
        _environment.VolumetricFogAlbedo = VolumetricWaterColor.Lerp(new Color(0.22f, 0.43f, 0.59f), depth);
        _environment.BackgroundColor = BackgroundWater.Lerp(new Color(0.025f, 0.095f, 0.18f), depth);
        _environment.AmbientLightColor = AmbientWater.Lerp(new Color(0.32f, 0.52f, 0.65f), depth);
        _environment.AmbientLightEnergy = AmbientEnergy * (1f - 0.52f * depth);
        _sun.LightEnergy = SunEnergy * (1f - 0.65f * depth);
        _skyMaterial?.SetShaderParameter("water_depth", depth);
        _surfaceMaterial?.SetShaderParameter("camera_depth", depth);
    }

    private void AddSky()
    {
        const string path = "res://shaders/underwater_sky.gdshader";
        var shader = ResourceLoader.Load<Shader>(path);
        if (shader is null)
        {
            GD.PushError("FISH_ATMOSPHERE_MISSING_SKY");
            return;
        }
        _skyMaterial = new ShaderMaterial { Shader = shader };
        _skyMaterial.SetShaderParameter("sun_direction", _sun!.GlobalTransform.Basis.Z.Normalized());
        _environment!.Sky = new Sky { SkyMaterial = _skyMaterial };
        _environment.BackgroundMode = Godot.Environment.BGMode.Sky;
    }

    private void AddSurface(Node3D main)
    {
        // The saved upper collision owns the boundary; this mesh has no physics shape.
        var upper = main.GetNode<CollisionShape3D>("Habitat/HiddenLimits/Upper");
        if (upper.Shape is not BoxShape3D box)
        {
            GD.PushError("FISH_ATMOSPHERE_MISSING_UPPER_LIMIT");
            return;
        }
        const string path = "res://shaders/water_surface.gdshader";
        var shader = ResourceLoader.Load<Shader>(path);
        if (shader is null)
        {
            GD.PushError("FISH_ATMOSPHERE_MISSING_SURFACE_SHADER");
            return;
        }
        var extent = box.Size;
        var plane = new PlaneMesh
        {
            Size = new Vector2(extent.X + 14.0f, extent.Z + 14.0f),
            SubdivideWidth = 48,
            SubdivideDepth = 48,
        };
        _surfaceMaterial = new ShaderMaterial { Shader = shader };
        _surfaceMaterial.SetShaderParameter("sun_direction", _sun!.GlobalTransform.Basis.Z.Normalized());
        _surfaceMaterial.SetShaderParameter("surface_time", 0.0f);
        _surfaceMaterial.SetShaderParameter("wave_height", SurfaceWaveHeight);
        _surfaceMaterial.SetShaderParameter("wave_speed", SurfaceWaveSpeed);
        _surfaceMaterial.SetShaderParameter("wave_detail", SurfaceWaveDetail);
        _surfaceMaterial.SetShaderParameter("surface_half_size", (extent.X + 14.0f) * 0.5f);
        _surface = new MeshInstance3D
        {
            Name = "OverheadSurface",
            Mesh = plane,
            MaterialOverride = _surfaceMaterial,
            CustomAabb = new Aabb(new Vector3(-(extent.X + 14.0f) * 0.5f, -0.35f, -(extent.Z + 14.0f) * 0.5f),
                new Vector3(extent.X + 14.0f, 0.7f, extent.Z + 14.0f)),
            Position = _effects!.ToLocal(upper.GlobalPosition) + new Vector3(0, extent.Y * 0.5f + 0.34f, 0),
        };
        _effects!.AddChild(_surface);
        _surfaceHeight = _surface.GlobalPosition.Y;
    }

    private void AddShafts(Node3D main)
    {
        if (_surface is null || _sun is null || _effects is null) return;
        var seabed = main.GetNodeOrNull<SeabedSurface>("Habitat/Seabed");
        var decal = _caustics[0];
        var image = decal?.TextureEmission?.GetImage();
        if (seabed is null || decal is null || image is null) return;
        var shader = ResourceLoader.Load<Shader>("res://shaders/sun_shafts.gdshader");
        if (shader is null)
        {
            GD.PushError("FISH_ATMOSPHERE_MISSING_SHAFT_SHADER");
            return;
        }

        // Each ribbon lands on a sampled bright caustic ridge. Translation of the
        // decal and mesh together preserves that contact as the projection drifts.
        var sunUp = _sun.GlobalTransform.Basis.Z.Normalized();
        if (sunUp.Y < 0.15f) return;
        Vector2[] centers = [new(-17, -14), new(-10, 9), new(4, -12), new(11, 12),
            new(21, -5), new(-22, 19), new(18, 21), new(-3, 22)];
        var vertices = new Vector3[centers.Length * 6];
        var uvs = new Vector2[vertices.Length];
        var colors = new Color[vertices.Length];
        var triangles = new[] { 0, 1, 2, 0, 2, 3 };
        for (var shaft = 0; shaft < centers.Length; shaft++)
        {
            var seed = decal.ToLocal(new Vector3(centers[shaft].X, 0, centers[shaft].Y));
            var pixelX = Mathf.Clamp(Mathf.RoundToInt((seed.X / 100f + 0.5f) * 255f), 10, 245);
            var pixelY = Mathf.Clamp(Mathf.RoundToInt((seed.Z / 100f + 0.5f) * 255f), 10, 245);
            var strongest = -1f;
            var bestX = pixelX;
            var bestY = pixelY;
            for (var dy = -8; dy <= 8; dy++)
            for (var dx = -8; dx <= 8; dx++)
            {
                var sample = image.GetPixel(pixelX + dx, pixelY + dy).A;
                if (sample <= strongest) continue;
                strongest = sample;
                bestX = pixelX + dx;
                bestY = pixelY + dy;
            }
            var gx = image.GetPixel(bestX + 2, bestY).A - image.GetPixel(bestX - 2, bestY).A;
            var gy = image.GetPixel(bestX, bestY + 2).A - image.GetPixel(bestX, bestY - 2).A;
            var tangentLocal = new Vector3(-gy, 0, gx).Normalized();
            if (tangentLocal.LengthSquared() < 0.01f) tangentLocal = Vector3.Right;
            var tangent = (decal.GlobalTransform.Basis * tangentLocal).Normalized();
            var localFloor = new Vector3(((bestX + 0.5f) / 256f - 0.5f) * 100f, 0,
                ((bestY + 0.5f) / 256f - 0.5f) * 100f);
            var worldFloor = decal.ToGlobal(localFloor);
            var bottom = new Vector3(worldFloor.X, seabed.GetHeightAt(worldFloor.X, worldFloor.Z) + 0.3f, worldFloor.Z);
            var top = bottom + sunUp * ((_surfaceHeight - 0.08f - bottom.Y) / sunUp.Y);
            var half = (1.1f + shaft % 3 * 0.22f) * 0.5f;
            var across = tangent * half;
            var corners = new[] { top - across, top + across, bottom + across, bottom - across };
            var coordinates = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
            for (var corner = 0; corner < 6; corner++)
            {
                var index = shaft * 6 + corner;
                var source = triangles[corner];
                vertices[index] = _effects.ToLocal(corners[source]);
                uvs[index] = coordinates[source];
                colors[index] = new Color(0.11f + shaft * 0.103f, (shaft + 0.5f) / 8f, 1, 1);
            }
        }
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices;
        arrays[(int)Mesh.ArrayType.TexUV] = uvs;
        arrays[(int)Mesh.ArrayType.Color] = colors;
        var mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        _shaftMaterial = new ShaderMaterial { Shader = shader };
        _shaftMaterial.SetShaderParameter("shaft_time", 0.0f);
        _shaftMaterial.SetShaderParameter("shaft_count", ShaftCount);
        _shaftMaterial.SetShaderParameter("shaft_brightness", ShaftBrightness);
        _shaftMaterial.SetShaderParameter("water_surface_y", _surfaceHeight);
        _shaftMaterial.SetShaderParameter("water_deep_y", _deepReferenceHeight);
        _shaftMaterial.SetShaderParameter("deep_transmission", 0.12f);
        _shafts = new MeshInstance3D
        {
            Name = "SunShafts",
            Mesh = mesh,
            MaterialOverride = _shaftMaterial,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
        };
        _effects.AddChild(_shafts);
    }

    private void AddCaustics()
    {
        var texture = MakeCausticTexture();
        for (var index = 0; index < _caustics.Length; index++)
        {
            var decal = new Decal
            {
                Name = $"CausticProjection{index + 1}",
                Position = new Vector3(0, 3, 0),
                Rotation = new Vector3(0, 0.12f, 0),
                // 100 m covers the 84 m habitat after the small off-axis rotation and drift.
                Size = new Vector3(100, 22, 100),
                TextureAlbedo = texture,
                AlbedoMix = 0,
                TextureEmission = texture,
                EmissionEnergy = CausticEnergy,
                NormalFade = 0.3f,
                UpperFade = 0.25f,
                LowerFade = 0.30f,
            };
            _effects!.AddChild(decal);
            _caustics[index] = decal;
        }
    }

    private static uint Hash(uint x, uint y, uint salt)
    {
        var value = x * 0x9e3779b9u ^ y * 0x85ebca6bu ^ salt;
        value ^= value >> 16;
        value *= 0x7feb352du;
        value ^= value >> 15;
        value *= 0x846ca68bu;
        return value ^ (value >> 16);
    }

    private static float Random01(uint x, uint y, uint salt) =>
        (Hash(x, y, salt) & 0x00ffffffu) / 16777215f;

    private static float ValueNoise(float x, float y, uint salt)
    {
        var ix = Mathf.FloorToInt(x);
        var iy = Mathf.FloorToInt(y);
        var fx = x - ix;
        var fy = y - iy;
        fx *= fx * (3 - 2 * fx);
        fy *= fy * (3 - 2 * fy);
        var a = Mathf.Lerp(Random01((uint)ix, (uint)iy, salt), Random01((uint)(ix + 1), (uint)iy, salt), fx);
        var b = Mathf.Lerp(Random01((uint)ix, (uint)(iy + 1), salt), Random01((uint)(ix + 1), (uint)(iy + 1), salt), fx);
        return Mathf.Lerp(a, b, fy);
    }

    private Texture2D MakeCausticTexture()
    {
        const int size = 256;
        var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var u = (x + 0.5f) / size - 0.5f;
            var v = (y + 0.5f) / size - 0.5f;
            // Rotate the cells and bend their boundaries before measuring the two nearest sites.
            var cellX = (0.95f * u - 0.31f * v + 0.5f) * 10f;
            var cellY = (0.31f * u + 0.95f * v + 0.5f) * 9f;
            cellX += 0.47f * (ValueNoise(u * 4f + 13f, v * 4f + 7f, 11) - 0.5f);
            cellY += 0.47f * (ValueNoise(u * 4f + 3f, v * 4f + 17f, 23) - 0.5f);
            var baseX = Mathf.FloorToInt(cellX);
            var baseY = Mathf.FloorToInt(cellY);
            var nearest = float.MaxValue;
            var second = float.MaxValue;
            var nearestX = 0;
            var nearestY = 0;
            for (var dy = -1; dy <= 1; dy++)
            for (var dx = -1; dx <= 1; dx++)
            {
                var sx = baseX + dx;
                var sy = baseY + dy;
                var px = sx + 0.5f + 0.78f * (Random01((uint)sx, (uint)sy, 31) - 0.5f);
                var py = sy + 0.5f + 0.78f * (Random01((uint)sx, (uint)sy, 47) - 0.5f);
                var distance = (cellX - px) * (cellX - px) + (cellY - py) * (cellY - py);
                if (distance < nearest)
                {
                    second = nearest;
                    nearest = distance;
                    nearestX = sx;
                    nearestY = sy;
                }
                else if (distance < second) second = distance;
            }
            var width = 0.22f + 0.11f * Random01((uint)nearestX, (uint)nearestY, 59);
            var ridge = Mathf.Clamp((width - (Mathf.Sqrt(second) - Mathf.Sqrt(nearest))) / width, 0f, 1f);
            ridge *= ridge;
            // Low frequency modulation opens gaps instead of drawing a continuous cell net.
            var breakup = ValueNoise(u * 13f + 21f, v * 11f + 9f, 71);
            var gap = Mathf.Clamp((breakup - 0.28f) * 3.5f, 0f, 1f);
            var edge = Mathf.Clamp((0.49f - Mathf.Abs(u)) * 12f, 0f, 1f) *
                Mathf.Clamp((0.49f - Mathf.Abs(v)) * 12f, 0f, 1f);
            var brightness = 0.45f + 0.55f * Random01((uint)nearestX, (uint)nearestY, 83);
            var strength = ridge * gap * edge * brightness;
            image.SetPixel(x, y, new Color(CausticColor.R * strength, CausticColor.G * strength,
                CausticColor.B * strength, strength * CausticColor.A * 0.55f));
        }
        return ImageTexture.CreateFromImage(image);
    }

    private void AddMotes()
    {
        var material = new StandardMaterial3D
        {
            AlbedoColor = MoteColor,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        };
        var process = new ParticleProcessMaterial
        {
            EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box,
            EmissionBoxExtents = new Vector3(27, 5.5f, 27),
            Direction = Vector3.Up,
            Spread = 22,
            Gravity = Vector3.Zero,
            InitialVelocityMin = 0.012f,
            InitialVelocityMax = 0.045f,
        };
        _motes = new GpuParticles3D
        {
            Name = "SuspendedMotes",
            Position = new Vector3(0, -0.5f, 0),
            Amount = MoteCount,
            Lifetime = MoteLifetime,
            ProcessMaterial = process,
            DrawPass1 = new SphereMesh
            {
                Radius = 0.025f, Height = 0.05f, RadialSegments = 6, Rings = 3,
                Material = material
            },
            VisibilityAabb = new Aabb(new Vector3(-28, -7, -28), new Vector3(56, 14, 56)),
            Emitting = true,
        };
        _effects!.AddChild(_motes);
    }

    private void AddScreenTreatment()
    {
        if (!ResourceLoader.Exists(ScreenShaderPath))
        {
            GD.PushWarning("FISH_ATMOSPHERE_OPTIONAL_MISSING underwater_screen.gdshader");
            return;
        }
        var shader = ResourceLoader.Load<Shader>(ScreenShaderPath);
        if (shader is null)
        {
            GD.PushWarning("FISH_ATMOSPHERE_OPTIONAL_MISSING underwater_screen.gdshader");
            return;
        }
        _screenMaterial = new ShaderMaterial { Shader = shader };
        _screenMaterial.SetShaderParameter("warp_pixels", ScreenWarpPixels);
        _screenMaterial.SetShaderParameter("color_variation", ScreenColorVariation);
        _screenMaterial.SetShaderParameter("screen_time", 0.0f);
        _screenLayer = new CanvasLayer { Name = "WaterScreen", Layer = 0 };
        AddChild(_screenLayer);
        _screenRect = new ColorRect
        {
            Name = "ScreenTreatment",
            Color = Colors.White,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Material = _screenMaterial,
        };
        _screenLayer.AddChild(_screenRect);
        ResizeScreen();
        GetViewport().SizeChanged += ResizeScreen;
    }

    private void ResizeScreen()
    {
        if (_screenRect is not null) _screenRect.Size = GetViewport().GetVisibleRect().Size;
    }

    public override void _ExitTree()
    {
        if (_screenRect is not null) GetViewport().SizeChanged -= ResizeScreen;
    }

    public bool ParametersAreFinite() =>
        ColorIsBounded(BackgroundWater) && ColorIsBounded(AmbientWater) &&
        ColorIsBounded(DistanceFogColor) && ColorIsBounded(VolumetricWaterColor) &&
        ColorIsBounded(SunColor) && ColorIsBounded(CausticColor) && ColorIsBounded(MoteColor) &&
        float.IsFinite(VolumetricDensity) && VolumetricDensity is > 0 and <= 0.06f &&
        float.IsFinite(DistanceFogDensity) && DistanceFogDensity is >= 0 and <= 0.02f &&
        float.IsFinite(LowerWaterFog) && LowerWaterFog is >= 0 and <= 0.05f &&
        float.IsFinite(LowerFogStartHeight) && LowerFogStartHeight is >= -8 and <= 8 &&
        float.IsFinite(FogLength) && FogLength is >= 20 and <= 90 &&
        float.IsFinite(AmbientEnergy) && AmbientEnergy is >= 0 and <= 1 &&
        float.IsFinite(VolumetricAmbientInject) && VolumetricAmbientInject is >= 0 and <= 1 &&
        float.IsFinite(VolumetricAnisotropy) && VolumetricAnisotropy is >= -0.5f and <= 0.5f &&
        float.IsFinite(SunEnergy) && SunEnergy is > 0 and <= 3 &&
        float.IsFinite(SunPitch) && SunPitch is >= -1.57f and <= -0.2f &&
        float.IsFinite(SunYaw) && SunYaw is >= -3.14f and <= 3.14f &&
        float.IsFinite(CausticEnergy) && CausticEnergy is >= 0 and <= 0.8f &&
        float.IsFinite(CausticDrift) && CausticDrift is >= 0 and <= 0.5f &&
        float.IsFinite(ScreenWarpPixels) && ScreenWarpPixels is >= 0 and <= 0.2f &&
        float.IsFinite(ScreenColorVariation) && ScreenColorVariation is >= 0 and <= 0.04f &&
        float.IsFinite(CausticTravel) && CausticTravel is >= 0 and <= 1.5f &&
        float.IsFinite(SurfaceWaveHeight) && SurfaceWaveHeight is >= 0.25f and <= 1.2f &&
        float.IsFinite(SurfaceWaveSpeed) && SurfaceWaveSpeed is >= 0.25f and <= 2f &&
        float.IsFinite(SurfaceWaveDetail) && SurfaceWaveDetail is >= 0 and <= 1f &&
        ShaftCount is >= 0 and <= 8 && float.IsFinite(ShaftBrightness) && ShaftBrightness is >= 0 and <= 2f &&
        MoteCount is >= 0 and <= 160 && float.IsFinite(MoteLifetime) &&
        MoteLifetime is >= 4 and <= 30;

    private static bool ColorIsBounded(Color color) =>
        float.IsFinite(color.R) && color.R is >= 0 and <= 1 &&
        float.IsFinite(color.G) && color.G is >= 0 and <= 1 &&
        float.IsFinite(color.B) && color.B is >= 0 and <= 1 &&
        float.IsFinite(color.A) && color.A is >= 0 and <= 1;
}

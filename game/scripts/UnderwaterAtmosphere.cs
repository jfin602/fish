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
    [Export(PropertyHint.Range, "0,0.8,0.01")] public float CausticEnergy { get; set; } = 0.45f;
    [Export(PropertyHint.Range, "0,0.5,0.01")] public float CausticDrift { get; set; } = 0.10f;
    [Export(PropertyHint.Range, "0,2,0.1")] public float ScreenWarpPixels { get; set; } = 0.55f;
    [Export(PropertyHint.Range, "0,0.04,0.001")] public float ScreenColorVariation { get; set; } = 0.007f;
    [Export(PropertyHint.File, "*.gdshader")] public string ScreenShaderPath { get; set; } = "res://shaders/underwater_screen.gdshader";
    [Export(PropertyHint.Range, "0,160,1")] public int MoteCount { get; set; } = 128;
    [Export(PropertyHint.Range, "4,30,0.5")] public float MoteLifetime { get; set; } = 18.0f;

    private Godot.Environment? _environment;
    private ShaderMaterial? _skyMaterial;
    private ShaderMaterial? _surfaceMaterial;
    private MeshInstance3D? _surface;
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
        AddCaustics();
        AddMotes();
        AddScreenTreatment();
        if (OS.GetCmdlineUserArgs().Contains("--atmosphere=off")) SetEnabled(false);
        GD.Print($"FISH_ATMOSPHERE_READY method=forward_plus volumetric={VolumetricDensity:F3} fog={DistanceFogDensity:F4} surface={(_surface is null ? "missing" : "ready")} caustics=1 motes={MoteCount} screen={(_screenLayer is null ? "missing" : "ready")}");
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
            _environment.BackgroundColor = enabled ? BackgroundWater : new Color(0.025f, 0.19f, 0.24f);
            _environment.AmbientLightSource = enabled
                ? Godot.Environment.AmbientSource.Color : Godot.Environment.AmbientSource.Bg;
            _environment.AmbientLightColor = enabled ? AmbientWater : Colors.White;
            _environment.AmbientLightEnergy = enabled ? AmbientEnergy : 1.0f;
        }
        if (_sun is not null)
        {
            _sun.LightColor = enabled ? SunColor : Colors.White;
            _sun.LightEnergy = enabled ? SunEnergy : 1.4f;
            _sun.LightVolumetricFogEnergy = enabled ? 0.8f : 0;
        }
        if (_effects is not null) _effects.Visible = enabled;
        if (_motes is not null) _motes.Emitting = enabled;
        if (_screenLayer is not null) _screenLayer.Visible = enabled;
    }

    public override void _Process(double delta)
    {
        if (!_enabled || _effects is null) return;
        _presentationSeconds += delta;
        _surfaceMaterial?.SetShaderParameter("surface_time", (float)_presentationSeconds);
        // Slow projected movement is world anchored. Pause stops this node's process.
        for (var index = 0; index < _caustics.Length; index++)
        {
            if (_caustics[index] is not { } decal) continue;
            var phase = (float)_presentationSeconds * CausticDrift + index * 1.7f;
            decal.Position = new Vector3(0.45f * Mathf.Sin(phase), 2,
                0.45f * Mathf.Cos(phase * 0.8f));
            decal.Rotation = new Vector3(0, 0.035f * Mathf.Sin(phase * 0.6f), 0);
        }
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
            Size = new Vector2(extent.X, extent.Z),
            SubdivideWidth = 32,
            SubdivideDepth = 32,
        };
        _surfaceMaterial = new ShaderMaterial { Shader = shader };
        _surfaceMaterial.SetShaderParameter("sun_direction", _sun!.GlobalTransform.Basis.Z.Normalized());
        _surfaceMaterial.SetShaderParameter("surface_time", 0.0f);
        _surface = new MeshInstance3D
        {
            Name = "OverheadSurface",
            Mesh = plane,
            MaterialOverride = _surfaceMaterial,
            CustomAabb = new Aabb(new Vector3(-extent.X * 0.5f, -0.2f, -extent.Z * 0.5f),
                new Vector3(extent.X, 0.4f, extent.Z)),
            Position = _effects!.ToLocal(upper.GlobalPosition) + new Vector3(0, extent.Y * 0.5f + 0.22f, 0),
        };
        _effects!.AddChild(_surface);
    }

    private void AddCaustics()
    {
        var texture = MakeCausticTexture();
        for (var index = 0; index < _caustics.Length; index++)
        {
            var decal = new Decal
            {
                Name = $"CausticProjection{index + 1}",
                Position = new Vector3(0, 2, 0),
                Size = new Vector3(84, 24, 84),
                TextureAlbedo = texture,
                AlbedoMix = 0,
                TextureEmission = texture,
                EmissionEnergy = CausticEnergy,
                NormalFade = 0.3f,
                UpperFade = 0.25f,
                LowerFade = 0.25f,
            };
            _effects!.AddChild(decal);
            _caustics[index] = decal;
        }
    }

    private Texture2D MakeCausticTexture()
    {
        const int size = 256;
        var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var u = x * Mathf.Tau / size;
            var v = y * Mathf.Tau / size;
            var a = Mathf.Sin(14 * u + 1.4f * Mathf.Sin(6 * v));
            var b = Mathf.Sin(12 * v - 1.2f * Mathf.Sin(8 * u));
            var ridge = Mathf.Pow(Mathf.Max(0, 1 - Mathf.Abs(a + b) * 0.8f), 9);
            var edge = Mathf.Pow(Mathf.Max(0, 1 - Mathf.Abs(x - 127.5f) / 127.5f), 0.5f) *
                Mathf.Pow(Mathf.Max(0, 1 - Mathf.Abs(y - 127.5f) / 127.5f), 0.5f);
            var line = Mathf.Sqrt(ridge) * edge;
            image.SetPixel(x, y, new Color(CausticColor.R * line, CausticColor.G * line,
                CausticColor.B * line, ridge * edge * CausticColor.A * 0.72f));
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
            DrawPass1 = new SphereMesh { Radius = 0.025f, Height = 0.05f, Material = material },
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
        var material = new ShaderMaterial { Shader = shader };
        material.SetShaderParameter("warp_pixels", ScreenWarpPixels);
        material.SetShaderParameter("color_variation", ScreenColorVariation);
        _screenLayer = new CanvasLayer { Name = "WaterScreen", Layer = 0 };
        AddChild(_screenLayer);
        _screenRect = new ColorRect
        {
            Name = "ScreenTreatment",
            Color = Colors.White,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Material = material,
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
        float.IsFinite(ScreenWarpPixels) && ScreenWarpPixels is >= 0 and <= 2 &&
        float.IsFinite(ScreenColorVariation) && ScreenColorVariation is >= 0 and <= 0.04f &&
        MoteCount is >= 0 and <= 160 && float.IsFinite(MoteLifetime) &&
        MoteLifetime is >= 4 and <= 30;

    private static bool ColorIsBounded(Color color) =>
        float.IsFinite(color.R) && color.R is >= 0 and <= 1 &&
        float.IsFinite(color.G) && color.G is >= 0 and <= 1 &&
        float.IsFinite(color.B) && color.B is >= 0 and <= 1 &&
        float.IsFinite(color.A) && color.A is >= 0 and <= 1;
}

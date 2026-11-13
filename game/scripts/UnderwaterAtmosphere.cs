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
    // Artistic depth-fog end distance at the surface. Actual optical range also depends on scatter.
    [Export(PropertyHint.Range, "18,60,1")] public float VisibilityRangeMeters { get; set; } = 35f;
    private const float FogNearMeters = 5f;
    private const float PondFogNearMeters = 4.5f;
    private const float FogExtinction = 0.96f;
    private const float PondFogExtinction = 0.78f;
    private const float FogCurve = 1.2f;
    [Export(PropertyHint.Range, "0,0.05,0.001")] public float LowerWaterFog { get; set; } = 0.018f;
    [Export(PropertyHint.Range, "-8,8,0.1")] public float LowerFogStartHeight { get; set; } = -1.0f;
    [Export(PropertyHint.Range, "20,90,1")] public float FogLength { get; set; } = 64.0f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float AmbientEnergy { get; set; } = 0.42f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float VolumetricAmbientInject { get; set; } = 0.34f;
    [Export(PropertyHint.Range, "-0.5,0.5,0.01")] public float VolumetricAnisotropy { get; set; } = 0.18f;
    [Export(PropertyHint.Range, "0,3,0.01")] public float SunEnergy { get; set; } = 1.25f;
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
    private ShaderMaterial? _pondBasinMaterial;
    private ShaderMaterial? _screenMaterial;
    private MeshInstance3D? _surface;
    private MeshInstance3D? _shafts;
    private float _surfaceHeight;
    private float _deepReferenceHeight;
    private float _lastDepth = -1f;
    private float _lastImmersion = -1f;
    private float _immersion = 1f;
    private bool _immersionInitialized;
    private const float ImmersionHalfBand = 0.45f;
    private const float ImmersionResponse = 7f;
    private static readonly Color PondAirBackground = new(0.43f, 0.48f, 0.46f);
    private static readonly Color PondAirAmbient = new(0.75f, 0.77f, 0.70f);
    private Color _airFogColor = PondAirBackground;
    private DirectionalLight3D? _sun;
    private Node3D? _effects;
    private CanvasLayer? _screenLayer;
    private ColorRect? _screenRect;
    private GpuParticles3D? _motes;
    private FogVolume? _pondTurbidity;
    private FogMaterial? _pondTurbidityMaterial;
    private bool _pondClarityDiagnosticOff;
    private readonly Decal[] _caustics = new Decal[1];
    private bool _enabled = true;
    private bool _visibilityBypass;
    private float _legacyDistanceFogDensity = 0.0035f;
    private double _presentationSeconds;
    private EnvironmentClock? _environmentClock;
    private SolarLightingState _solarLighting;
    private SolarWaterState _solarWater;
    private float _nightWeight;
    private double _lastSolarFraction = double.NaN;
    private WaterVisualProfile _profile = WaterVisualProfile.Ocean;

    public WorldEnvironmentProfile ProfileId => _profile.Id;
    public EnvironmentClock? EnvironmentTime => _environmentClock;
    public SolarState Solar => (_environmentClock ??
        throw new InvalidOperationException("Environment clock is not bound")).Solar;

    /// <summary>Choose scene-local presentation before Initialize; never edits user presets.</summary>
    public void ConfigureProfile(WorldEnvironmentProfile id)
    {
        if (_environment is not null) throw new InvalidOperationException("Water profile is already initialized");
        _profile = WaterVisualProfile.For(id);
        BackgroundWater = _profile.Background;
        AmbientWater = _profile.Ambient;
        DistanceFogColor = _profile.Fog;
        VolumetricWaterColor = _profile.Volume;
        SunColor = _profile.Sun;
        CausticColor = _profile.Caustic;
        MoteColor = _profile.Motes;
        if (id == WorldEnvironmentProfile.Pond)
        {
            LowerWaterFog = 0.024f;
            VolumetricAmbientInject = 0.25f;
            VolumetricAnisotropy = 0.11f;
            FogLength = 52f;
            ScreenWarpPixels = 0.04f;
            ScreenColorVariation = 0.003f;
        }
    }

    public bool Enabled => _enabled;
    public GpuParticles3D? Motes => _motes;
    public CanvasLayer? ScreenLayer => _screenLayer;
    public Godot.Environment? WaterEnvironment => _environment;
    public float SurfaceHeight => _surfaceHeight;
    public float DeepReferenceHeight => _deepReferenceHeight;
    public float EffectiveDepth01 { get; private set; }
    public float CameraSignedImmersionDepth { get; private set; }
    public float CameraImmersion01 => _immersion;
    public static float TargetCameraImmersion(float? waterHeight, float eyeY)
    {
        if (waterHeight is not { } height || !float.IsFinite(height) || !float.IsFinite(eyeY))
            return 1f;
        var signedDepth = height - eyeY;
        return Mathf.SmoothStep(0f, 1f, Mathf.Clamp(
            (signedDepth + ImmersionHalfBand) / (2f * ImmersionHalfBand), 0f, 1f));
    }
    /// <summary>Artistic depth-fog end distance, not a measured optical sight distance.</summary>
    public float EffectiveUsefulRange { get; private set; }
    public float EffectiveBaseVisibilityRange => VisibilityRangeMeters;

    /// <summary>Native diagnostic: bypass only the new distance extinction, retaining height/volume fog and every other effect.</summary>
    public void SetVisibilityDiagnosticBypass(bool bypass)
    {
        if (_environment is null) throw new InvalidOperationException("Atmosphere is not initialized");
        _visibilityBypass = bypass;
        _environment.FogDensity = bypass ? 0f :
            Mathf.Lerp(_profile.AirFogDensity,
                _profile.Id == WorldEnvironmentProfile.Pond ? PondFogExtinction : FogExtinction, _immersion);
    }

    /// <summary>Approximate engine depth-fog scene retention before height/volumetric fog.</summary>
    public static float EstimateDistanceRetention(float distanceMeters, float usefulRangeMeters,
        float extinction = FogExtinction, float fogNearMeters = FogNearMeters)
    {
        var position = Mathf.Clamp((distanceMeters - fogNearMeters) /
            (usefulRangeMeters - fogNearMeters), 0f, 1f);
        return 1f - extinction * Mathf.Pow(position, FogCurve);
    }

    public LightingSettings CaptureLighting() => new()
    {
        SurfaceWaveHeight = SurfaceWaveHeight, SurfaceWaveSpeed = SurfaceWaveSpeed,
        SurfaceWaveDetail = SurfaceWaveDetail, CausticEnergy = CausticEnergy,
        CausticDrift = CausticDrift, CausticTravel = CausticTravel,
        ShaftCount = ShaftCount, ShaftBrightness = ShaftBrightness,
        SunEnergy = SunEnergy, AmbientEnergy = AmbientEnergy,
        VolumetricDensity = VolumetricDensity, VisibilityDistanceMeters = VisibilityRangeMeters,
        DistanceFogDensity = _legacyDistanceFogDensity,
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
        VisibilityRangeMeters = settings.VisibilityDistanceMeters;
        _legacyDistanceFogDensity = settings.DistanceFogDensity;
        MoteCount = settings.MoteCount;
        ScreenWarpPixels = settings.ScreenWarpPixels;
        _surfaceMaterial?.SetShaderParameter("wave_height", SurfaceWaveHeight);
        _surfaceMaterial?.SetShaderParameter("wave_speed", SurfaceWaveSpeed);
        _surfaceMaterial?.SetShaderParameter("wave_detail", SurfaceWaveDetail);
        _shaftMaterial?.SetShaderParameter("shaft_count", ShaftCount);
        _screenMaterial?.SetShaderParameter("warp_pixels", ScreenWarpPixels);
        RefreshSolar(true);
        if (_motes is not null) _motes.Amount = Mathf.Max(1, MoteCount);
        if (_motes is not null) _motes.Visible = MoteCount > 0 && _immersion > 0.005f;
    }

    public void Initialize(Node3D main)
    {
        _environmentClock = main.GetNode<EnvironmentClock>("EnvironmentClock");
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
            FogMode = Godot.Environment.FogModeEnum.Depth,
            FogLightColor = DistanceFogColor,
            FogDensity = FogExtinction,
            FogDepthBegin = FogNearMeters,
            FogDepthEnd = EffectiveBaseVisibilityRange,
            FogDepthCurve = FogCurve,
            FogSkyAffect = 0f,
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
        // One short, single-map shadow from the existing solar light gives nearby
        // trunks and foliage a real shaded side without extra light nodes.
        if (_profile.Id == WorldEnvironmentProfile.Pond)
        {
            _sun.DirectionalShadowMode = DirectionalLight3D.ShadowMode.Orthogonal;
            _sun.DirectionalShadowMaxDistance = 28f;
        }
        UpdateSolar(true);

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
        // Generated deep-core metadata anchors the accepted water-column response.
        _deepReferenceHeight = seabed.ToGlobal(new Vector3(seabed.DeepCore.X,
            seabed.DeepReferenceY, seabed.DeepCore.Y)).Y;
        if (_deepReferenceHeight >= _surfaceHeight || !float.IsFinite(_deepReferenceHeight))
        {
            GD.PushError("FISH_ATMOSPHERE_INVALID_DEPTH_ANCHOR");
            return;
        }
        seabed.SetWaterColumn(_surfaceHeight, _deepReferenceHeight);
        if (_profile.Id == WorldEnvironmentProfile.Pond)
        {
            _pondBasinMaterial = seabed.GetNode<MeshInstance3D>("Mesh").MaterialOverride as ShaderMaterial;
            _pondBasinMaterial?.SetShaderParameter("water_surface_y", _surfaceHeight);
            _pondBasinMaterial?.SetShaderParameter("sun_direction", _sun.GlobalTransform.Basis.Z.Normalized());
            _pondBasinMaterial?.SetShaderParameter("sunlight_energy", _solarLighting.DirectEnergyFactor);
        }
        // Continuous height fog affects imported reef meshes without replacing their materials.
        _environment.FogHeight = _surfaceHeight - 0.60f * (_surfaceHeight - _deepReferenceHeight)
            + LowerFogStartHeight;
        if (_profile.Id == WorldEnvironmentProfile.Pond)
            AddPondTurbidity(seabed);
        AddCaustics();
        AddShafts(main);
        AddMotes();
        AddScreenTreatment();
        _pondClarityDiagnosticOff = OS.GetCmdlineUserArgs().Contains("--pond-clarity=off");
        RefreshSolar(true);
        if (OS.GetCmdlineUserArgs().Contains("--atmosphere=off")) SetEnabled(false);
        GD.Print($"FISH_ATMOSPHERE_READY method=forward_plus volumetric={VolumetricDensity:F3} visibility_base={VisibilityRangeMeters:F1} visibility_end={EffectiveUsefulRange:F1} surface={(_surface is null ? "missing" : "ready")} shafts={ShaftCount} shaft_surfaces={(_shafts?.Mesh?.GetSurfaceCount() ?? 0)} caustics=1 motes={MoteCount} screen={(_screenLayer is null ? "missing" : "ready")}");
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
            if (!enabled) _environment.BackgroundColor = _profile.Id == WorldEnvironmentProfile.Ocean
                ? new Color(0.025f, 0.19f, 0.24f) : _profile.Background;
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
            if (!enabled)
            {
                _sun.LightColor = Colors.White;
                _sun.LightEnergy = 1.4f;
                _sun.LightVolumetricFogEnergy = 0;
            }
        }
        if (_effects is not null) _effects.Visible = enabled;
        if (_motes is not null) _motes.Emitting = enabled;
        if (_screenLayer is not null) _screenLayer.Visible = enabled;
        if (enabled)
        {
            // The eye may have moved while effects were disabled. Rejoin at its
            // current position instead of replaying a stale underwater state.
            _immersionInitialized = false;
            RefreshSolar(true);
        }
    }

    /// <summary>Development comparison only; leaves graphics presets and the baked world untouched.</summary>
    public void SetPondClarityDiagnosticEnabled(bool enabled)
    {
        _pondClarityDiagnosticOff = !enabled;
        RefreshDepth(true);
    }

    private void AddPondTurbidity(SeabedSurface seabed)
    {
        var start = Time.GetTicksUsec();
        var field = new PondTurbidityField(seabed.Basin, _surfaceHeight);
        _pondTurbidityMaterial = new FogMaterial
        {
            Albedo = VolumetricWaterColor,
            Density = 0f,
            DensityTexture = field.CreateTexture(),
            EdgeFade = .25f,
        };
        _pondTurbidity = new FogVolume
        {
            Name = "PondTurbidity",
            Shape = RenderingServer.FogVolumeShape.Box,
            Size = field.Size,
            Material = _pondTurbidityMaterial,
            Position = ToLocal(field.Center),
        };
        AddChild(_pondTurbidity);
        GD.Print($"POND_TURBIDITY_READY seed={seabed.Basin.Seed} texture={PondTurbidityField.TextureWidth}x{PondTurbidityField.TextureHeight}x{PondTurbidityField.TextureWidth} size={field.Size} density_max={PondTurbidityField.VolumeDensity:F3} build_ms={(Time.GetTicksUsec() - start) / 1000.0:F2}");
    }

    public override void _Process(double delta)
    {
        if (!_enabled || _effects is null) return;
        var solarChanged = UpdateSolar(false);
        if (solarChanged) ApplySolarLightLevels(EffectiveDepth01);
        RefreshDepth(false, (float)delta);
        _presentationSeconds += delta;
        _surfaceMaterial?.SetShaderParameter("surface_time", (float)_presentationSeconds);
        _pondBasinMaterial?.SetShaderParameter("surface_time", (float)_presentationSeconds);
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

    /// <summary>Reapply the shared solar state after a deterministic development time change.</summary>
    public void RefreshSolar(bool force = false)
    {
        if (UpdateSolar(force)) RefreshDepth(true);
    }

    private bool UpdateSolar(bool force)
    {
        if (!_enabled || _environmentClock is null || _sun is null) return false;
        var solar = _environmentClock.Solar;
        if (!force && solar.DayFraction == _lastSolarFraction) return false;
        _lastSolarFraction = solar.DayFraction;
        _solarLighting = SolarLightingState.From(solar, _profile.Id);
        _sun.ShadowEnabled = _profile.Id == WorldEnvironmentProfile.Pond && solar.DirectLight > 0.08f;
        _solarWater = SolarWaterState.From(solar, _solarLighting, _profile, SunEnergy);
        _nightWeight = 1f - Mathf.Clamp(solar.Daylight + 0.5f * solar.Twilight, 0f, 1f);
        _airFogColor = _solarLighting.AirHorizon.Lerp(PondAirBackground,
            solar.Daylight * solar.Daylight);
        var direction = solar.DirectionToSun.Y < 0f ? -solar.DirectionToSun : solar.DirectionToSun;
        var right = Vector3.Back.Cross(direction).Normalized();
        _sun.GlobalBasis = new Basis(right, direction.Cross(right).Normalized(), direction);
        _skyMaterial?.SetShaderParameter("sun_direction", direction);
        _skyMaterial?.SetShaderParameter("warm_direction", solar.DirectionToSun);
        _skyMaterial?.SetShaderParameter("air_horizon", ToVector(_solarLighting.AirHorizon));
        _skyMaterial?.SetShaderParameter("air_zenith", ToVector(_solarLighting.AirZenith));
        _skyMaterial?.SetShaderParameter("warm_horizon", ToVector(_solarLighting.WarmHorizon));
        _skyMaterial?.SetShaderParameter("direct_color", ToVector(_solarLighting.DirectColor));
        _skyMaterial?.SetShaderParameter("warm_strength", _solarLighting.WarmStrength);
        _skyMaterial?.SetShaderParameter("sun_glow", _solarLighting.SunGlow);
        _skyMaterial?.SetShaderParameter("moon_glow", SolarLightingState.MoonVisibility(solar));
        _skyMaterial?.SetShaderParameter("night_weight", _nightWeight);
        _skyMaterial?.SetShaderParameter("underwater_daylight", _solarWater.UnderwaterSky);
        _screenMaterial?.SetShaderParameter("night_saturation",
            Mathf.Lerp(1f, _profile.NightSaturation, _nightWeight));
        _surfaceMaterial?.SetShaderParameter("sun_direction", direction);
        _surfaceMaterial?.SetShaderParameter("surface_daylight", _solarWater.SurfaceDaylight);
        _surfaceMaterial?.SetShaderParameter("surface_illumination", _solarWater.SurfaceIllumination);
        _surfaceMaterial?.SetShaderParameter("night_surface_glow",
            ToVector(_profile.NightSurfaceGlow) * _nightWeight);
        _surfaceMaterial?.SetShaderParameter("night_underwater_color",
            ToVector(_profile.NightFog) * 0.11f);
        _surfaceMaterial?.SetShaderParameter("night_weight", _nightWeight);
        _surfaceMaterial?.SetShaderParameter("moon_glow", SolarLightingState.MoonVisibility(solar));
        _surfaceMaterial?.SetShaderParameter("reflected_sky_horizon", ToVector(_solarLighting.AirHorizon));
        _surfaceMaterial?.SetShaderParameter("reflected_sky_zenith", ToVector(_solarLighting.AirZenith));
        _surfaceMaterial?.SetShaderParameter("reflection_sun_color", ToVector(_solarLighting.DirectColor));
        _surfaceMaterial?.SetShaderParameter("reflection_sun_energy", _solarWater.ReflectionSunEnergy);
        _pondBasinMaterial?.SetShaderParameter("sun_direction", direction);
        _pondBasinMaterial?.SetShaderParameter("sunlight_energy", _solarLighting.DirectEnergyFactor);
        _shaftMaterial?.SetShaderParameter("sun_direction", direction);
        _shaftMaterial?.SetShaderParameter("shaft_lean", _solarWater.ShaftLean);
        _shaftMaterial?.SetShaderParameter("shaft_color", ToVector(_profile.ShaftColor.Lerp(_solarLighting.DirectColor, 0.45f * _solarLighting.WarmStrength)));
        ApplySolarEffects();
        return true;
    }

    private static Vector3 ToVector(Color color) => new(color.R, color.G, color.B);

    /// <summary>Presentation-only read of the active viewport camera. Base settings never change here.</summary>
    public void RefreshDepth(bool force = false, float delta = 0f)
    {
        if (!_enabled || _environment is null || _sun is null || _deepReferenceHeight >= _surfaceHeight)
            return;
        var camera = GetViewport().GetCamera3D();
        if (camera is null) return;
        // A positive signed depth places the eye below the real, level Pond plane.
        // No fish transform, collider or motor state is consulted here.
        if (_profile.Id == WorldEnvironmentProfile.Pond)
        {
            var seabed = GetParent()?.GetNodeOrNull<SeabedSurface>("Habitat/Seabed");
            if (seabed is not null && seabed.TryGetPondWaterSurfaceHeight(out var height))
            {
                CameraSignedImmersionDepth = height - camera.GlobalPosition.Y;
                var target = TargetCameraImmersion(height, camera.GlobalPosition.Y);
                if (!_immersionInitialized) { _immersion = target; _immersionInitialized = true; }
                else if (delta > 0f) _immersion = Mathf.Lerp(_immersion, target,
                    1f - Mathf.Exp(-ImmersionResponse * delta));
            }
            // Missing baked height data fails closed to the underwater presentation.
            else { CameraSignedImmersionDepth = float.NaN; _immersion = 1f; _immersionInitialized = false; }
        }
        else { CameraSignedImmersionDepth = _surfaceHeight - camera.GlobalPosition.Y; _immersion = 1f; }
        var raw = Mathf.Clamp((_surfaceHeight - camera.GlobalPosition.Y) /
            (_surfaceHeight - _deepReferenceHeight), 0f, 1f);
        var depth = raw * raw * (3f - 2f * raw);
        if (!force && Mathf.Abs(depth - _lastDepth) < 0.001f &&
            Mathf.Abs(_immersion - _lastImmersion) < 0.001f) return;
        _lastDepth = depth;
        _lastImmersion = _immersion;
        EffectiveDepth01 = depth;
        // The depth mode gives a clear foreground and a continuous camera-distance
        // reveal. Camera depth never changes the saved surface-distance base.
        EffectiveUsefulRange = EffectiveBaseVisibilityRange * Mathf.Lerp(1f, _profile.DeepRangeFactor, depth);
        var waterFogBegin = _profile.Id == WorldEnvironmentProfile.Pond ? PondFogNearMeters : FogNearMeters;
        _environment.FogDepthBegin = Mathf.Lerp(_profile.AirFogBegin, waterFogBegin, _immersion);
        _environment.FogDepthEnd = Mathf.Lerp(_profile.AirFogEnd, EffectiveUsefulRange, _immersion);
        _environment.FogDepthCurve = Mathf.Lerp(FogCurve,
            _profile.Id == WorldEnvironmentProfile.Pond ? 1.55f : FogCurve, _immersion);
        _environment.FogSkyAffect = _profile.Id == WorldEnvironmentProfile.Pond ? .35f * _immersion : 0f;
        var waterFog = _profile.Id == WorldEnvironmentProfile.Pond ? PondFogExtinction : FogExtinction;
        _environment.FogDensity = _visibilityBypass ? 0f : Mathf.Lerp(_profile.AirFogDensity, waterFog, _immersion);
        _environment.FogHeightDensity = LowerWaterFog * _immersion;
        _environment.VolumetricFogDensity = Mathf.Min(0.06f, VolumetricDensity * Mathf.Lerp(1f, _profile.DeepVolumeFactor, depth)) * _immersion
            * (_profile.Id == WorldEnvironmentProfile.Pond ? .19f : 1f);
        if (_pondTurbidityMaterial is not null)
            _pondTurbidityMaterial.Density = _pondClarityDiagnosticOff ? 0f :
                PondTurbidityField.VolumeDensity * _immersion;
        _environment.VolumetricFogLength = Mathf.Max(20f, FogLength * Mathf.Lerp(1f, _profile.DeepFogLengthFactor, depth));
        var waterVolumeColor = VolumetricWaterColor.Lerp(_profile.DeepVolume, depth)
            .Lerp(_profile.NightVolume, _nightWeight);
        _environment.VolumetricFogAlbedo = PondAirAmbient.Lerp(waterVolumeColor, _immersion);
        if (_pondTurbidityMaterial is not null)
            _pondTurbidityMaterial.Albedo = waterVolumeColor;
        _environment.BackgroundColor = PondAirBackground.Lerp(BackgroundWater.Lerp(_profile.DeepBackground, depth), _immersion);
        ApplySolarLightLevels(depth);
        _skyMaterial?.SetShaderParameter("water_depth", depth);
        _skyMaterial?.SetShaderParameter("air_mix", 1f - _immersion);
        _surfaceMaterial?.SetShaderParameter("camera_depth", depth);
        _screenMaterial?.SetShaderParameter("immersion", _immersion);
        ApplySolarEffects();
        if (_motes is not null) _motes.Visible = MoteCount > 0 && _immersion > 0.005f;
    }

    private void ApplySolarLightLevels(float depth)
    {
        if (!_enabled || _environment is null || _sun is null) return;
        var waterFogColor = DistanceFogColor.Lerp(_profile.DeepFog, depth);
        waterFogColor = waterFogColor.Lerp(_profile.NightFog, _nightWeight);
        _environment.FogLightColor = _airFogColor.Lerp(waterFogColor, _immersion);
        _environment.AmbientLightColor = _solarLighting.AirAmbient.Lerp(
            AmbientWater.Lerp(_profile.DeepAmbient, depth)
                .Lerp(_profile.NightAmbient, _nightWeight), _immersion);
        _environment.AmbientLightEnergy = Mathf.Lerp(_solarLighting.AirAmbientEnergy,
            AmbientEnergy * Mathf.Lerp(1f, _profile.DeepAmbientFactor, depth) *
                _solarWater.UnderwaterAmbient, _immersion);
        _sun.LightColor = _solarLighting.DirectColor.Lerp(
            SunColor.Lerp(_solarLighting.DirectColor, 0.65f * _solarLighting.WarmStrength), _immersion);
        var airSun = (_profile.Id == WorldEnvironmentProfile.Pond ? 1f : SunEnergy) *
            _solarLighting.DirectEnergyFactor;
        var waterSun = SunEnergy * _solarLighting.DirectEnergyFactor *
            Mathf.Lerp(1f, _profile.DeepLightFactor, depth);
        var moon = SolarLightingState.MoonVisibility(Solar);
        var moonEnergy = _profile.Id == WorldEnvironmentProfile.Pond ? 0.18f : 0.03f;
        _sun.LightEnergy = Mathf.Lerp(airSun + 0.085f * moon,
            waterSun + moonEnergy * moon * Mathf.Lerp(1f, _profile.DeepLightFactor, depth), _immersion);
        _sun.LightVolumetricFogEnergy = 0.8f * _solarLighting.VolumetricFactor * _immersion;
    }

    private void ApplySolarEffects()
    {
        _shaftMaterial?.SetShaderParameter("shaft_brightness", ShaftBrightness * _immersion * _solarWater.ShaftEnergy);
        foreach (var decal in _caustics)
        {
            if (decal is null) continue;
            decal.EmissionEnergy = CausticEnergy * _immersion * _solarWater.CausticEnergy;
            decal.Modulate = _solarWater.CausticTint;
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
        _skyMaterial.SetShaderParameter("shallow_horizon", new Vector3(_profile.SkyHorizon.R, _profile.SkyHorizon.G, _profile.SkyHorizon.B));
        _skyMaterial.SetShaderParameter("deep_horizon", new Vector3(_profile.DeepSkyHorizon.R, _profile.DeepSkyHorizon.G, _profile.DeepSkyHorizon.B));
        _skyMaterial.SetShaderParameter("shallow_zenith", new Vector3(_profile.SkyZenith.R, _profile.SkyZenith.G, _profile.SkyZenith.B));
        _skyMaterial.SetShaderParameter("deep_zenith", new Vector3(_profile.DeepSkyZenith.R, _profile.DeepSkyZenith.G, _profile.DeepSkyZenith.B));
        _skyMaterial.SetShaderParameter("overcast", _profile.Id == WorldEnvironmentProfile.Pond ? 1f : 0f);
        _skyMaterial.SetShaderParameter("air_horizon", ToVector(_solarLighting.AirHorizon));
        _skyMaterial.SetShaderParameter("air_zenith", ToVector(_solarLighting.AirZenith));
        _skyMaterial.SetShaderParameter("warm_horizon", ToVector(_solarLighting.WarmHorizon));
        _skyMaterial.SetShaderParameter("direct_color", ToVector(_solarLighting.DirectColor));
        _skyMaterial.SetShaderParameter("warm_strength", _solarLighting.WarmStrength);
        _skyMaterial.SetShaderParameter("sun_glow", _solarLighting.SunGlow);
        _environment!.Sky = new Sky { SkyMaterial = _skyMaterial };
        _environment.BackgroundMode = Godot.Environment.BGMode.Sky;
    }

    private void AddSurface(Node3D main)
    {
        // The mesh shares the generated Pond water plane with the swim boundary.
        // The upper box is only a size anchor in Pond, with collision disabled.
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
        var seabed = main.GetNode<SeabedSurface>("Habitat/Seabed");
        var pondWaterHeight = 0f;
        if (seabed.Basin.IsPond && !seabed.TryGetPondWaterSurfaceHeight(out pondWaterHeight))
        {
            GD.PushError("FISH_ATMOSPHERE_POND_WATERLINE_INVALID action=check-generated-surface");
            return;
        }
        var plane = new PlaneMesh
        {
            Size = new Vector2(extent.X + 14.0f, extent.Z + 14.0f),
            SubdivideWidth = 48,
            SubdivideDepth = 48,
        };
        var buildStart = Time.GetTicksUsec();
        Mesh waterMesh = seabed.Basin.IsPond ? PondSurfaceGeometry.Build(seabed.Basin) : plane;
        if (seabed.Basin.IsPond)
            GD.Print($"POND_SURFACE_READY vertices={waterMesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array().Length} build_ms={(Time.GetTicksUsec() - buildStart) / 1000.0:F2} single_mesh=1");
        _surfaceMaterial = new ShaderMaterial { Shader = shader };
        _surfaceMaterial.SetShaderParameter("air_surface_enabled", 1f);
        _surfaceMaterial.SetShaderParameter("shore_mask_enabled", seabed.Basin.IsPond ? 1f : 0f);
        _surfaceMaterial.SetShaderParameter("air_fog_begin", _profile.AirFogBegin);
        _surfaceMaterial.SetShaderParameter("air_fog_end", _profile.AirFogEnd);
        _surfaceMaterial.SetShaderParameter("water_scatter", new Vector3(_profile.SurfaceScatter.R,
            _profile.SurfaceScatter.G, _profile.SurfaceScatter.B));
        _surfaceMaterial.SetShaderParameter("underwater_scatter", ToVector(_profile.UnderwaterSurfaceScatter));
        _surfaceMaterial.SetShaderParameter("surface_extinction", _profile.SurfaceExtinctionPerMeter);
        _surfaceMaterial.SetShaderParameter("reflection_strength", _profile.SurfaceReflectionStrength);
        _surfaceMaterial.SetShaderParameter("reflection_distortion", _profile.SurfaceReflectionDistortion);
        _surfaceMaterial.SetShaderParameter("ripple_strength", _profile.SurfaceRippleStrength);
        _surfaceMaterial.SetShaderParameter("fine_normal_strength", _profile.SurfaceFineNormalStrength);
        _surfaceMaterial.SetShaderParameter("surface_daylight", _solarWater.SurfaceDaylight);
        _surfaceMaterial.SetShaderParameter("sun_direction", _sun!.GlobalTransform.Basis.Z.Normalized());
        _surfaceMaterial.SetShaderParameter("surface_time", 0.0f);
        _surfaceMaterial.SetShaderParameter("wave_height", SurfaceWaveHeight);
        _surfaceMaterial.SetShaderParameter("wave_speed", SurfaceWaveSpeed);
        _surfaceMaterial.SetShaderParameter("wave_detail", SurfaceWaveDetail);
        _surfaceMaterial.SetShaderParameter("surface_half_size", (extent.X + 14.0f) * 0.5f);
        _surfaceMaterial.SetShaderParameter("surface_dark", new Vector3(_profile.SurfaceDark.R, _profile.SurfaceDark.G, _profile.SurfaceDark.B));
        _surfaceMaterial.SetShaderParameter("surface_light", new Vector3(_profile.SurfaceLight.R, _profile.SurfaceLight.G, _profile.SurfaceLight.B));
        _surfaceMaterial.SetShaderParameter("overcast", _profile.Id == WorldEnvironmentProfile.Pond ? 1f : 0f);
        _surface = new MeshInstance3D
        {
            Name = "OverheadSurface",
            Mesh = waterMesh,
            MaterialOverride = _surfaceMaterial,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            CustomAabb = new Aabb(new Vector3(-(extent.X + 14.0f) * 0.5f, -0.35f, -(extent.Z + 14.0f) * 0.5f),
                new Vector3(extent.X + 14.0f, 0.7f, extent.Z + 14.0f)),
            Position = _effects!.ToLocal(upper.GlobalPosition) + new Vector3(0,
                seabed.Basin.IsPond
                    ? pondWaterHeight - upper.GlobalPosition.Y
                    : extent.Y * 0.5f + 0.34f, 0),
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
        // The shader steers the top vertices; keep this mesh present at night.
        var sunUp = Vector3.Up;
        Vector2[] centers = [new(-17, -14), new(-10, 9), new(4, -12), new(11, 12),
            new(21, -5), new(-22, 19), new(18, 21), new(-3, 22)];
        var vertices = new Vector3[centers.Length * 6];
        var uvs = new Vector2[vertices.Length];
        var colors = new Color[vertices.Length];
        var triangles = new[] { 0, 1, 2, 0, 2, 3 };
        for (var shaft = 0; shaft < centers.Length; shaft++)
        {
            var seed = decal.ToLocal(new Vector3(centers[shaft].X, 0, centers[shaft].Y));
            var pixelX = Mathf.Clamp(Mathf.RoundToInt((seed.X / decal.Size.X + 0.5f) * 255f), 10, 245);
            var pixelY = Mathf.Clamp(Mathf.RoundToInt((seed.Z / decal.Size.Z + 0.5f) * 255f), 10, 245);
            var strongest = -1f;
            var bestX = -1;
            var bestY = -1;
            for (var dy = -32; dy <= 32; dy++)
            for (var dx = -32; dx <= 32; dx++)
            {
                var sx = Mathf.Clamp(pixelX + dx, 3, 252);
                var sy = Mathf.Clamp(pixelY + dy, 3, 252);
                var sample = image.GetPixel(sx, sy).A;
                if (sample <= Mathf.Max(strongest, 0.02f)) continue;
                var candidate = decal.ToGlobal(new Vector3(((sx + 0.5f) / 256f - 0.5f) * decal.Size.X, 0,
                    ((sy + 0.5f) / 256f - 0.5f) * decal.Size.Z));
                var floorY = seabed.GetHeightAt(candidate.X, candidate.Z) + 0.3f;
                if (floorY < decal.GlobalPosition.Y - decal.Size.Y * 0.5f + 0.3f ||
                    floorY > _surfaceHeight - 1f) continue;
                strongest = sample;
                bestX = sx;
                bestY = sy;
            }
            if (bestX < 0)
            {
                GD.PushError("FISH_ATMOSPHERE_NO_LIT_BASIN_SHAFT_ANCHOR");
                return;
            }
            var gx = image.GetPixel(bestX + 2, bestY).A - image.GetPixel(bestX - 2, bestY).A;
            var gy = image.GetPixel(bestX, bestY + 2).A - image.GetPixel(bestX, bestY - 2).A;
            var tangentLocal = new Vector3(-gy, 0, gx).Normalized();
            if (tangentLocal.LengthSquared() < 0.01f) tangentLocal = Vector3.Right;
            var tangent = (decal.GlobalTransform.Basis * tangentLocal).Normalized();
            var localFloor = new Vector3(((bestX + 0.5f) / 256f - 0.5f) * decal.Size.X, 0,
                ((bestY + 0.5f) / 256f - 0.5f) * decal.Size.Z);
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
                colors[index] = new Color(0.11f + shaft * 0.103f, (shaft + 0.5f) / 8f,
                    Mathf.Clamp((bottom.Y - _deepReferenceHeight) / (_surfaceHeight - _deepReferenceHeight), 0f, 1f), 1);
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
        _shaftMaterial.SetShaderParameter("shaft_color", new Vector3(_profile.ShaftColor.R, _profile.ShaftColor.G, _profile.ShaftColor.B));
        _shafts = new MeshInstance3D
        {
            Name = "SunShafts",
            Mesh = mesh,
            MaterialOverride = _shaftMaterial,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            CustomAabb = new Aabb(new Vector3(-80f, _deepReferenceHeight - 5f, -80f),
                new Vector3(160f, _surfaceHeight - _deepReferenceHeight + 10f, 160f)),
        };
        _effects.AddChild(_shafts);
    }

    private void AddCaustics()
    {
        var texture = MakeCausticTexture();
        var planeWidth = (_surface?.Mesh as PlaneMesh)?.Size.X ?? 100f;
        var largeWorld = planeWidth > 100f;
        var horizontal = largeWorld ? planeWidth : 100f;
        var vertical = largeWorld ? Mathf.Max(22f, _surfaceHeight - _deepReferenceHeight + 20f) : 22f;
        for (var index = 0; index < _caustics.Length; index++)
        {
            var decal = new Decal
            {
                Name = $"CausticProjection{index + 1}",
                Position = new Vector3(0, largeWorld ? 14f - vertical * .5f : 3f, 0),
                Rotation = new Vector3(0, 0.12f, 0),
                Size = new Vector3(horizontal, vertical, horizontal),
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
        if (_environment is not null && GetParent()?.GetNodeOrNull<WorldEnvironment>("WorldEnvironment") is { } world &&
            world.Environment == _environment) world.Environment = null;
    }

    public bool ParametersAreFinite() =>
        ColorIsBounded(BackgroundWater) && ColorIsBounded(AmbientWater) &&
        ColorIsBounded(DistanceFogColor) && ColorIsBounded(VolumetricWaterColor) &&
        ColorIsBounded(SunColor) && ColorIsBounded(CausticColor) && ColorIsBounded(MoteColor) &&
        float.IsFinite(VolumetricDensity) && VolumetricDensity is > 0 and <= 0.06f &&
        float.IsFinite(VisibilityRangeMeters) && VisibilityRangeMeters is >= 18 and <= 60 &&
        float.IsFinite(LowerWaterFog) && LowerWaterFog is >= 0 and <= 0.05f &&
        float.IsFinite(LowerFogStartHeight) && LowerFogStartHeight is >= -8 and <= 8 &&
        float.IsFinite(FogLength) && FogLength is >= 20 and <= 90 &&
        float.IsFinite(AmbientEnergy) && AmbientEnergy is >= 0 and <= 1 &&
        float.IsFinite(VolumetricAmbientInject) && VolumetricAmbientInject is >= 0 and <= 1 &&
        float.IsFinite(VolumetricAnisotropy) && VolumetricAnisotropy is >= -0.5f and <= 0.5f &&
        float.IsFinite(SunEnergy) && SunEnergy is > 0 and <= 3 &&
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

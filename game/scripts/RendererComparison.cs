#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Diagnostic presentation layered over the unchanged P1 main scene.</summary>
public partial class RendererComparison : Node3D
{
    private const float CausticStrength = 0.11f;
    private const float ScreenStrength = 0.07f;
    private const string Vendor = "res://assets/vendor-local/p2-environment/";
    private static readonly string[] PreparedMeshes =
    [
        "polyhaven/rock_face_02/rock_face_02_2k.gltf",
        "polyhaven/coastal_cliff_01/coastal_cliff_01_2k.gltf",
        "polyhaven/boulder_01/boulder_01_2k.gltf",
        "smithsonian/acropora-cervicornis/Acropora_cervicornis-150k-4096_std.glb",
        "smithsonian/acropora-palmata/usnm_3968-100k-2048_std.glb",
        "smithsonian/meandrina-meandrites/usnm_1174917-100k-2048_std.glb",
        "smithsonian/montipora-danae/usnm_307-100k-2048_std.glb",
    ];
    private static readonly string[] PreparedGround = ["Ground060", "Gravel041", "Rock063"];
    private static readonly string[] GroundMaps = ["AmbientOcclusion", "Color", "Displacement", "NormalGL", "Roughness"];
    private static readonly string[] RockMaps = ["arm", "diff", "nor_gl"];

    private Node3D _effects = null!;
    private Godot.Environment _environment = null!;
    private CanvasLayer? _screenLayer;
    private bool _checkMode;

    public override void _Ready()
    {
        _checkMode = OS.GetCmdlineUserArgs().Contains("--comparison-check");
        try
        {
            var main = GetNode<Node3D>("Main");
            var fish = main.GetNode<FishController>("Fish");
            var camera = main.GetNode<Camera3D>("Fish/CameraRig/Camera3D");
            var method = RenderingServer.GetCurrentRenderingMethod();
            if (method != "gl_compatibility" && method != "forward_plus")
                throw new InvalidOperationException($"Unsupported comparison renderer: {method}");
            if (fish.GetNodeOrNull<Node3D>("PlayerVisual") is null)
                throw new InvalidOperationException("P1 Fish3 did not load");

            _environment = main.GetNode<WorldEnvironment>("WorldEnvironment").Environment;
            _environment.FogEnabled = true;
            _environment.FogLightColor = new Color(0.045f, 0.27f, 0.31f);
            _environment.FogDensity = 0.055f;
            _environment.FogSunScatter = 0.12f;
            _environment.AmbientLightColor = new Color(0.38f, 0.63f, 0.67f);
            _environment.AmbientLightEnergy = 0.45f;

            var presentation = new Node3D { Name = "ComparisonPresentation" };
            AddChild(presentation);
            _effects = new Node3D { Name = "Effects" };
            presentation.AddChild(_effects);

            var rock = LoadScene(Vendor + PreparedMeshes[0]);
            AddAsset(presentation, rock, "RockFace02", new Vector3(-5.1f, -3.85f, 0.6f), 0.55f);
            var coral = LoadScene(Vendor + PreparedMeshes[4]);
            AddAsset(presentation, coral, "AcroporaPalmata", new Vector3(5.2f, -3.75f, -1.3f), 1.4f);
            AddCaustics();
            AddMotes();
            if (method == "forward_plus") AddForwardScreen();

            var off = OS.GetCmdlineUserArgs().Contains("--comparison-effects=off");
            SetEffectsEnabled(!off);
            GD.Print($"FISH_RENDER_COMPARISON method={method} driver={RenderingServer.GetCurrentRenderingDriverName()} effects={(off ? "off" : "on")} scene=main-plus-bounded-presentation");

            if (_checkMode)
            {
                CheckPreparedAssets();
                CheckIsolation(main, fish, camera, method);
                GD.Print("RENDER_COMPARISON_OK assets=complete fish=camera=single movement=unchanged ui=above-screen effects=finite");
                GetTree().Quit();
            }
        }
        catch (Exception error)
        {
            GD.PushError($"RENDER_COMPARISON_FAILED {error}");
            if (_checkMode) GetTree().Quit(1);
        }
    }

    private static PackedScene LoadScene(string path) =>
        ResourceLoader.Load<PackedScene>(path) ?? throw new InvalidOperationException($"Cannot load {path}");

    private static void AddAsset(Node3D parent, PackedScene source, string name, Vector3 position, float scale)
    {
        var node = source.Instantiate<Node3D>();
        node.Name = name;
        parent.AddChild(node);
        node.Position = position;
        node.Scale = Vector3.One * scale;
    }

    private void AddCaustics()
    {
        var shader = ResourceLoader.Load<Shader>("res://shaders/comparison_caustics.gdshader")
            ?? throw new InvalidOperationException("Caustic shader did not load");
        var material = new ShaderMaterial { Shader = shader };
        material.SetShaderParameter("strength", CausticStrength);
        var mesh = new MeshInstance3D
        {
            Name = "AnimatedCaustics",
            Position = new Vector3(0, -4.085f, 0),
            Mesh = new PlaneMesh { Size = new Vector2(18, 18) },
            MaterialOverride = material,
        };
        _effects.AddChild(mesh);
    }

    private void AddMotes()
    {
        var moteMaterial = new StandardMaterial3D
        {
            AlbedoColor = new Color(0.62f, 0.85f, 0.82f, 0.25f),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        };
        var motes = new CpuParticles3D
        {
            Name = "SuspendedMotes",
            Position = new Vector3(0, 0.4f, -3),
            Amount = 24,
            Lifetime = 8,
            EmissionShape = CpuParticles3D.EmissionShapeEnum.Box,
            EmissionBoxExtents = new Vector3(8, 3.5f, 8),
            Direction = Vector3.Up,
            Spread = 8,
            Gravity = Vector3.Zero,
            InitialVelocityMin = 0.015f,
            InitialVelocityMax = 0.045f,
            Mesh = new SphereMesh { Radius = 0.012f, Height = 0.024f, Material = moteMaterial },
        };
        _effects.AddChild(motes);
    }

    private void AddForwardScreen()
    {
        // A screen-copy tint is diagnostic Forward+ coverage, kept below all P1 UI layers.
        var shader = ResourceLoader.Load<Shader>("res://shaders/comparison_screen.gdshader")
            ?? throw new InvalidOperationException("Forward+ screen shader did not load");
        _screenLayer = new CanvasLayer { Name = "ForwardScreenTreatment", Layer = 0 };
        AddChild(_screenLayer);
        var rect = new ColorRect
        {
            Name = "ScreenTint",
            Color = Colors.White,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Material = new ShaderMaterial { Shader = shader },
        };
        ((ShaderMaterial)rect.Material).SetShaderParameter("strength", ScreenStrength);
        _screenLayer.AddChild(rect);
        // CanvasLayer is not a Control, so size the root rectangle from the viewport.
        rect.Size = GetViewport().GetVisibleRect().Size;
        GetViewport().SizeChanged += () => rect.Size = GetViewport().GetVisibleRect().Size;
    }

    private void SetEffectsEnabled(bool enabled)
    {
        _environment.FogEnabled = enabled;
        _effects.Visible = enabled;
        if (_screenLayer is not null) _screenLayer.Visible = enabled;
    }

    private static void CheckPreparedAssets()
    {
        foreach (var relative in PreparedMeshes)
        {
            var scene = LoadScene(Vendor + relative);
            var instance = scene.Instantiate<Node3D>();
            if (!HasMesh(instance)) throw new InvalidOperationException($"No mesh in {relative}");
            instance.Free();
        }
        foreach (var name in PreparedGround)
        foreach (var map in GroundMaps)
        {
            var path = $"{Vendor}ambientcg/{name}/{name}_2K-JPG_{map}.jpg";
            if (ResourceLoader.Load<Texture2D>(path) is null)
                throw new InvalidOperationException($"Cannot load {path}");
        }
        foreach (var name in new[] { "rock_face_02", "coastal_cliff_01", "boulder_01" })
        {
            var stem = $"{Vendor}polyhaven/{name}/";
            if (!FileAccess.FileExists(stem + name + ".bin"))
                throw new InvalidOperationException($"Missing {stem + name + ".bin"}");
            foreach (var map in RockMaps)
            {
                var path = $"{stem}textures/{name}_{map}_2k.jpg";
                if (ResourceLoader.Load<Texture2D>(path) is null)
                    throw new InvalidOperationException($"Cannot load {path}");
            }
        }
    }

    private static bool HasMesh(Node node)
    {
        if (node is MeshInstance3D { Mesh: not null }) return true;
        foreach (var child in node.GetChildren())
            if (HasMesh(child)) return true;
        return false;
    }

    private void CheckIsolation(Node3D main, FishController fish, Camera3D camera, string method)
    {
        var position = fish.GlobalPosition;
        var velocity = fish.Velocity;
        var tuning = fish.Tuning;
        var cameraId = camera.GetInstanceId();
        SetEffectsEnabled(false);
        SetEffectsEnabled(true);
        if (fish.GlobalPosition != position || fish.Velocity != velocity || fish.Tuning != tuning ||
            camera.GetInstanceId() != cameraId || !camera.Current)
            throw new InvalidOperationException("Effect toggle changed P1 movement/camera state");
        if (CountNodes<FishController>(this) != 1 || CountNodes<Camera3D>(this) != 1)
            throw new InvalidOperationException("Comparison duplicated fish or camera");
        var caustic = (ShaderMaterial)_effects.GetNode<MeshInstance3D>("AnimatedCaustics").MaterialOverride;
        var causticStrength = (float)caustic.GetShaderParameter("strength");
        var screenStrength = _screenLayer is null ? 0.0f :
            (float)((ShaderMaterial)_screenLayer.GetNode<ColorRect>("ScreenTint").Material).GetShaderParameter("strength");
        if (_environment.FogDensity <= 0 || !float.IsFinite(_environment.FogDensity) ||
            !float.IsFinite(_environment.FogSunScatter) ||
            !float.IsFinite(causticStrength) || causticStrength is < 0 or > 0.3f ||
            !float.IsFinite(screenStrength) || screenStrength is < 0 or > 0.2f ||
            _effects.GetNode<CpuParticles3D>("SuspendedMotes").Amount > 32)
            throw new InvalidOperationException("Effect parameters are unbounded");
        var help = main.GetNode<CanvasLayer>("PrototypeHelp");
        var shell = main.GetNode<Node>("PlayShell").GetChildren().OfType<CanvasLayer>().Single();
        if (method == "forward_plus" && (_screenLayer is null ||
            _screenLayer.Layer >= help.Layer || _screenLayer.Layer >= shell.Layer))
            throw new InvalidOperationException("Screen treatment covers UI");
        if (method == "gl_compatibility" && _screenLayer is not null)
            throw new InvalidOperationException("Forward+ resource loaded in Compatibility");
    }

    private static int CountNodes<T>(Node root) where T : Node
    {
        var count = root is T ? 1 : 0;
        foreach (var child in root.GetChildren()) count += CountNodes<T>(child);
        return count;
    }
}

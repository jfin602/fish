using Godot;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json.Nodes;

namespace Fish;

/// <summary>Focused source-bound storage checks, launched by --control-preset-check.</summary>
public static class ControlPresetChecks
{
    private static void Require(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException(label);
    }
    private static void Reject(Action action, string label)
    {
        try { action(); }
        catch (ArgumentException) { return; }
        catch (InvalidOperationException) { return; }
        throw new InvalidOperationException("Expected rejection: " + label);
    }

    public static void Run()
    {
        var directory = Path.Combine(Path.GetTempPath(), "fish-control-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "control-presets.json");
        var legacy = Path.Combine(directory, "preferences.cfg");
        try
        {
            var store = new ControlPresetStore(path, legacy);
            store.Load();
            Require(store.CurrentId == ControlPreset.KeyboardId && store.List().Count == 5,
                "fresh deterministic keyboard fallback");
            var builtins = ControlPreset.Builtins();
            Require(builtins.Count == 5 && builtins.Select(p => p.Id).Distinct().Count() == 5 &&
                builtins.Select(p => p.Name).Distinct().Count() == 5 &&
                builtins[0].Bindings(ControlAction.Thrust).Single().Code == (int)Key.W &&
                builtins[0].Bindings(ControlAction.PitchUp).Single().Code == (int)Key.Up &&
                builtins[1].MouseX.Steering == SteeringAxis.Yaw &&
                builtins[1].MouseY.Steering == SteeringAxis.Pitch &&
                builtins[1].Bindings(ControlAction.Thrust).Single().Code == (int)MouseButton.Left &&
                builtins[1].Bindings(ControlAction.Slow).Single().Code == (int)MouseButton.Right &&
                builtins[1].Bindings(ControlAction.PauseOptions).Any(b =>
                    b.Device == BindingDevice.MouseButton && b.Code == (int)MouseButton.Middle) &&
                builtins[2].Bindings(ControlAction.YawLeft).Single().Code == (int)Key.A &&
                builtins[2].Bindings(ControlAction.PitchDown).Single().Code == (int)Key.Down &&
                builtins[3].Id == ControlPreset.Hybrid2Id &&
                builtins[3].CameraControl == CameraControlPolicy.MouseOrbit360 &&
                builtins[3].MouseX.Steering == SteeringAxis.None &&
                builtins[3].MouseY.Steering == SteeringAxis.None &&
                builtins[3].Bindings(ControlAction.Thrust).Single() ==
                    new ControlBinding(BindingDevice.MouseButton, (int)MouseButton.Left) &&
                builtins[3].Bindings(ControlAction.Slow).Single() ==
                    new ControlBinding(BindingDevice.MouseButton, (int)MouseButton.Right) &&
                builtins[3].Bindings(ControlAction.YawLeft).Single().Code == (int)Key.A &&
                builtins[3].Bindings(ControlAction.YawRight).Single().Code == (int)Key.D &&
                builtins[3].Bindings(ControlAction.PitchUp).Single().Code == (int)Key.W &&
                builtins[3].Bindings(ControlAction.PitchDown).Single().Code == (int)Key.S &&
                builtins[3].Bindings(ControlAction.PauseOptions).Select(b => b.Code)
                    .OrderBy(code => code).SequenceEqual(new[] { (int)MouseButton.Middle, (int)Key.Escape }.OrderBy(code => code)) &&
                builtins.Take(3).All(p => p.CameraControl == CameraControlPolicy.LegacyPan) &&
                builtins[4].Id == ControlPreset.AdaptiveFollowId &&
                builtins[4].CameraControl == CameraControlPolicy.AdaptiveFollow &&
                builtins[4].MouseX.Steering == SteeringAxis.Yaw &&
                builtins[4].MouseY.Steering == SteeringAxis.Pitch &&
                builtins[4].Bindings(ControlAction.PitchUp).Select(b => b.Code)
                    .SequenceEqual(new[] { (int)Key.W, (int)Key.Up }) &&
                builtins[4].Bindings(ControlAction.PitchDown).Select(b => b.Code)
                    .SequenceEqual(new[] { (int)Key.S, (int)Key.Down }) &&
                builtins[4].Bindings(ControlAction.Thrust).Single() ==
                    new ControlBinding(BindingDevice.MouseButton, (int)MouseButton.Left) &&
                builtins[4].Bindings(ControlAction.Slow).Single() ==
                    new ControlBinding(BindingDevice.MouseButton, (int)MouseButton.Right),
                "templates match source control requests");
            Reject(() => store.Rename(ControlPreset.KeyboardId, "Changed"), "builtin rename");
            Reject(() => store.Delete(ControlPreset.MouseId), "builtin delete");
            Reject(() => store.Update(builtins[0].WithName("Changed")), "builtin update");

            var copy = store.Duplicate(ControlPreset.HybridId, "My Hybrid");
            Require(copy.Id.StartsWith("custom:") && ControlPreset.IsValidId(copy.Id), "stable custom identity");
            Reject(() => store.Duplicate(ControlPreset.MouseId, "my hybrid"), "duplicate profile name");
            Reject(() => store.Update(copy.WithBindings(ControlAction.Slow,
                new ControlBinding(BindingDevice.PhysicalKey, (int)Key.W))), "action conflict");
            Reject(() => copy.WithBindings(ControlAction.Thrust,
                new ControlBinding(BindingDevice.PhysicalKey, ControlPreset.RecoveryKey)), "reserved Escape");
            Reject(() => copy.WithBindings(ControlAction.Thrust,
                new ControlBinding(BindingDevice.MouseButton, ControlPreset.RecoveryMouseButton)),
                "reserved mouse recovery");
            Reject(() => copy.WithMouse(new(SteeringAxis.Yaw), new(SteeringAxis.Yaw), 0.12f),
                "axis target collision");
            Reject(() => copy.WithBindings(ControlAction.Thrust,
                new ControlBinding(BindingDevice.MouseButton, (int)MouseButton.WheelUp)), "wheel cannot be captured");
            Reject(() => copy.WithBindings(ControlAction.Thrust), "unbound swim");
            Reject(() => copy.WithBindings(ControlAction.Slow), "unbound slow");
            Reject(() => builtins[4].Copy("custom:" + Guid.NewGuid().ToString("N"), "Reserved Shift")
                .WithBindings(ControlAction.YawLeft, new ControlBinding(BindingDevice.PhysicalKey, (int)Key.Shift)),
                "Adaptive Follow reserves Shift");
            var adaptiveWithC = builtins[4].Copy("custom:" + Guid.NewGuid().ToString("N"), "C Steering")
                .WithBindings(ControlAction.YawLeft, new ControlBinding(BindingDevice.PhysicalKey, (int)Key.C));
            Require(adaptiveWithC.Bindings(ControlAction.YawLeft).Single().Code == (int)Key.C,
                "C is available for custom Adaptive Follow bindings");
            var missingActions = Enum.GetValues<ControlAction>().ToDictionary(action => action,
                action => copy.Bindings(action).ToArray());
            missingActions.Remove(ControlAction.Slow);
            Reject(() => new ControlPreset(copy.Id, copy.Name, missingActions,
                copy.MouseX, copy.MouseY, copy.MouseSensitivity), "missing action");
            Reject(() => copy.WithName(new string('x', ControlPreset.MaxNameLength + 1)), "bounded name");
            Require(ControlPreset.RecoveryMouseButton == (int)MouseButton.Middle &&
                copy.Bindings(ControlAction.PauseOptions).Any(b => b.Code == ControlPreset.RecoveryKey),
                "keyboard and mouse recovery policy");
            foreach (var builtIn in builtins)
            {
                var resolver = new ProfileSwimInput();
                resolver.Select(builtIn);
                if (builtIn.Id is ControlPreset.MouseId or ControlPreset.Hybrid2Id or ControlPreset.AdaptiveFollowId)
                    resolver.Accept(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true }, true);
                else
                    resolver.Accept(new InputEventKey { PhysicalKeycode = Key.W, Pressed = true }, true);
                var intent = resolver.Consume(1f / 60f, new SwimTuning(), true,
                    builtIn.MouseSensitivity, builtIn.MouseY.Inverted);
                Require(intent == new SwimIntent(1, 0, 0, 0), "equivalent thrust intent: " + builtIn.Id);
                var state = SwimMotor.Step(new SwimState(), intent, 1f / 60f, new SwimTuning());
                var reference = SwimMotor.Step(new SwimState(), new SwimIntent(1, 0, 0, 0),
                    1f / 60f, new SwimTuning());
                Require(state.Speed == reference.Speed && state.Velocity == reference.Velocity &&
                    state.Yaw == reference.Yaw && state.Pitch == reference.Pitch,
                    "shared motor result parity: " + builtIn.Id);
            }
            var hybrid2Input = new ProfileSwimInput();
            hybrid2Input.Select(builtins[3]);
            hybrid2Input.CaptureStarted();
            hybrid2Input.Accept(new InputEventMouseMotion { Relative = new Vector2(20, -15) }, true);
            hybrid2Input.Accept(new InputEventMouseMotion { Relative = new Vector2(20, -15) }, true);
            Require(hybrid2Input.Consume(1f / 60f, new SwimTuning(), true, 0.12f, false) ==
                new SwimIntent(0, 0, 0, 0), "Hybrid 2 mouse motion never steers fish");
            hybrid2Input.Accept(new InputEventKey { PhysicalKeycode = Key.W, Pressed = true }, true);
            hybrid2Input.Accept(new InputEventKey { PhysicalKeycode = Key.A, Pressed = true }, true);
            hybrid2Input.Accept(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true }, true);
            Require(hybrid2Input.Consume(1f / 60f, new SwimTuning(), true, 0.12f, false) ==
                new SwimIntent(1, 0, -1, 1), "Hybrid 2 W/A/left input");
            hybrid2Input.Accept(new InputEventKey { PhysicalKeycode = Key.W, Pressed = false }, true);
            hybrid2Input.Accept(new InputEventKey { PhysicalKeycode = Key.A, Pressed = false }, true);
            hybrid2Input.Accept(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false }, true);
            hybrid2Input.Accept(new InputEventKey { PhysicalKeycode = Key.S, Pressed = true }, true);
            hybrid2Input.Accept(new InputEventKey { PhysicalKeycode = Key.D, Pressed = true }, true);
            hybrid2Input.Accept(new InputEventMouseButton { ButtonIndex = MouseButton.Right, Pressed = true }, true);
            Require(hybrid2Input.Consume(1f / 60f, new SwimTuning(), true, 0.12f, false) ==
                new SwimIntent(0, 1, 1, -1), "Hybrid 2 S/D/right input");
            var adaptiveInput = new ProfileSwimInput();
            adaptiveInput.Select(builtins[4]);
            adaptiveInput.CaptureStarted();
            adaptiveInput.Accept(new InputEventKey { PhysicalKeycode = Key.W, Pressed = true }, true);
            Require(adaptiveInput.Consume(1f / 60f, new SwimTuning(), true, 0.12f, false) ==
                new SwimIntent(0, 0, 0, 1), "Adaptive Follow W pitches up without thrust");
            adaptiveInput.Accept(new InputEventKey { PhysicalKeycode = Key.W, Pressed = false }, true);
            adaptiveInput.Accept(new InputEventKey { PhysicalKeycode = Key.S, Pressed = true }, true);
            Require(adaptiveInput.Consume(1f / 60f, new SwimTuning(), true, 0.12f, false) ==
                new SwimIntent(0, 0, 0, -1), "Adaptive Follow S pitches down without slow");
            adaptiveInput.Accept(new InputEventKey { PhysicalKeycode = Key.S, Pressed = false }, true);
            adaptiveInput.Accept(new InputEventMouseMotion { Relative = new Vector2(300, 0) }, true);
            adaptiveInput.Consume(1f / 60f, new SwimTuning(), true, 0.12f, false);
            adaptiveInput.Accept(new InputEventMouseMotion { Relative = new Vector2(1, 0) }, true);
            Require(adaptiveInput.Consume(1f / 60f, new SwimTuning(), true, 0.12f, false).Yaw == 0,
                "Adaptive Follow filters isolated sub-2px movement");
            adaptiveInput.Accept(new InputEventMouseMotion { Relative = new Vector2(20, -15) }, true);
            var deliberate = adaptiveInput.Consume(1f / 60f, new SwimTuning(), true, 0.12f, false);
            Require(deliberate.Yaw > 0.2f && deliberate.Pitch > 0.2f,
                "Adaptive Follow preserves fast mouse steering");
            var filteredMouse = new MouseSwimInput();
            filteredMouse.OnCaptureStarted();
            filteredMouse.Accept(new InputEventMouseMotion { Relative = new Vector2(300, 0) });
            filteredMouse.Accept(new InputEventMouseMotion { Relative = new Vector2(10, 0) });
            var deliveredYaw = 0f;
            for (var tick = 0; tick < 120; tick++)
                deliveredYaw += filteredMouse.ConsumeSteering(1f / 60f, new SwimTuning(),
                    builtins[4], 0.12f, false).X;
            Require(Math.Abs(deliveredYaw - 0.48f) < 0.01f,
                "Adaptive Follow filtering preserves total deliberate turn");
            adaptiveInput.Accept(new InputEventKey { PhysicalKeycode = Key.Shift,
                Location = KeyLocation.Right, Pressed = true }, true);
            Require(!adaptiveInput.AdaptiveFreeLookHeld, "Right Shift does not enter free-look");
            adaptiveInput.Accept(new InputEventKey { PhysicalKeycode = Key.Shift,
                Location = KeyLocation.Right, Pressed = false }, true);
            adaptiveInput.Accept(new InputEventKey { PhysicalKeycode = Key.Shift,
                Location = KeyLocation.Left, Pressed = true }, true);
            adaptiveInput.Accept(new InputEventMouseMotion { Relative = new Vector2(80, 80) }, true);
            Require(adaptiveInput.AdaptiveFreeLookHeld &&
                adaptiveInput.Consume(1f / 60f, new SwimTuning(), true, 0.12f, false).Yaw == 0,
                "C routes mouse away from fish");
            adaptiveInput.Accept(new InputEventKey { PhysicalKeycode = Key.Shift,
                Location = KeyLocation.Left, Pressed = false }, true);
            Require(!adaptiveInput.AdaptiveFreeLookHeld &&
                adaptiveInput.Consume(1f / 60f, new SwimTuning(), true, 0.12f, false).Yaw == 0,
                "Left Shift release clears free-look mouse motion");
            adaptiveInput.Accept(new InputEventKey { PhysicalKeycode = Key.Shift,
                Location = KeyLocation.Left, Pressed = true }, true);
            adaptiveInput.ClearQueued();
            adaptiveInput.Select(builtins[3]);
            Require(!adaptiveInput.AllReleased, "held Left Shift blocks preset switch");
            adaptiveInput.Accept(new InputEventKey { PhysicalKeycode = Key.Shift,
                Location = KeyLocation.Left, Pressed = false }, true);
            Require(adaptiveInput.AllReleased, "Left Shift release rearms after preset switch");
            var adaptiveCopy = store.Duplicate(ControlPreset.AdaptiveFollowId, "My Adaptive");
            store.Choose(adaptiveCopy.Id);
            store.Save();
            var adaptiveLoaded = new ControlPresetStore(path, legacy);
            adaptiveLoaded.Load();
            Require(adaptiveLoaded.Current.CameraControl == CameraControlPolicy.AdaptiveFollow,
                "Adaptive Follow custom policy round trip");
            store.Delete(adaptiveCopy.Id);
            var orbitCopy = store.Duplicate(ControlPreset.Hybrid2Id, "My Orbit");
            Require(orbitCopy.CameraControl == CameraControlPolicy.MouseOrbit360, "duplicate preserves orbit policy");
            store.Choose(orbitCopy.Id);
            store.Save();
            var orbitLoaded = new ControlPresetStore(path, legacy);
            orbitLoaded.Load();
            Require(orbitLoaded.Current.CameraControl == CameraControlPolicy.MouseOrbit360 &&
                orbitLoaded.Current.MouseX.Steering == SteeringAxis.None, "orbit policy round trip");
            store.Delete(orbitCopy.Id);
            copy = copy.WithMouse(new(SteeringAxis.Pitch), new(SteeringAxis.Yaw, true), 0.21f)
                .WithBindings(ControlAction.Thrust,
                    new ControlBinding(BindingDevice.PhysicalKey, (int)Key.W),
                    new ControlBinding(BindingDevice.MouseButton, (int)MouseButton.Left));
            store.Update(copy);
            store.Choose(copy.Id);
            store.Save();
            Require(File.ReadAllText(path).Contains("\"schemaVersion\": 2"), "explicit schema version");
            var oldSchema = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            oldSchema["schemaVersion"] = 1;
            foreach (var profile in oldSchema["profiles"]!.AsArray())
                profile!.AsObject().Remove("cameraControl");
            File.WriteAllText(path, oldSchema.ToJsonString());
            var v1Loaded = new ControlPresetStore(path, legacy);
            v1Loaded.Load();
            Require(v1Loaded.CurrentId == copy.Id &&
                v1Loaded.Current.CameraControl == CameraControlPolicy.LegacyPan,
                "schema 1 custom profile migrates to existing camera behavior");
            store.Save();
            var loaded = new ControlPresetStore(path, legacy);
            loaded.Load();
            Require(loaded.CurrentId == copy.Id && loaded.Current.MouseX.Steering == SteeringAxis.Pitch &&
                loaded.Current.MouseY.Inverted && Math.Abs(loaded.Current.MouseSensitivity - 0.21f) < 0.00001f &&
                loaded.Current.Bindings(ControlAction.Thrust).Count == 2, "custom profile round trip");
            loaded.Reset(copy.Id);
            Require(loaded.Current.MouseX.Steering == SteeringAxis.Yaw &&
                Math.Abs(loaded.Current.MouseSensitivity - 0.12f) < 0.00001f,
                "custom reset from immutable template");
            loaded.Rename(copy.Id, "Renamed");
            loaded.Delete(copy.Id);
            Require(loaded.CurrentId == ControlPreset.KeyboardId && loaded.List().Count == 5,
                "delete current safely falls back");
            loaded.Save();

            var config = new ConfigFile();
            config.SetValue("display", "mode", DisplayPreferences.WindowedFullscreen);
            config.SetValue("input", "mode", DisplayPreferences.MouseMode);
            config.SetValue("input", "mouse_sensitivity", 0.28f);
            config.SetValue("input", "invert_mouse_y", true);
            Require(config.Save(legacy) == Error.Ok, "write legacy fixture");
            var legacyBytes = File.ReadAllBytes(legacy);
            File.Delete(path);
            var migrated = new ControlPresetStore(path, legacy);
            migrated.Load();
            Require(migrated.CurrentId.StartsWith("custom:") &&
                Math.Abs(migrated.Current.MouseSensitivity - 0.28f) < 0.00001f &&
                migrated.Current.MouseY.Inverted, "legacy mouse mode and settings migration");
            migrated.Save();
            Require(File.ReadAllBytes(legacy).SequenceEqual(legacyBytes),
                "legacy display and input preferences unchanged");
            config.SetValue("input", "mode", DisplayPreferences.KeyboardMode);
            Require(config.Save(legacy) == Error.Ok, "write keyboard migration fixture");
            File.Delete(path);
            var keyboardMigrated = new ControlPresetStore(path, legacy);
            keyboardMigrated.Load();
            Require(keyboardMigrated.CurrentId == ControlPreset.KeyboardId &&
                keyboardMigrated.List().Count == 6 &&
                keyboardMigrated.List().Any(p => p.Name == "Legacy Mouse Settings" && p.MouseY.Inverted),
                "keyboard mode stays active while prior mouse settings remain available");
            config.SetValue("input", "mode", DisplayPreferences.MouseMode);
            Require(config.Save(legacy) == Error.Ok, "restore mouse migration fixture");
            legacyBytes = File.ReadAllBytes(legacy);
            migrated = new ControlPresetStore(path, legacy);
            migrated.Load();
            migrated.Save();
            var persisted = new ControlPresetStore(path, legacy);
            persisted.Load();
            Require(persisted.CurrentId == migrated.CurrentId, "migration identity persisted");

            var validJson = File.ReadAllText(path);
            var invalidBinding = Regex.Replace(validJson, "\\\"code\\\": 1", "\"code\": 99999999", RegexOptions.None);
            Require(invalidBinding != validJson, "bad binding fixture was changed");
            File.WriteAllText(path, invalidBinding);
            var invalid = new ControlPresetStore(path, legacy);
            invalid.Load();
            Require(invalid.CurrentId.StartsWith("custom:") && invalid.List().Count == 6,
                "invalid physical binding rejected with legacy fallback");

            File.WriteAllText(path, validJson);
            var duplicates = new ControlPresetStore(path, legacy);
            duplicates.Load();
            var second = duplicates.Duplicate(ControlPreset.MouseId, "Second custom");
            duplicates.Save();
            var duplicateJson = File.ReadAllText(path).Replace(second.Id, migrated.CurrentId);
            File.WriteAllText(path, duplicateJson);
            var duplicateRecovered = new ControlPresetStore(path, legacy);
            duplicateRecovered.Load();
            Require(duplicateRecovered.List().Count == 6 && duplicateRecovered.CurrentId.StartsWith("custom:"),
                "duplicate identity rejected with legacy fallback");

            File.WriteAllText(path, "{ broken json");
            var recovered = new ControlPresetStore(path, legacy);
            recovered.Load();
            Require(recovered.Current.MouseY.Inverted && recovered.CurrentId.StartsWith("custom:") &&
                Directory.GetFiles(directory, "control-presets.json.corrupt-*.json").Length >= 3,
                "corrupt config backup and legacy fallback");
            Require(File.ReadAllBytes(legacy).SequenceEqual(legacyBytes), "corruption did not touch display choices");
            File.WriteAllText(path, "{\"schemaVersion\":999,\"currentId\":\"builtin:keyboard\",\"profiles\":[]}");
            var unknown = new ControlPresetStore(path, legacy);
            unknown.Load();
            Require(unknown.CurrentId.StartsWith("custom:"), "unsupported schema safely falls back");
            GD.Print("CONTROL_PRESET_CHECK_OK schema=2 templates=5 migration roundtrip corrupt recovery missing-unbound-conflict parity display intact");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}

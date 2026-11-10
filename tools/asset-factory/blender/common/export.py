"""Guarded selected-object GLB export for Blender Python sessions."""

from pathlib import Path

from .validation import inspect_objects


def export_selected_glb(
    output_path: str, *, include_animations: bool = True, overwrite: bool = False
) -> dict:
    """Validate selection and export GLB; caller must save/review the .blend separately."""
    import bpy

    destination = Path(output_path).expanduser().resolve()
    if destination.suffix.lower() != ".glb":
        raise ValueError("GLB output must end in .glb")
    if not destination.parent.is_dir():
        raise ValueError(f"output directory does not exist: {destination.parent}")
    if destination.exists() and not overwrite:
        raise FileExistsError(f"GLB already exists: {destination}")
    report = inspect_objects(bpy.context.selected_objects)
    if report["problems"]:
        raise ValueError("selected objects failed FAF checks: " + "; ".join(report["problems"]))
    result = bpy.ops.export_scene.gltf(
        filepath=str(destination),
        export_format="GLB",
        use_selection=True,
        export_animations=include_animations,
    )
    if "FINISHED" not in result:
        raise RuntimeError(f"Blender GLB export did not finish: {result}")
    return {**report, "glb_output": str(destination)}

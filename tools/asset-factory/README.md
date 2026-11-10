# FAF Blender tooling

Source-controlled Python foundations for original Fish assets. Add this `blender/` directory to Blender's Python path, then import `common.naming`, `common.palette`, `common.validation` or `common.export`. Blender MCP may call these modules from a controlled session; durable construction logic belongs here, not in a one-time console command. The `fish/`, `coral/`, `rocks/`, `plants/` and `terrain/` packages are reserved for generators proven by actual reviewed assets.

The helpers provide canonical names, starter palette swatches, basic selected-object checks and a guarded selected-object GLB export. The export helper refuses an existing output unless `overwrite=True` is passed deliberately. They do **not** generate a fish, allocate IDs, prove visual quality or certify a Godot import. See the [FAF documentation](../../docs/fish-asset-factory/README.md) for the full pipeline and [export contract](../../docs/fish-asset-factory/blender-godot-contract.md).

Run focused checks from the repository root with `PYTHONPATH=tools/asset-factory/blender python3 -c 'from common import naming, palette, validation, export'`. Export needs Blender's `bpy`; importing the module does not.

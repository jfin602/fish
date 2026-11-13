#nullable enable
using Godot;

namespace Fish;

/// <summary>Non-shipping invertebrate proxy for the isolated encounter only.</summary>
public partial class FixtureFood : Node3D
{
    public string StableId { get; set; } = "";
    public bool IsAvailable { get; private set; } = true;

    public void Consume()
    {
        IsAvailable = false;
        Visible = false;
    }
}

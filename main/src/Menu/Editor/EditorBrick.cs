using Godot;
using System;

public class EditorBrick : Area2D
{
    [Signal]
    public delegate void BrickClick(EditorBrick brick,int mouseStatus);
    public string id;

    private void MouseDetector()
    {
        var tree = GetTree().Root.GetNode<Editor>("Area/Editor");
        if (tree.mouseHeld != 0)
        {
            EmitSignal("BrickClick",this,tree.mouseHeld);
        }
    }

    private void ClickDetector(Node viewport, InputEvent @event, int shape_idx)
    {
        if (@event is InputEventMouseButton btn && @event.IsPressed())
        {
            if (btn.ButtonIndex == (int)ButtonList.Left)
                EmitSignal("BrickClick",this,1);
            else if (btn.ButtonIndex == (int)ButtonList.Right)
                EmitSignal("BrickClick",this,2);
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

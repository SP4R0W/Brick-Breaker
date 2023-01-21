using Godot;
using System;

public class Background : CanvasLayer
{
    private Vector2 scrollValue = new Vector2(0,-1);
    [Export]
    private int speed = 100;

    private ParallaxBackground bg;

    public override void _Ready()
    {
        bg = GetNode<ParallaxBackground>("ParallaxBackground");
        GD.Print(bg);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Global.composer.CurrentScene.Name != "Editor" && Global.composer.CurrentScene.Name != "Game")
        {
            bg.Visible = true;
            bg.ScrollOffset += scrollValue * speed * delta;
        }
        else
        {
            bg.Visible = false;
        }
    }
}

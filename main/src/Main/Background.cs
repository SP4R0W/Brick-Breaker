using Godot;
using System;

public class Background : CanvasLayer
{
    Vector2 scrollValue = new Vector2(0,-1);
    [Export]
    int speed = 100;

    ParallaxBackground bg;

    public override void _Ready()
    {
        bg = GetNode<ParallaxBackground>("ParallaxBackground");
    }

    public override void _Process(float delta)
    {
        if (Global.composer.CurrentScene.Name != "Editor" && Global.composer.CurrentScene.Name != "Game")
        {
            bg.Visible = true;
            bg.ScrollOffset += scrollValue * speed * delta;
        }
        else
            bg.Visible = false;
    }
}

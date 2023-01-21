using Godot;
using System;

public class Bullet : Area2D
{

    private Vector2 velocity = new Vector2(0,-1);
    private float speed = 500;

    public override void _Process(float delta)
    {
        GlobalPosition += velocity * speed * delta;

        var height = GetNode<Sprite>("Sprite").Texture.GetHeight();

        if (GlobalPosition.y <= (0 - height))
        {
            QueueFree();
        }
    }

    private void BrickTouch(Node body)
    {
        var game = (Game)GetTree().Root.GetNode<Node>("Area/Game");

        game.BrickHit((Brick)body);
        QueueFree();
    }
}

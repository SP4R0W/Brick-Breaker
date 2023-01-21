using Godot;
using System;

public class Powerup : Area2D
{
    [Signal]
    public delegate void PaddleTouched(string powerup);

    [Signal]
    public delegate void PowerupDied();

    private float screenHeight = OS.GetRealWindowSize().y;

    private Vector2 velocity = new Vector2(0,1);
    private float speed = 100;

    public override void _Process(float delta)
    {
        GlobalPosition += velocity * speed * delta;

        if (GlobalPosition.y >= screenHeight)
        {
            EmitSignal("PowerupDied");
            QueueFree();
        }
    }

    private void PaddleTouch(Node body)
    {
        if (body.Name == "Paddle")
        {
            EmitSignal("PaddleTouched",GetNode<AnimatedSprite>("AnimatedSprite").Animation);
            QueueFree();
        }
    }

    private void BallDead()
    {
        QueueFree();
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

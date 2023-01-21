using Godot;
using System;

public class Brick : StaticBody2D
{
    private void KillBrick()
    {
        QueueFree();
    }
}

using Godot;
using System;

public class Brick : StaticBody2D
{
    void KillBrick()
    {
        QueueFree();
    }
}

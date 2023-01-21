using Godot;
using System;

public class Paddle : KinematicBody2D
{

    [Export]
    private float speed = 8.5f;

    private float speedMultiplier = 1f;

    private int bulletCount = 0;
    private int maxBulletCount = 10;

    private Vector2 velocity = Vector2.Zero;

    public float direction = 0;

    private Vector2 screenSize;

    private Timer sizeTimer;
    private Timer shootTimer;
    
    private Tween tween;

    private bool canMove = false;

    private PackedScene bullet = (PackedScene)ResourceLoader.Load("res://src/Game/Objects/Bullet.tscn");
    private AudioStreamPlayer shootSound;

    public override void _Ready()
    {
        shootSound = GetNode<AudioStreamPlayer>("Shoot");

        screenSize = GetViewportRect().Size;

        GetTree().Root.GetNode<Node>("Area/Game").Connect("GameStart",this,"GameStart");
        GetTree().Root.GetNode<Node>("Area/Game").Connect("RestartGame",this,"RestartGame");
        GetTree().Root.GetNode<Node>("Area/Game").Connect("GameOver",this,"GameOver");
        GetTree().Root.GetNode<Node>("Area/Game").Connect("GameWin",this,"GameOver");
        GetTree().Root.GetNode<Node>("Area/Game/Control/CanvasLayer/GamePanel/Ball").Connect("BallDead",this,"StopMoving");

        sizeTimer = GetNode<Timer>("SizeTimer");
        shootTimer = GetNode<Timer>("ShootTimer");
        tween = GetNode<Tween>("Tween");
    }

    private void GameStart()
    {
        shootTimer.Stop();
        sizeTimer.Stop();

        canMove = false;

        Position = new Vector2(607.5f,920);
        Modulate = new Color(1,1,1,0);
        Scale = new Vector2(1,1);
        speedMultiplier = 1f;

        tween.InterpolateProperty(this,"modulate:a",0,1,1);
        tween.InterpolateProperty(this,"position:x",Position.x,607.5f,1);
        tween.Start();

        GetTree().CreateTimer(1).Connect("timeout",this,"StartMoving");
    }

    public void StartMoving()
    {
        canMove = true;
    }

    public void StopMoving()
    {
        canMove = false;

        shootTimer.Stop();
        sizeTimer.Stop();
        tween.InterpolateProperty(this,"scale:x",Scale.x,1,0.5f);
        tween.Start();
    }

    private void RestartGame()
    {
        canMove = false;
        tween.InterpolateProperty(this,"position:x",Position.x,607.5f,1);
        tween.Start();

        GetTree().CreateTimer(1).Connect("timeout",this,"StartMoving");
    }

    private void GameOver()
    {
        canMove = false;
        tween.InterpolateProperty(this,"modulate:a",1,0,1);
        tween.Start();
    }

    public void FastPaddle()
    {
        speedMultiplier = 1.25f;

        sizeTimer.Start();
    }

    public void SlowPaddle()
    {
        speedMultiplier = 0.75f;

        sizeTimer.Start();
    }

    public void ShrinkPaddle()
    {
        tween.InterpolateProperty(this,"scale:x",1,0.75f,0.5f);
        tween.Start();

        sizeTimer.Start();
    }
    
    public void BiggerPaddle()
    {
        tween.InterpolateProperty(this,"scale:x",1,1.25,0.5f);
        tween.Start();

        sizeTimer.Start();
    }

    private void ResizePaddle()
    {
        if (Scale.x != 1)
        {
            tween.InterpolateProperty(this,"scale:x",Scale.x,1,0.5f);
            tween.Start();
        }

        speedMultiplier = 1.00f;
    }

    public void StartShooting()
    {
        if (shootTimer.IsStopped())
        {
            shootTimer.Start();
        }
    }

    private void Shoot()
    {
        bulletCount++;
        if (bulletCount == maxBulletCount)
        {
            shootTimer.Stop();
        }
        else
        {
            shootSound.Play();
            shootSound.VolumeDb = Global.sfxVolume;

            var panel = (Panel)GetParent();

            var pos1 = GetNode<Position2D>("ShootOrigin1");
            var pos2 = GetNode<Position2D>("ShootOrigin2");

            var bullet1 = (Bullet)bullet.Instance();
            var bullet2 = (Bullet)bullet.Instance();

            panel.AddChild(bullet1);
            panel.AddChild(bullet2);

            bullet1.GlobalPosition = pos1.GlobalPosition;
            bullet2.GlobalPosition = pos2.GlobalPosition;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        var width = GetNode<Sprite>("Sprite").Texture.GetWidth();
        var panel = (Panel)GetParent();
        var panelWidth = panel.RectSize.x;

        direction = Convert.ToInt32(Input.IsActionPressed("move_right")) - Convert.ToInt32(Input.IsActionPressed("move_left"));

        if (canMove)
        {
            velocity = new Vector2(direction,0) * speed * speedMultiplier;
            MoveAndCollide(velocity);

            Position = new Vector2(
                x:Mathf.Clamp(Position.x,1 + (width / 2),panelWidth - (width / 2)),
                y:920);
        }
    }
}

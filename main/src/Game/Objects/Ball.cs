using Godot;
using System;

public class Ball : KinematicBody2D
{
    [Export]
    public float defaultSpeed = 5f;

    [Export]
    public bool disableMovement = false;

    [Signal]
    public delegate void BallDead();

    [Signal]
    public delegate void BrickHit(Brick brick);

    bool isBallStarted = false;
    bool isGrace = false;
    bool isBallImmune = false;
    bool isBallDestructive = false;

    Vector2 velocity = new Vector2(0,0);

    Tween tween;
    Timer powerupTimer;

    int screenWidth, screenHeight;

    float scale = 1.0f;

    MusicPlayer player;

    public override void _Ready()
    {
        player = (MusicPlayer)GetTree().Root.GetNode<Node2D>("Area/Game/Player");

        screenWidth = Convert.ToInt32(ProjectSettings.GetSetting("display/window/size/width"));
        screenHeight = Convert.ToInt32(ProjectSettings.GetSetting("display/window/size/height"));

        GetTree().Root.GetNode<Node>("Area/Game").Connect("GameStart",this,"GameStart");
        GetTree().Root.GetNode<Node>("Area/Game").Connect("RestartGame",this,"GameStart");
        GetTree().Root.GetNode<Node>("Area/Game").Connect("GameOver",this,"GameOver");
        GetTree().Root.GetNode<Node>("Area/Game").Connect("GameWin",this,"GameOver");

        tween = GetNode<Tween>("Tween");
        powerupTimer = GetNode<Timer>("PowerupTimer");
    }

    void GameStart()
    {
        ((Game)GetTree().Root.GetNode<Node>("Area/Game")).player.StopPlaying();

        var timer = GetTree().Root.GetNode<Node>("Area/Game").GetNode<Timer>("TimeTimer");
        timer.Stop();

        var collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        collisionShape.SetDeferred("disabled",false);

        velocity.x = 0;
        velocity.y = 0;

        Position = new Vector2(607.5f,835);
        Modulate = new Color(1,1,1,0);

        tween.InterpolateProperty(this,"modulate:a",0,1,1);
        tween.Start();

        GetTree().CreateTimer(1).Connect("timeout",this,"GameStartEnd");
    }

    void GameOver()
    {
        var collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        collisionShape.SetDeferred("disabled",false);

        velocity.x = 0;
        velocity.y = 0;

        tween.InterpolateProperty(this,"modulate:a",1,0,1);
        tween.Start();
    }

    void GameStartEnd()
    {
        isBallStarted = true;
        var text = GetTree().Root.GetNode<Label>("Area/Game/Control/CanvasLayer/GamePanel/HelpText");
        text.Visible = true;
    }

    public void WeakBall()
    {
        isBallDestructive = false;
        isBallImmune = true;

        powerupTimer.Start();
    }

    public void PowerBall()
    {
        isBallDestructive = true;
        isBallImmune = false;

        powerupTimer.Start();
    }

    void EndPowerup()
    {
        isBallDestructive = false;
        isBallImmune = false;
    }

    void EndGrace()
    {
        isGrace = false;
    }

    void EndCollisionTimer(CollisionShape2D shape)
    {
        shape.SetDeferred("disabled",true);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (isBallStarted)
        {
            var text = GetTree().Root.GetNode<Label>("Area/Game/Control/CanvasLayer/GamePanel/HelpText");

            if (Input.IsActionPressed("move_left"))
            {
                text.Visible = false;
                player.canInteract = true;

                isBallStarted = false;
                velocity.x = -defaultSpeed;
                velocity.y = -defaultSpeed;

                ((Game)GetTree().Root.GetNode<Node>("Area/Game")).player.PlayRandom(true);
                ((Game)GetTree().Root.GetNode<Node>("Area/Game")).cooldown.Start();

                var timer = GetTree().Root.GetNode<Node>("Area/Game").GetNode<Timer>("TimeTimer");
                timer.Start();
            }
            else if (Input.IsActionPressed("move_right"))
            {
                player.canInteract = true;
                text.Visible = false;

                isBallStarted = false;
                velocity.x = defaultSpeed;
                velocity.y = -defaultSpeed;

                ((Game)GetTree().Root.GetNode<Node>("Area/Game")).player.PlayRandom(true);
                ((Game)GetTree().Root.GetNode<Node>("Area/Game")).cooldown.Start();

                var timer = GetTree().Root.GetNode<Node>("Area/Game").GetNode<Timer>("TimeTimer");
                timer.Start();
            }
        }

        var width = GetNode<Sprite>("Sprite").Texture.GetWidth();
        var height = GetNode<Sprite>("Sprite").Texture.GetHeight();

        var panel = (Panel)GetParent();
        var panelWidth = panel.RectSize.x;

        // Check if the ball tried to go off bounds
        if (Position.x <= ((0 + (width / 2))))
        {
            Position = new Vector2(0 + (width / 2),Position.y);
            velocity.x *= -1;
        }
        else if (Position.x >= (panelWidth - (width / 2)))
        {
            Position = new Vector2(panelWidth - (width / 2),Position.y);
            velocity.x *= -1;
        }

        if (Position.y <= (0 + (height / 2)))
        {
            Position = new Vector2(Position.x,0 + (height / 2));
            velocity.y *= -1;
        }
        else if (Position.y >= 1080) // Ball missed the paddle; kill
        {
            ((Game)GetTree().Root.GetNode<Node>("Area/Game")).player.StopPlaying();

            player.canInteract = false;

            EmitSignal("BallDead");

            tween.InterpolateProperty(this,"modulate:a",0,1,1);
            tween.Start();
        }

        if (velocity.y > 0 && velocity.y < 1)
            velocity.y = 1;
        else if (velocity.y < 0 && velocity.y > -1)
            velocity.y = -1;

        if (!disableMovement)
        {
            var oldPosition = Position;
            var collision = MoveAndCollide(velocity);
            var newPosition = Position;

            var movement = newPosition - oldPosition;

            if (movement == Vector2.Zero && !isBallStarted && velocity.x != 0 && velocity.y != 0)
            {
                Position = new Vector2(607.5f,835);
                velocity.y = -defaultSpeed;
            }

            // Collision check code
            if (collision != null)
            {
                var collider = (Node)collision.Collider;

                if (collider.IsInGroup("paddle"))
                {
                    var sound = GetNode<AudioStreamPlayer>("BouncePaddle");
                    sound.Play();
                    sound.VolumeDb = Global.sfxVolume;

                    var paddle = (Paddle)collider;

                    var distance = paddle.GlobalPosition.x - GlobalPosition.x;
                    var spriteWidth = paddle.GetNode<Sprite>("Sprite").Texture.GetWidth() / 2;
                    var relation = distance/spriteWidth;
                    var angle = relation * 45f;
                    var angleRad = -Mathf.Deg2Rad(angle);

                    velocity = velocity.Bounce(collision.Normal).Rotated(angleRad);
                }

                if (collider.IsInGroup("bricks"))
                {

                    var brick = (Brick)collider;

                    if (!isGrace && !isBallImmune)
                    {
                        isGrace = true;
                        EmitSignal("BrickHit",brick);

                        GetTree().CreateTimer(0.01f).Connect("timeout",this,"EndGrace");
                    }

                    if (!isBallDestructive)
                        velocity = velocity.Bounce(collision.Normal);
                }
            }
        }
    }
}

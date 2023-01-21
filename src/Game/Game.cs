using Godot;
using System;
using Godot.Collections;

public class Game : Node
{
    [Signal]
    public delegate void GameStart();

    [Signal]
    public delegate void RestartGame();

    [Signal]
    public delegate void GameOver();

    [Signal]
    public delegate void GameWin();

    private Tween tween;
    private Composer Composer;
    public MusicPlayer player;

    public Timer cooldown;

    private PackedScene brickScene = (PackedScene)ResourceLoader.Load("res://src/Game/Objects/Brick.tscn");
    private PackedScene powerupScene = (PackedScene)ResourceLoader.Load("res://src/Game/Objects/Powerup.tscn");

    private Dictionary<Brick,Godot.Collections.Array> brickDict = new Dictionary<Brick,Godot.Collections.Array>();

    private Dictionary<string,int> brickScore = new Dictionary<string, int>()
    {
        {"1",25},{"2",35},{"3",15},{"4",50},{"5",75},{"6",100}
    };

    private int gameTime = 0;
    private int gameScore = 0;
    private int gameLives = 3;
    private int gameDeaths = 0;

    private float powerupChance = 0;

    private bool isRestartGame = false;
    private bool isPausedGame = false;

    private int brickCount = 0;
    private int gameDestroyed = 0;

    public override void _Ready()
    {
        Global.totalScore = 0;
        Global.totalTime = 0;
        Global.totalBricks = 0;
        Global.totalDeaths = 0;
        Global.levelName = "";
        Global.levelAuth = "";

        Composer = Global.composer;
        var control = GetNode<Control>("Control");
        control.RectSize = new Vector2(Global.screenSize.x,Global.screenSize.y);

        tween = GetNode<Tween>("Tween");
        player = GetNode<MusicPlayer>("Player");
        cooldown = GetNode<Timer>("PowerupCooldown");

        player.Connect("MusicChanged",this,"UpdateTrackName");
        player.Connect("LoopChanged",this,"UpdateLoopText");

        GetTree().Root.GetNode<Node>("Area/Game/Control/CanvasLayer/GamePanel/Ball").Connect("BallDead",this,"BallDead");
        GetTree().Root.GetNode<Node>("Area/Game/Control/CanvasLayer/GamePanel/Ball").Connect("BrickHit",this,"BrickHit");

        var musicBar = GetNode<HSlider>("Control/CanvasLayer/HelpPanel/MusicSlider");
        musicBar.Value = Global.musicVolume;

        var sfxBar = GetNode<HSlider>("Control/CanvasLayer/HelpPanel/MusicSlider2");
        sfxBar.Value = Global.sfxVolume;

        EmitSignal("GameStart");

        DrawBricks();
    }

    private void GotoMenu()
    {
        Global.webLevel = null;
        Composer.GotoScene(Composer.scenes["levelselect"],true,"fade");
    }

    private void GotoGameOver()
    {
        Composer.GotoScene(Composer.scenes["levelend"],true,"fade",true);
    }

    private void CheckPowerup(string powerup)
    {
        var paddle = GetTree().Root.GetNode<Paddle>("Area/Game/Control/CanvasLayer/GamePanel/Paddle");
        var ball = GetTree().Root.GetNode<Ball>("Area/Game/Control/CanvasLayer/GamePanel/Ball");

        var badSound = GetNode<AudioStreamPlayer>("Sounds/BadPowerup");
        var goodSound = GetNode<AudioStreamPlayer>("Sounds/GoodPowerup");

        switch (powerup)
        {
            case "0":
                goodSound.Play();
                goodSound.VolumeDb = Global.sfxVolume;
                paddle.BiggerPaddle();
                break;
            case "1":
                badSound.Play();
                badSound.VolumeDb = Global.sfxVolume;
                paddle.ShrinkPaddle();
                break;
            case "2":
                badSound.Play(); 
                badSound.VolumeDb = Global.sfxVolume;
                ball.WeakBall();
                break;
            case "3":
                goodSound.Play();
                goodSound.VolumeDb = Global.sfxVolume;
                paddle.StartShooting();
                break;
            case "4":
                goodSound.Play();
                goodSound.VolumeDb = Global.sfxVolume;
                paddle.FastPaddle();
                break;
            case "5":
                badSound.Play(); 
                badSound.VolumeDb = Global.sfxVolume;
                paddle.SlowPaddle();
                break;
            case "6":
                goodSound.Play();
                goodSound.VolumeDb = Global.sfxVolume;
                ball.PowerBall();
                break;
            case "7":
                goodSound.Play();
                goodSound.VolumeDb = Global.sfxVolume;
                AddLife();
                break;
        }
    }

    private void KillPowerup()
    {
        cooldown.Start();
    }

    private void SpawnPowerup(Vector2 brickPos)
    {
        var rand = GD.Randi() % 100;

        if (powerupChance <= rand)
        {
            if (powerupChance == 0)
            {
                powerupChance = 5;
            }
            else
            {
                powerupChance *= 1.5f;
            }
        }
        else if (powerupChance > rand)
        {
            var panel = GetNode<Node2D>("Control/CanvasLayer/GamePanel/Bricks");

            var powerup = (Powerup)powerupScene.Instance();
            var powerupNum = Convert.ToString((GD.Randi() % 8) - 1);
            var powerupSprite = powerup.GetNode<AnimatedSprite>("AnimatedSprite");

            GetTree().Root.GetNode<Node>("Area/Game/Control/CanvasLayer/GamePanel/Ball").Connect("BallDead",powerup,"BallDead");

            panel.AddChild(powerup); 

            powerup.Connect("PaddleTouched",this,"CheckPowerup");
            powerup.Connect("PowerupDied",this,"KillPowerup");

            powerup.Position = brickPos;
            if (powerupNum != "4294967295")
            {
                powerupSprite.Animation = powerupNum;
            }
            else
            {
                powerupSprite.Animation = "1";
            }

            cooldown.Start();
            powerupChance = 0;
        }
    }

    public void BrickHit(Brick brick)
    {
        var health = Convert.ToInt32(brickDict[brick][0]);
        var type = Convert.ToString(brickDict[brick][1]);
        var color = Convert.ToString(brick.GetNode<AnimatedSprite>("AnimatedSprite").Animation[0]);
        health--;

        brickDict[brick][0] = health;

        if (type == "normal")
        {
            var sound = GetNode<AudioStreamPlayer>("Sounds/BounceNormal");
            sound.Play();
            sound.VolumeDb = Global.sfxVolume;
        }
        else
        {
            var sound = GetNode<AudioStreamPlayer>("Sounds/BounceArmor");
            sound.Play();
            sound.VolumeDb = Global.sfxVolume;
        }

        if (health == 0)
        {
            if (cooldown.IsStopped())
            {
                SpawnPowerup(brick.Position);
            }

            var brickColShape = brick.GetNode<CollisionShape2D>("CollisionShape2D");
            brickColShape.SetDeferred("disabled",true);
            brickDict.Remove(brick);
            gameDestroyed++;
            brickCount--;

            if (type == "normal")
                gameScore += brickScore[color];
            else
                gameScore += brickScore[color] * 2;

            var scoreText = GetNode<Label>("Control/CanvasLayer/UIPanel/ScoreLabel");
            scoreText.Text = "Score: " + Convert.ToString(gameScore);

            var tween = brick.GetNode<Tween>("Tween");

            tween.InterpolateProperty(brick,"modulate:a",1,0,.5f);
            tween.Start();

            GetTree().CreateTimer(0.5f).Connect("timeout",brick,"KillBrick");

            if (brickCount == 0)
            {
                player.canInteract = false;
                player.StopPlaying();

                var sound = GetNode<AudioStreamPlayer>("Sounds/Win");
                sound.Play();
                sound.VolumeDb = Global.sfxVolume;

                EmitSignal("GameWin");

                Global.totalScore = gameScore;
                Global.totalTime = gameTime;
                Global.totalBricks = gameDestroyed;
                Global.totalDeaths = gameDeaths;

                GetTree().CreateTimer(2).Connect("timeout",this,"GotoGameOver");
            }
        }
        else if (health == 1)
        {
            var brickSprite = brick.GetNode<AnimatedSprite>("AnimatedSprite");
            brickSprite.Animation = brickSprite.Animation[0] + "_normal";
        }
    }

    private void IncreaseTime()
    {
        gameTime++;

        var timeText = GetNode<Label>("Control/CanvasLayer/UIPanel/TimeLabel");
        timeText.Text = "Time: " + Convert.ToString(gameTime);
    }

    private void AddLife()
    {
        if (!isRestartGame)
        {
            gameLives++;
            var livesText = GetNode<Label>("Control/CanvasLayer/UIPanel/LivesLabel");
            livesText.Text = "Lives: " + Convert.ToString(gameLives);
        }
    }

    private void UpdateTrackName(int newTrack)
    {
        var musicText = GetNode<Label>("Control/CanvasLayer/HelpPanel/TrackLabel");
        musicText.Text = "Current track: " + player.trackNames[newTrack];
    }

    private void UpdateLoopText(bool newLoop)
    {
        var loopText = GetNode<Label>("Control/CanvasLayer/HelpPanel/Setting3");
        loopText.Visible = newLoop;
    }

    private void BallDead()
    {
        if (!isRestartGame)
        {
            var sound = GetNode<AudioStreamPlayer>("Sounds/Fail");
            sound.Play();
            sound.VolumeDb = Global.sfxVolume;

            var timer = GetNode<Timer>("TimeTimer");
            timer.Stop();

            isRestartGame = true;

            gameDeaths++;
            gameLives--;
            var livesText = GetNode<Label>("Control/CanvasLayer/UIPanel/LivesLabel");
            livesText.Text = "Lives: " + Convert.ToString(gameLives);

            if (gameLives > 0)
            {
                powerupChance = 0;

                GetTree().CreateTimer(2).Connect("timeout",this,"EmitRestartGame");
            }
            else
            {
                player.StopPlaying();

                EmitSignal("GameOver");

                Global.totalScore = gameScore;
                Global.totalTime = gameTime;
                Global.totalBricks = gameDestroyed;
                Global.totalDeaths = gameDeaths;

                GetTree().CreateTimer(2).Connect("timeout",this,"GotoGameOver");
            }
        }
    }

    private void EmitRestartGame()
    {
        isRestartGame = false;
        EmitSignal("RestartGame");
    }

    private Godot.Collections.Dictionary<string, Godot.Collections.Array> GetFile()
    {
        if (Global.levelPath.Contains(".level"))
        {
            var mainLevelDict = new Dictionary<string, Godot.Collections.Array>(){};

            var file = new File();

            file.Open(Global.levelPath,File.ModeFlags.Read);

            var obj = (Dictionary)file.GetVar();
            file.Close();

            var brickColumns = 15;
            var brickRows = 14;

            string str = "";

            for (int i = 0;i <= brickColumns;i++)
            {
                for (int j = 0;j <= brickRows;j++)
                {
                    var name = Convert.ToString(i) + "_" + Convert.ToString(j);

                    Godot.Collections.Array arr = (Godot.Collections.Array)obj[name];

                    var col = Convert.ToString(arr[0]);

                    var type = Convert.ToString(arr[1]);

                    //{"0_0",new Godot.Collections.Array(){1,"immune"}}
                    str += ("{\"" + name + "\",new Godot.Collections.Array(){" + col + ",\"" + type + "\"}}");
                    if (name != "15_14")
                    {
                        str += ",";
                    }
                    
                    if (name.Contains("_0") && name != "0_0")
                    {
                        str += "\n";
                    }

                    mainLevelDict.Add(name,(Godot.Collections.Array)obj[name]);
                }
            }

            var levelId = (Godot.Collections.Array)obj["id"];
            var levelName = (string)levelId[0];
            var levelAuth = (string)levelId[1];

            Global.levelName = levelName;
            Global.levelAuth = levelAuth;

            var levelText = GetNode<Label>("Control/CanvasLayer/UIPanel/LevelText");

            levelText.Text = levelName + " by: " + levelAuth;

            return mainLevelDict;
        }
        else
        {
            var levelId = (Godot.Collections.Array)Levels.gameData[Global.levelPath]["id"];
            var levelName = (string)levelId[0];
            var levelAuth = (string)levelId[1];

            var levelText = GetNode<Label>("Control/CanvasLayer/UIPanel/LevelText");
            levelText.Text = levelName;

            Global.levelName = levelName;
            Global.levelAuth = levelAuth;
            return Levels.gameData[Global.levelPath];
        }
    }

    private void DrawBricks()
    {
        var levelData = new Godot.Collections.Dictionary<string, Godot.Collections.Array>();

        if (Global.levelPath != "" && Global.webLevel == null)
        {
            levelData = GetFile();
        }
        else
        {
            levelData = Global.webLevel;
            Global.levelName = (string)levelData["id"][0];
            Global.levelAuth = (string)levelData["id"][1];

            var levelText = GetNode<Label>("Control/CanvasLayer/UIPanel/LevelText");
            levelText.Text = Global.levelName + " by: " + Global.levelAuth;
        }

        var startX = 715;
        var startY = 350;

        var posX = startX;
        var posY = startY;

        var brickColumns = 15;
        var brickRows = 14;

        var brickNode = GetNode<Node2D>("Control/CanvasLayer/GamePanel/Bricks");

        for (int i = 0;i <= brickColumns;i++)
        {
            posY = startY;
            for (int j = 0;j <= brickRows;j++)
            {
                var brickId = Convert.ToString(i) + "_" + Convert.ToString(j);
                var brickData = levelData[brickId];

                int brickCol = Convert.ToInt32(brickData[0]);
                string brickType = Convert.ToString(brickData[1]);


                if (brickCol == 0)
                {
                    posY += 35;
                    continue;
                }

                var brick = (Brick)brickScene.Instance();

                var brickSprite = brick.GetNode<AnimatedSprite>("AnimatedSprite");
                var brickSpriteName = Convert.ToString(brickCol) + "_" + brickType;

                brickNode.AddChild(brick);

                if (brickType == "normal")
                {
                    brickSprite.Animation = brickSpriteName;
                    brickDict[brick] = new Godot.Collections.Array(){1,"normal"};
                }
                else if (brickType == "armor")
                {
                    brickSprite.Animation = brickSpriteName;
                    brickDict[brick] = new Godot.Collections.Array(){2,"armor"};
                }
                else if (brickType == "immune")
                {
                    brickSprite.Animation = "3_armor";
                    brick.Modulate = new Color(0.1f,0.1f,0.1f,1);
                    brickDict[brick] = new Godot.Collections.Array(){-1,"immune"};
                }
                
                brick.GlobalPosition = new Vector2(posX,posY);
                if (brickType != "immune")
                {
                    brick.Modulate = new Color(1,1,1,0);
                    brickCount++;
                }
                brick.Scale = new Vector2(2,2);

                tween.InterpolateProperty(brick,"modulate:a",0,1,0.5f,Tween.TransitionType.Linear,Tween.EaseType.InOut,0.1f);
                tween.InterpolateProperty(brick,"scale",new Vector2(2,2),new Vector2(1.15f,1),0.5f,Tween.TransitionType.Linear,Tween.EaseType.InOut,0.1f);

                tween.Start();

                posY += 35;
            }

            posX += 75;
        }
    }

    private void PauseGame()
    {
        var text = GetNode<Label>("Control/CanvasLayer/GamePanel/PauseText");
        text.Visible = true;

        player.StopPlaying();

        var paddle = GetNode<Paddle>("Control/CanvasLayer/GamePanel/Paddle");
        paddle.StopMoving();

        var ball = GetNode<Ball>("Control/CanvasLayer/GamePanel/Ball");
        ball.disableMovement = true;

        var timer = GetNode<Timer>("TimeTimer");
        timer.Paused = true;
    }

    private void UnpauseGame()
    {
        var text = GetNode<Label>("Control/CanvasLayer/GamePanel/PauseText");
        text.Visible = false;

        player.StartPlaying();

        var paddle = GetNode<Paddle>("Control/CanvasLayer/GamePanel/Paddle");
        paddle.StartMoving();

        var ball = GetNode<Ball>("Control/CanvasLayer/GamePanel/Ball");
        ball.disableMovement = false;

        var timer = GetNode<Timer>("TimeTimer");
        timer.Paused = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("pause") && player.canInteract)
        {
            isPausedGame = !isPausedGame;
            if (isPausedGame)
            {
                PauseGame();
            }
            else
            {
                UnpauseGame();
            }
        }
    }
}

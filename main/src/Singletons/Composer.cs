using Godot;
using System;
using Godot.Collections;

public class Composer : Node
{
    public Node CurrentScene;
    bool isEnteringNewScene = false;

    AudioStreamPlayer buttonclick;

    public Dictionary<string,string> scenes = new Dictionary<string, string>()
    {
        {"mainmenu","res://src/Menu/MainMenu.tscn"},
        {"levelselect","res://src/Menu/LevelSelect.tscn"},
        {"editor","res://src/Menu/Editor.tscn"},
        {"help","res://src/Menu/Help.tscn"},
        {"options","res://src/Menu/Options.tscn"},
        {"game","res://src/Game/Game.tscn"},
        {"levelend","res://src/Menu/LevelEnd.tscn"}
    };

    Dictionary<string,int> screen = new Dictionary<string, int>()
    {

    };

    public override void _Ready()
    {
        screen["width"] = Convert.ToInt32(OS.GetRealWindowSize().x);
        screen["height"] = Convert.ToInt32(OS.GetRealWindowSize().y);

        buttonclick = GetTree().Root.GetNode<AudioStreamPlayer>("Area/ButtonClick");
    }

    public void GotoScene(string newScene,bool animate=true,string animation="fade",bool noSound=false)
    {
        if (!isEnteringNewScene)
        {
            if (!noSound)
            {
                buttonclick.Play();
                buttonclick.VolumeDb = Global.sfxVolume;
            }

            isEnteringNewScene = true;
            CallDeferred(nameof(DefferedGotoScene),newScene,animate,animation);
        }
    }

    async void DefferedGotoScene(string newScene,bool animate=true,string animation="fade")
    {
        var root = GetTree().Root.GetNode<Node>("Area");

        if (animate)
        {
            // Preparations for animation
            var rootControl = root.GetNode<CanvasLayer>("Control/CanvasLayer");
            var colorRect = root.GetNode<ColorRect>("Control/CanvasLayer/ColorRect");
            var tween = root.GetNode<Tween>("Tween");

            colorRect.Visible = true;

            colorRect.Color = new Color(0,0,0,0);

            colorRect.SetSize(new Vector2(screen["width"]*4,screen["height"]*4));
            colorRect.RectGlobalPosition = new Vector2(0,0);
            colorRect.RectScale = new Vector2(1,1);

            // Fade in
            tween.InterpolateProperty(colorRect,"color",new Color(0,0,0,0),new Color(0,0,0,1),.5f,Tween.TransitionType.Sine,Tween.EaseType.InOut);
            tween.Start();

            await ToSignal(tween,"tween_all_completed");

            var scene = root.GetNode<Node>(CurrentScene.Name);
            scene.QueueFree();

            var nextScene = (PackedScene)ResourceLoader.Load(newScene);
            Node nextLevel = (Node)nextScene.Instance();
            root.AddChild(nextLevel);
            root.MoveChild(nextLevel,1);

            root.MoveChild(nextLevel,0);
            CurrentScene = root.GetChild(0);

            // Fade out
            tween.InterpolateProperty(colorRect,"color",colorRect.Color,new Color(0,0,0,0),.5f,Tween.TransitionType.Sine,Tween.EaseType.InOut);
            tween.Start();

            await ToSignal(tween,"tween_all_completed");

            colorRect.Visible = false;

            isEnteringNewScene = false;
            return;
        }

        if (CurrentScene != null)
        {
            var scene = root.GetNode<Node>(CurrentScene.Name);
            scene.QueueFree();

            var nextScene = (PackedScene)ResourceLoader.Load(newScene);
            Node nextLevel = (Node)nextScene.Instance();
            root.AddChild(nextLevel);
            root.MoveChild(nextLevel,1);

            root.MoveChild(nextLevel,0);
            CurrentScene = root.GetChild(0);

            isEnteringNewScene = false;
        }
        else
        {
            var nextLevel = (PackedScene)ResourceLoader.Load(newScene);
            root.AddChild(nextLevel.Instance());

            CurrentScene = root.GetChild(root.GetChildCount() - 1);

            isEnteringNewScene = false;
        }
    }
}

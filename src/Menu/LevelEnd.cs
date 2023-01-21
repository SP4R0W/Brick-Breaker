using Godot;
using Godot.Collections;
using System;

public class LevelEnd : Node
{

    private bool isAnimationFinished = false;
    private Composer Composer;
    private Tween tween;

    public async override void _Ready()
    {
        Composer = Global.composer;
        var control = GetNode<Control>("Control");
        control.RectSize = new Vector2(Global.screenSize.x,Global.screenSize.y);

        tween = GetNode<Tween>("Tween");

        var title = control.GetNode<Label>("CanvasLayer/GameTitle");
        title.RectGlobalPosition = new Vector2(title.RectGlobalPosition.x,-500);
        tween.InterpolateProperty(title,"rect_global_position:y",title.RectGlobalPosition.y,0,1,Tween.TransitionType.Quart,Tween.EaseType.InOut);

        var devTitle = control.GetNode<Label>("CanvasLayer/Subtitle");
        devTitle.RectGlobalPosition = new Vector2(devTitle.RectGlobalPosition.x,-375);
        tween.InterpolateProperty(devTitle,"rect_global_position:y",devTitle.RectGlobalPosition.y,130,1,Tween.TransitionType.Expo,Tween.EaseType.InOut,.5f);

        var helpText = control.GetNode<RichTextLabel>("CanvasLayer/HelpText");
        helpText.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(helpText,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,.5f);

        helpText.Clear();
        if (Global.levelPath.Contains(".level"))
        {
            helpText.AppendBbcode("[center]You played " + Global.levelName + " by " + Global.levelAuth + "\n\ntotal time: " + Global.totalTime + "\ntotal score: " + Global.totalScore + "\ntotal bricks broken: " + Global.totalBricks + "\n\nyour deaths: " + Global.totalDeaths + "[/center]");
        }
        else if (Global.webLevel != null)
        {
            Global.webLevel = null;
            helpText.AppendBbcode("[center]You played " + Global.levelName + " by " + Global.levelAuth + "\n\ntotal time: " + Global.totalTime + "\ntotal score: " + Global.totalScore + "\ntotal bricks broken: " + Global.totalBricks + "\n\nyour deaths: " + Global.totalDeaths + "[/center]");
        }
        else
        {
            helpText.AppendBbcode("[center]You played " + Global.levelName + "\n\ntotal time: " + Global.totalTime + "\ntotal score: " + Global.totalScore + "\ntotal bricks broken: " + Global.totalBricks + "\n\nyour deaths: " + Global.totalDeaths + "[/center]");            
        }


        var button1 = control.GetNode<TextureButton>("CanvasLayer/QuitButton");
        button1.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button1,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1.75f);
        

        tween.Start();

        await ToSignal(tween,"tween_all_completed");

        var menuTheme = GetTree().Root.GetNode<AudioStreamPlayer>("Area/MenuTheme");
        if (!menuTheme.Playing)
            menuTheme.Play();

        isAnimationFinished = true;
    }

    public override async void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && !isAnimationFinished)
        {
            if (mouseButton.ButtonIndex == 1)
            {
                var control = GetNode<Control>("Control");

                tween.RemoveAll();

                var title = control.GetNode<Label>("CanvasLayer/GameTitle");
                title.RectGlobalPosition = new Vector2(title.RectGlobalPosition.x,0);

                var devTitle = control.GetNode<Label>("CanvasLayer/Subtitle");
                devTitle.RectGlobalPosition = new Vector2(devTitle.RectGlobalPosition.x,130);

                var helpText = control.GetNode<RichTextLabel>("CanvasLayer/HelpText");
                helpText.Modulate = new Color(1,1,1,1);

                var button1 = control.GetNode<TextureButton>("CanvasLayer/QuitButton");
                button1.Modulate = new Color(1,1,1,1);

                await ToSignal(GetTree().CreateTimer(0.2f),"timeout");

                var menuTheme = GetTree().Root.GetNode<AudioStreamPlayer>("Area/MenuTheme");
                if (!menuTheme.Playing)
                    menuTheme.Play();
                
                isAnimationFinished = true;
            }
        }
    }

    private void GotoMenu()
    {
        if (!isAnimationFinished)
        {
            return;
        }

        Composer.GotoScene(Composer.scenes["mainmenu"],true,"fade");
    }
}

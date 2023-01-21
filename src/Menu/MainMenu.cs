using Godot;
using System;

public class MainMenu : Node
{

    private bool isAnimationFinished = false;

    private Composer Composer;
    private Tween tween;

    private bool isBrowserGame = false;

    public async override void _Ready()
    {
        if (OS.GetName() == "HTML5")
        {
            isBrowserGame = true;
        }

        var dir = new Directory();

        if (dir.Open("user://Levels") != Error.Ok)
        {
            dir.Open("user://");
            dir.MakeDir("Levels");
        }

        Composer = Global.composer;
        var control = GetNode<Control>("Control");
        control.RectSize = new Vector2(Global.screenSize.x,Global.screenSize.y);

        tween = GetNode<Tween>("Tween");

        var title = control.GetNode<Label>("CanvasLayer/GameTitle");
        title.RectGlobalPosition = new Vector2(title.RectGlobalPosition.x,-500);
        tween.InterpolateProperty(title,"rect_global_position:y",title.RectGlobalPosition.y,110,1,Tween.TransitionType.Quart,Tween.EaseType.InOut);

        var devTitle = control.GetNode<Label>("CanvasLayer/CreatorSubtitle");
        devTitle.RectGlobalPosition = new Vector2(devTitle.RectGlobalPosition.x,-375);
        tween.InterpolateProperty(devTitle,"rect_global_position:y",devTitle.RectGlobalPosition.y,250,1,Tween.TransitionType.Expo,Tween.EaseType.InOut,.5f);

        var button1 = control.GetNode<TextureButton>("CanvasLayer/PlayButton");
        button1.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button1,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1.5f);

        var button2 = control.GetNode<TextureButton>("CanvasLayer/EditorButton");
        button2.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button2,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1.75f);

        var button3 = control.GetNode<TextureButton>("CanvasLayer/HelpButton");
        button3.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button3,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2);
        
        var button4 = control.GetNode<TextureButton>("CanvasLayer/OptionsButton");
        button4.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button4,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2.25f);

        var button5 = control.GetNode<TextureButton>("CanvasLayer/QuitButton");
        button5.Modulate = new Color(1,1,1,0);

        if (!isBrowserGame)
        {
            tween.InterpolateProperty(button5,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2.5f);
        }

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
            GD.Print("test");
            if (mouseButton.ButtonIndex == 1)
            {
                var control = GetNode<Control>("Control");

                tween.RemoveAll();

                var title = control.GetNode<Label>("CanvasLayer/GameTitle");
                title.RectGlobalPosition = new Vector2(title.RectGlobalPosition.x,110);

                var devTitle = control.GetNode<Label>("CanvasLayer/CreatorSubtitle");
                devTitle.RectGlobalPosition = new Vector2(devTitle.RectGlobalPosition.x,250);

                var button1 = control.GetNode<TextureButton>("CanvasLayer/PlayButton");
                button1.Modulate = new Color(1,1,1,1);

                var button2 = control.GetNode<TextureButton>("CanvasLayer/EditorButton");
                button2.Modulate = new Color(1,1,1,1);

                var button3 = control.GetNode<TextureButton>("CanvasLayer/HelpButton");
                button3.Modulate = new Color(1,1,1,1);

                var button4 = control.GetNode<TextureButton>("CanvasLayer/OptionsButton");
                button4.Modulate = new Color(1,1,1,1);

                if (!isBrowserGame)
                {
                    var button5 = control.GetNode<TextureButton>("CanvasLayer/QuitButton");
                    button5.Modulate = new Color(1,1,1,1);                    
                }

                await ToSignal(GetTree().CreateTimer(0.2f),"timeout");
                
                var menuTheme = GetTree().Root.GetNode<AudioStreamPlayer>("Area/MenuTheme");
                if (!menuTheme.Playing)
                    menuTheme.Play();

                isAnimationFinished = true;
            }
        }
    }

    private void GotoGame()
    {
        if (!isAnimationFinished)
        {
            return;
        }

        Composer.GotoScene(Composer.scenes["levelselect"],true,"fade");
    }

    private void GotoEditor()
    {
        if (!isAnimationFinished)
        {
            return;
        }

        Composer.GotoScene(Composer.scenes["editor"],true,"fade");
    }

    private void GotoHelp()
    {
        if (!isAnimationFinished)
        {
            return;
        }

        Composer.GotoScene(Composer.scenes["help"],true,"fade");
    }

    private void GotoOptions()
    {
        if (!isAnimationFinished)
        {
            return;
        }

        Composer.GotoScene(Composer.scenes["options"],true,"fade");
    }

    private void QuitButton()
    {
        GetTree().Quit();
    }
}

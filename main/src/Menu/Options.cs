using Godot;
using Godot.Collections;
using System;

public class Options : Node
{

    bool isAnimationFinished = false;

    Composer Composer;
    Tween tween;

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

        var musicText = control.GetNode<Label>("CanvasLayer/MusicText");
        musicText.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(musicText,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1f);

        var sfxText = control.GetNode<Label>("CanvasLayer/SFXText");
        sfxText.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(sfxText,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1f);

        var musicBar = control.GetNode<HSlider>("CanvasLayer/MusicBar");
        musicBar.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(musicBar,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1.5f);

        musicBar.Value = Global.musicVolume;

        var sfxBar = control.GetNode<HSlider>("CanvasLayer/SFXBar");
        sfxBar.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(sfxBar,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1.5f);

        sfxBar.Value = Global.sfxVolume;

        var button1 = control.GetNode<TextureButton>("CanvasLayer/QuitButton");
        button1.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button1,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2f);

        tween.Start();

        await ToSignal(tween,"tween_all_completed");

        isAnimationFinished = true;
    }

    public override async void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && !isAnimationFinished)
        {
            if (mouseButton.ButtonIndex == 1) // Skip the animations on mouse press
            {
                var control = GetNode<Control>("Control");

                tween.RemoveAll();

                var title = control.GetNode<Label>("CanvasLayer/GameTitle");
                title.RectGlobalPosition = new Vector2(title.RectGlobalPosition.x,0);

                var devTitle = control.GetNode<Label>("CanvasLayer/Subtitle");
                devTitle.RectGlobalPosition = new Vector2(devTitle.RectGlobalPosition.x,130);

                var musicText = control.GetNode<Label>("CanvasLayer/MusicText");
                musicText.Modulate = new Color(1,1,1,1);

                var sfxText = control.GetNode<Label>("CanvasLayer/SFXText");
                sfxText.Modulate = new Color(1,1,1,1);

                var musicBar = control.GetNode<HSlider>("CanvasLayer/MusicBar");
                musicBar.Modulate = new Color(1,1,1,1);

                var sfxBar = control.GetNode<HSlider>("CanvasLayer/SFXBar");
                sfxBar.Modulate = new Color(1,1,1,1);

                var button1 = control.GetNode<TextureButton>("CanvasLayer/QuitButton");
                button1.Modulate = new Color(1,1,1,1);

                await ToSignal(GetTree().CreateTimer(0.2f),"timeout");

                isAnimationFinished = true;
            }
        }
    }

    void ChangeMusicVolume(float value)
    {
        Global.musicVolume = value;

        var menuTheme = GetTree().Root.GetNode<AudioStreamPlayer>("Area/MenuTheme");
        menuTheme.VolumeDb = Global.musicVolume;
    }

    void ChangeSfxVolume(float value)
    {
        Global.sfxVolume = value;
    }

    void GotoMenu()
    {
        if (!isAnimationFinished)
            return;

        Composer.GotoScene(Composer.scenes["mainmenu"],true,"fade");
    }
}

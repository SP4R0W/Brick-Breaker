using Godot;
using Godot.Collections;
using System;

public class Help : Node
{

    bool isAnimationFinished = false;
    bool isScrollingDone = false;

    Composer Composer;
    Tween tween;

    int page = 0;

    AudioStreamPlayer click;

    Array<string> helpTitle = new Array<string>()
    {
        "Game help:",
        "Editor help:",
        "Credits:"
    };

    Array<string> helpString = new Array<string>()
    {
        "welcome to brick breaker!\nyour goal in this game is to destroy all colorful bricks using the ball while keeping it in the field by bouncing it off with your paddle. you can also set the ball's direction using the paddle. some bricks are regular 1-hitters, some will need more hits, while black bricks can't be destroyed at all! these don't count towards completing a level.\n\ncontrols:\nwasd to control paddle\np to pause game\nm - mute music in game\nl - loop current track in game\narrow keys - switch the track playing in game\nWARNING! At the start of the game or when you lose a life, use A or D key to shoot the ball in preffered direction.",
        "to start editing, first select the editor choice in main menu.\nfirst, either load an existing file from user folder or create a new one. then, select your preferred brick color and brick type from the left panel. to start adding bricks, click on the semi-visible ones. to remove a brick, click on it with your right mouse button. you can also hold the buttons to do the action for multiple bricks.\n\nafter you're done with adding bricks, you can enter in your level's name and your name in the respective fields on the right panel. then, save your file and you're good to go.\n\nBrick sccore values for level creators:\nGrey - 15 pts\nBlue - 25 pts\nGreen - 35 pts\nPurple - 50 pts\n Red - 75 pts\nYellow - 100 pts",
        "game developed by sparrowworks\ncoding by: sp4r0w\ntesting: varga\n\ngraphics:\n- kenney (puzzle pack)\n- DinVStudio (Dyanmic Space Background)\n\nicon by: Freepik (Arkanoid free icon)\n\nfonts:\n- watermelon days by Khurasan\n- poppins by Jonny Pinhorn\n\nmusic by: Of Far Different Nature\n\nSound Effects by:\n- GameAudio (https://freesound.org/people/GameAudio/)\n- Kenney (sci-fi sounds)",
    };

    public async override void _Ready()
    {
        click = GetNode<AudioStreamPlayer>("Click");

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

        devTitle.Text = helpTitle[0];

        var helpText = control.GetNode<RichTextLabel>("CanvasLayer/HelpText");
        helpText.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(helpText,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,.5f);

        helpText.Clear();
        helpText.AppendBbcode("[center]" + helpString[0] + "[/center]");

        var button1 = control.GetNode<TextureButton>("CanvasLayer/NextButton");
        button1.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button1,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1.5f);

        var button2 = control.GetNode<TextureButton>("CanvasLayer/QuitButton");
        button2.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button2,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1.75f);

        tween.Start();

        await ToSignal(tween,"tween_all_completed");

        isAnimationFinished = true;
        isScrollingDone = true;
    }

    public override async void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && !isAnimationFinished)
        {
            if (mouseButton.ButtonIndex == 1) // Skip animations on mouse press
            {
                var control = GetNode<Control>("Control");

                tween.RemoveAll();

                var title = control.GetNode<Label>("CanvasLayer/GameTitle");
                title.RectGlobalPosition = new Vector2(title.RectGlobalPosition.x,0);

                var devTitle = control.GetNode<Label>("CanvasLayer/Subtitle");
                devTitle.RectGlobalPosition = new Vector2(devTitle.RectGlobalPosition.x,130);

                var helpText = control.GetNode<RichTextLabel>("CanvasLayer/HelpText");
                helpText.Modulate = new Color(1,1,1,1);

                var button1 = control.GetNode<TextureButton>("CanvasLayer/NextButton");
                button1.Modulate = new Color(1,1,1,1);

                var button2 = control.GetNode<TextureButton>("CanvasLayer/QuitButton");
                button2.Modulate = new Color(1,1,1,1);

                await ToSignal(GetTree().CreateTimer(0.2f),"timeout");

                isAnimationFinished = true;
                isScrollingDone = true;
            }
        }
    }

    async void ChangePage()
    {
        if (!isAnimationFinished || !isScrollingDone)
            return;

        click.Play();
        click.VolumeDb = Global.sfxVolume;

        isScrollingDone = false;
        var control = GetNode<Control>("Control");

        var titleText = control.GetNode<Label>("CanvasLayer/Subtitle");
        var helpText = control.GetNode<RichTextLabel>("CanvasLayer/HelpText");

        tween.InterpolateProperty(titleText,"modulate:a",1,0,1,Tween.TransitionType.Linear,Tween.EaseType.InOut);

        tween.InterpolateProperty(helpText,"modulate:a",1,0,1,Tween.TransitionType.Linear,Tween.EaseType.InOut);
        tween.Start();

        await ToSignal(tween,"tween_all_completed");

        page++;
        if (page > 2)
            page = 0;

        titleText.Text = helpTitle[page];
        helpText.Clear();
        helpText.AppendBbcode("[center]" + helpString[page] + "[/center]");

        tween.InterpolateProperty(titleText,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut);

        tween.InterpolateProperty(helpText,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut);
        tween.Start();

        await ToSignal(tween,"tween_all_completed");

        isScrollingDone = true;
    }

    void GotoMenu()
    {
        if (!isAnimationFinished || !isScrollingDone)
            return;

        Composer.GotoScene(Composer.scenes["mainmenu"],true,"fade");
    }
}

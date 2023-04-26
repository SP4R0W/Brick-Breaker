using Godot;
using Godot.Collections;
using System;

public class LevelSelect : Node
{

    bool isAnimationFinished = false;

    Composer Composer;
    Tween tween;
    AcceptDialog errorDialog;

    ItemList levelList;

    int page = 0;

    public async override void _Ready()
    {
        Global.levelPath = "";

        Composer = Global.composer;
        var control = GetNode<Control>("Control");
        control.RectSize = new Vector2(Global.screenSize.x,Global.screenSize.y);

        errorDialog = control.GetNode<AcceptDialog>("CanvasLayer/ErrorDialog");
        levelList = control.GetNode<ItemList>("CanvasLayer/ScrollContainer/LevelList");

        for (int i = 1;i < 11;i++)
        {
            var name = "Level " + Convert.ToString(i);
            levelList.AddItem(name);
        }

        var list = GetUserLevelCount();
        if (list != null)
        {
            foreach (Godot.Collections.Array id in list)
            {
                var name = (string)id[0] + " Author:" +(string)id[1] + " ;" + (string)id[2];
                levelList.AddItem(name);
            }
        }

        tween = GetNode<Tween>("Tween");

        var title = control.GetNode<Label>("CanvasLayer/GameTitle");
        title.RectGlobalPosition = new Vector2(title.RectGlobalPosition.x,-500);
        tween.InterpolateProperty(title,"rect_global_position:y",title.RectGlobalPosition.y,0,1,Tween.TransitionType.Quart,Tween.EaseType.InOut);

        var devTitle = control.GetNode<Label>("CanvasLayer/CreatorSubtitle");
        devTitle.RectGlobalPosition = new Vector2(devTitle.RectGlobalPosition.x,-375);
        tween.InterpolateProperty(devTitle,"rect_global_position:y",devTitle.RectGlobalPosition.y,130,1,Tween.TransitionType.Expo,Tween.EaseType.InOut,.5f);

        var scrollContainer = control.GetNode<ScrollContainer>("CanvasLayer/ScrollContainer");
        scrollContainer.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(scrollContainer,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1f);

        var button1 = control.GetNode<TextureButton>("CanvasLayer/QuitButton");
        button1.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button1,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1.5f);

        tween.Start();

        await ToSignal(tween,"tween_all_completed");

        var menuTheme = GetTree().Root.GetNode<AudioStreamPlayer>("Area/MenuTheme");
        if (!menuTheme.Playing)
            menuTheme.Play();

        isAnimationFinished = true;
    }

    void LevelSelected(int index)
    {
        var item = levelList.GetItemText(index);
        var pos = item.Find(";");

        if (pos != -1)
            Global.levelPath = item.Substring(pos+1);
        else
            Global.levelPath = item;

        var menuTheme = GetTree().Root.GetNode<AudioStreamPlayer>("Area/MenuTheme");
        if (menuTheme.Playing)
            menuTheme.Stop();

        Composer.GotoScene(Composer.scenes["game"],true,"fade");
    }

    Godot.Collections.Array GetUserLevelCount()
    {
        var files = new Godot.Collections.Array();
        var dir = new Directory();

        if (dir.Open("user://Levels") != Error.Ok)
        {
            errorDialog.DialogText = "Cannot open user://Levels directory.";
            errorDialog.Show();
            return null;
        }
        dir.ListDirBegin();

        while (true)
        {
            var path = "user://Levels/" + dir.GetNext();

            if (path == "user://Levels/") // We have reached the end of the folder
                break;
            else if (!path.BeginsWith(".") && path.Contains(".level"))
            {
                var file = new File();

                if (file.Open(path,File.ModeFlags.Read) != Error.Ok)
                {
                    errorDialog.DialogText = "Cannot open file: " + path;
                    errorDialog.Popup_();
                    continue;
                }
                else
                {
                    var obj = (Dictionary)file.GetVar();
                    file.Close();

                    var array = new Godot.Collections.Array();
                    var id = (Godot.Collections.Array)obj["id"];
                    array.Add((string)id[0]);
                    array.Add((string)id[1]);
                    array.Add(path);

                    files.Add(array);
                }
            }
        }

        dir.ListDirEnd();

        return files;
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

                var devTitle = control.GetNode<Label>("CanvasLayer/CreatorSubtitle");
                devTitle.RectGlobalPosition = new Vector2(devTitle.RectGlobalPosition.x,130);

                var scrollContainer = control.GetNode<ScrollContainer>("CanvasLayer/ScrollContainer");
                scrollContainer.Modulate = new Color(1,1,1,1);

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


    void GotoMenu()
    {
        if (!isAnimationFinished)
            return;

        Composer.GotoScene(Composer.scenes["mainmenu"],true,"fade");
    }
}
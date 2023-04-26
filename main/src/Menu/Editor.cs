using Godot;
using Godot.Collections;
using System;

public class Editor : Node
{
    AcceptDialog errorDialog;
    FileDialog fileDialog;
    File mainFile;
    Dictionary<string,Godot.Collections.Array> mainLevelDict = new Dictionary<string, Godot.Collections.Array>(){};
    string filePath = "";

    Composer Composer;
    Sprite arrow;
    Sprite arrow2;

    PackedScene brickScene = (PackedScene)ResourceLoader.Load("res://src/Menu/Editor/EditorBrick.tscn");

    public int mouseHeld = 0;

    int normalCount = 0;
    int armorCount = 0;
    int immuneCount = 0;

    int currentColor = 1;
    string currentType = "normal";

    bool isAutoSave = false;
    bool isBrowserGame = false;

    AudioStreamPlayer error;
    AudioStreamPlayer ask;
    AudioStreamPlayer change;

    const int MOUSEBUTTON_LEFT = 1;
    const int MOUSEBUTTON_RIGHT = 2;

    public override void _Ready()
    {
        if (OS.GetName() == "HTML5") // Browser version does not support saving custom levels.
            isBrowserGame = true;

        Composer = Global.composer;
        arrow = GetNode<Sprite>("Control/CanvasLayer/HeadPanel/Arrow1");
        arrow2 = GetNode<Sprite>("Control/CanvasLayer/HeadPanel/Arrow2");

        var control = GetNode<Control>("Control");
        control.RectSize = new Vector2(Global.screenSize.x,Global.screenSize.y);

        errorDialog = GetNode<AcceptDialog>("Control/CanvasLayer/EditorPanel/ErrorDialog");
        fileDialog = GetNode<FileDialog>("Control/CanvasLayer/EditorPanel/FileDialog");

        var panel1 = GetNode<Panel>("Control/CanvasLayer/HeadPanel");
        var panel2 = GetNode<Panel>("Control/CanvasLayer/BodyPanel");

        error = GetNode<AudioStreamPlayer>("Error");
        ask = GetNode<AudioStreamPlayer>("Ask");
        change = GetNode<AudioStreamPlayer>("Change");

        panel1.Visible = false;
        panel2.Visible = false;

        var loadBtn = GetNode<Button>("Control/CanvasLayer/EditorPanel/LoadButton");
        var playBtn = GetNode<Button>("Control/CanvasLayer/EditorPanel/PlayButton");

        if (isBrowserGame)
        {
            loadBtn.Visible = false;
            playBtn.Visible = true;
        }
        else
        {
            loadBtn.Visible = true;
            playBtn.Visible = false;
        }
    }

    void BrickClicked(EditorBrick brick,int mouseStatus)
    {
        Godot.Collections.Array array = mainLevelDict[brick.id];

        // Manage the counters for brick amount
        if (mouseStatus == MOUSEBUTTON_LEFT)
        {
            if (Convert.ToInt32(array[0]) != 0) // A value that isn't 0 means that a block has existed in this spot before
            {
                if (Convert.ToString(array[1]) == "normal")
                {
                    if (normalCount > 0)
                        normalCount--;
                }
                else if (Convert.ToString(array[1]) == "armor")
                {
                    if (armorCount > 0)
                        armorCount--;
                }
                else if (Convert.ToString(array[1]) == "immune")
                {
                    if (immuneCount > 0)
                        immuneCount--;
                }
            }

            if (currentType == "normal")
                normalCount++;
            else if (currentType == "armor")
                armorCount++;
            else if (currentType == "immune")
                immuneCount++;

            array[0] = currentColor;
            array[1] = currentType;

            var brickSprite = brick.GetNode<AnimatedSprite>("AnimatedSprite");

            if (currentType == "immune")
            {
                brickSprite.Animation = "3_armor";
                brick.Modulate = new Color(0.1f,0.1f,0.1f,1);
            }
            else
            {
                brick.Modulate = new Color(1,1,1,1);
                brickSprite.Animation = Convert.ToString(currentColor) + "_" + currentType;
            }
        }
        else if (mouseStatus == MOUSEBUTTON_RIGHT)
        {
            if (Convert.ToInt32(array[0]) != 0) // A value that isn't 0 means that a block has existed in this spot before
            {
                if (Convert.ToString(array[1]) == "normal")
                {
                    if (normalCount > 0)
                        normalCount--;
                }
                else if (Convert.ToString(array[1]) == "armor")
                {
                    if (armorCount > 0)
                        armorCount--;
                }
                else if (Convert.ToString(array[1]) == "immune")
                {
                    if (immuneCount > 0)
                        immuneCount--;
                }
            }

            array[0] = 0;
            array[1] = "normal";
            brick.Modulate = new Color(1,1,1,0.4f);

            var brickSprite = brick.GetNode<AnimatedSprite>("AnimatedSprite");
            brickSprite.Animation = "1_normal";
        }

        var count1 = GetNode<Label>("Control/CanvasLayer/BodyPanel/Count1");
        var count2 = GetNode<Label>("Control/CanvasLayer/BodyPanel/Count2");
        var count3 = GetNode<Label>("Control/CanvasLayer/BodyPanel/Count3");

        count1.Text = "Normal: " + normalCount;
        count2.Text = "Armor: " + armorCount;
        count3.Text = "Immune: " + immuneCount;

        if (isAutoSave && !isBrowserGame)
            SaveFile();
    }

    void EditTitle(string newText)
    {
        mainLevelDict["id"][0] = newText;

        if (isAutoSave && !isBrowserGame)
            SaveFile();
    }

    void EditAuthor(string newText)
    {
        mainLevelDict["id"][1] = newText;

        if (isAutoSave && !isBrowserGame)
            SaveFile();
    }

    void DrawBricks()
    {
        var startX = 715;
        var startY = 350;

        var posX = startX;
        var posY = startY;

        var brickColumns = 15;
        var brickRows = 14;

        var brickNode = GetNode<Node2D>("Control/CanvasLayer/GamePanel/Bricks");

        for (int child=0;child<brickNode.GetChildCount();child++)
        {
            var brick = brickNode.GetChild(child);
            brick.QueueFree();
        }

        for (int i = 0;i <= brickColumns;i++)
        {
            posY = startY;
            for (int j = 0;j <= brickRows;j++)
            {
                var brickId = Convert.ToString(i) + "_" + Convert.ToString(j);

                var brick = (EditorBrick)brickScene.Instance();
                var brickSprite = brick.GetNode<AnimatedSprite>("AnimatedSprite");
                brick.id = brickId;

                var brickTypeData = mainLevelDict[brickId];

                int brickCol = Convert.ToInt32(brickTypeData[0]);
                string brickType = Convert.ToString(brickTypeData[1]);
                string brickSpriteName;

                if (brickCol == 0)
                    brickSpriteName = "1_" + brickType;
                else
                    brickSpriteName = brickCol + "_" + brickType;

                brickNode.AddChild(brick);

                if (brickType == "normal")
                    brickSprite.Animation = brickSpriteName;
                else if (brickType == "armor")
                    brickSprite.Animation = brickSpriteName;
                else if (brickType == "immune")
                    brickSprite.Animation = "3_armor";
                    brick.Modulate = new Color(0.1f,0.1f,0.1f,1);

                brick.GlobalPosition = new Vector2(posX,posY);
                if (brickCol == 0 && brickType != "immune")
                    brick.Modulate = new Color(1,1,1,0.4f);
                else if (brickType != "immune")
                    brick.Modulate = new Color(1,1,1,1);

                brick.Connect("BrickClick",this,"BrickClicked");

                posY += 35;
            }

            posX += 75;
        }
    }

    void ShowEditorPanels()
    {
        var panel1 = GetNode<Panel>("Control/CanvasLayer/HeadPanel");
        var panel2 = GetNode<Panel>("Control/CanvasLayer/BodyPanel");

        panel1.Visible = true;
        panel2.Visible = true;

        var saveBtn = GetNode<Button>("Control/CanvasLayer/BodyPanel/SaveButton");
        var autosave = GetNode<CheckBox>("Control/CanvasLayer/BodyPanel/Autosave");

        if (isBrowserGame)
        {
            saveBtn.Visible = false;
            autosave.Visible = false;
        }
        else
        {
            saveBtn.Visible = true;
            autosave.Visible = true;
        }
    }

    int GetLevelCount()
    {
        var files = new Godot.Collections.Array();
        var dir = new Directory();

        dir.Open("user://Levels");
        dir.ListDirBegin();

        while (true)
        {
            var file = dir.GetNext();
            if (file == "") // Reached the end of the folder.
                break;
            else if (!file.BeginsWith(".") && file.Contains(".level"))
                files.Add(file);
        }

        dir.ListDirEnd();

        return files.Count;
    }

    void PlayFile()
    {
        if (normalCount < 1 && armorCount < 1)
        {
            error.Play();
            errorDialog.DialogText = "File cannot be empty.";
            errorDialog.Popup_();
            return;
        }

        Global.webLevel = mainLevelDict;

        var menuTheme = GetTree().Root.GetNode<AudioStreamPlayer>("Area/MenuTheme");
        menuTheme.Stop();

        Composer.GotoScene(Composer.scenes["game"]);
    }

    void ClearFile()
    {
        var brickColumns = 15;
        var brickRows = 14;

        var brickNode = GetNode<Node2D>("Control/CanvasLayer/GamePanel/Bricks");

        for (int i = 0;i <= brickColumns;i++)
        {
            for (int j = 0;j <= brickRows;j++)
            {
                var brickId = Convert.ToString(i) + "_" + Convert.ToString(j);

                var brickTypeData = mainLevelDict[brickId] = new Godot.Collections.Array(){0,"normal"};
            }
        }

        for (int i = 0;i < brickNode.GetChildCount();i++)
        {
            var child = (EditorBrick)brickNode.GetChild(i);

            var sprite = child.GetNode<AnimatedSprite>("AnimatedSprite");
            sprite.Animation = "1_normal";

            child.Modulate = new Color(1,1,1,0.4f);
        }

            normalCount = 0;
            armorCount = 0;
            immuneCount = 0;

            var count1 = GetNode<Label>("Control/CanvasLayer/BodyPanel/Count1");
            var count2 = GetNode<Label>("Control/CanvasLayer/BodyPanel/Count2");
            var count3 = GetNode<Label>("Control/CanvasLayer/BodyPanel/Count3");

            count1.Text = "Normal: " + normalCount;
            count2.Text = "Armor: " + armorCount;
            count3.Text = "Immune: " + immuneCount;
    }

    void CreateEmptyFile()
    {
        var brickColumns = 15;
        var brickRows = 14;

        for (int i = 0;i <= brickColumns;i++)
        {
            for (int j = 0;j <= brickRows;j++)
            {
                var name = Convert.ToString(i) + "_" + Convert.ToString(j);
                mainLevelDict.Add(name,new Godot.Collections.Array(){0,"normal"});
            }
        }

        mainLevelDict.Add("id",new Godot.Collections.Array(){"new_level","Anon"});

        var levelName = "";

        if (!isBrowserGame)
        {
            Directory dir = new Directory();

            if (dir.Open("user://Levels") != Error.Ok)
            {
                if (dir.MakeDir("user://Levels") != Error.Ok)
                {
                    error.Play();
                    filePath = "";
                    errorDialog.DialogText = "Cannot open user://Levels";
                    errorDialog.Popup_();
                    return;
                }
            }

            var fileCount = GetLevelCount();
            var fileName = "";
            levelName = "";

            if (fileCount == 0)
            {
                levelName = "new_level";
                fileName = "user://Levels/new_level.level";
            }
            else
            {
                levelName = "level" + fileCount;
                fileName = "user://Levels/new_level" + "(" + fileCount + ").level";
            }

            mainFile = new File();

            if (mainFile.Open(fileName,File.ModeFlags.Write) != Error.Ok)
            {
                error.Play();
                filePath = "";
                errorDialog.DialogText = "Cannot create a new file.";
                errorDialog.Popup_();
                return;
            }

            mainFile.StoreVar(mainLevelDict);
            mainFile.Close();

            filePath = fileName;

            var linePath = GetNode<Label>("Control/CanvasLayer/EditorPanel/FileLabel");

            linePath.Text = filePath;
        }
        else
            levelName = "My Level";

        var lineTitle = GetNode<LineEdit>("Control/CanvasLayer/BodyPanel/LineEdit");
        var lineAuth = GetNode<LineEdit>("Control/CanvasLayer/BodyPanel/LineEdit2");

        lineTitle.Text = levelName;
        lineAuth.Text = "Anon";

        ShowEditorPanels();
        DrawBricks();
    }

    bool SaveFile()
    {
        mainFile = new File();
        if (mainFile.Open(filePath,File.ModeFlags.Write) != Error.Ok)
        {
            error.Play();
            errorDialog.DialogText = "Cannot save the file.";
            errorDialog.Popup_();
            return false;
        }

        if (normalCount < 1 && armorCount < 1)
        {
            error.Play();
            errorDialog.DialogText = "File cannot be empty.";
            errorDialog.Popup_();
            return false;
        }

        mainFile.StoreVar(mainLevelDict);
        mainFile.Close();

        var dir = new Directory();

        var newFilePath = "user://Levels/" + mainLevelDict["id"][0] + ".level";

        dir.Rename(filePath,newFilePath);

        var linePath = GetNode<Label>("Control/CanvasLayer/EditorPanel/FileLabel");

        filePath = newFilePath;

        linePath.Text = filePath;

        return true;
    }

    void CheckAutoSave()
    {
        isAutoSave = !isAutoSave;
    }

    void OpenFile()
    {
        change.Play();
        string[] filters = {"*.level ; Level files"};
        fileDialog.Filters = filters;
        fileDialog.Popup_();
    }

    void ConfirmFileSelect(string path)
    {
        if (path.Contains(".level"))
        {
            mainLevelDict = new Dictionary<string, Godot.Collections.Array>(){};

            mainFile = new File();

            if (mainFile.Open(path,File.ModeFlags.Read) != Error.Ok)
            {
                error.Play();
                filePath = "";
                errorDialog.DialogText = "Cannot open the file.";
                errorDialog.Popup_();
                return;
            }

            var obj = (Dictionary)mainFile.GetVar();
            mainFile.Close();

            var brickColumns = 15;
            var brickRows = 14;

            for (int i = 0;i <= brickColumns;i++)
            {
                for (int j = 0;j <= brickRows;j++)
                {
                    var name = Convert.ToString(i) + "_" + Convert.ToString(j);
                    var brickCol = Convert.ToInt32(((Godot.Collections.Array)obj[name])[0]);
                    var brickName = Convert.ToString(((Godot.Collections.Array)obj[name])[1]);

                    if (brickName == "normal" && brickCol != 0)
                                            normalCount++;
                    else if (brickName == "armor")
                        armorCount++;
                    else if (brickName == "immune")
                        immuneCount++;
                    mainLevelDict.Add(name,(Godot.Collections.Array)obj[name]);
                }
            }

            var id = (Godot.Collections.Array)obj["id"];
            mainLevelDict.Add("id",(Godot.Collections.Array)obj["id"]);

            var lineTitle = GetNode<LineEdit>("Control/CanvasLayer/BodyPanel/LineEdit");
            var lineAuth = GetNode<LineEdit>("Control/CanvasLayer/BodyPanel/LineEdit2");
            var linePath = GetNode<Label>("Control/CanvasLayer/EditorPanel/FileLabel");

            filePath = path;

            lineTitle.Text = Convert.ToString(id[0]);
            lineAuth.Text = Convert.ToString(id[1]);
            linePath.Text = filePath;

            var count1 = GetNode<Label>("Control/CanvasLayer/BodyPanel/Count1");
            var count2 = GetNode<Label>("Control/CanvasLayer/BodyPanel/Count2");
            var count3 = GetNode<Label>("Control/CanvasLayer/BodyPanel/Count3");

            count1.Text = "Normal: " + normalCount;
            count2.Text = "Armor: " + armorCount;
            count3.Text = "Immune: " + immuneCount;

            ShowEditorPanels();
            DrawBricks();
        }
    }

    void CheckConfirm()
    {
        if (SaveFile())
            Composer.GotoScene(Composer.scenes["mainmenu"]);
    }

    void CancelSave()
    {
        if (normalCount < 1 && armorCount < 1)
        {
            var dir = new Directory();
            dir.Remove(filePath);
        }

        Composer.GotoScene(Composer.scenes["mainmenu"]);
    }

    void GotoMenu()
    {
        if (filePath != "" && !isBrowserGame)
        {
            ask.Play();
            var confirm = GetNode<ConfirmationDialog>("Control/CanvasLayer/EditorPanel/ConfirmationDialog");
            confirm.GetCancel().Connect("pressed",this,"CancelSave");
            confirm.Popup_();
        }
        else
            Composer.GotoScene(Composer.scenes["mainmenu"]);
    }

    void BlueColorPick()
    {
        change.Play();
        arrow.Position = new Vector2(85,145);
        currentColor = 1;
    }

    void GreenColorPick()
    {
        change.Play();
        arrow.Position = new Vector2(85,245);
        currentColor = 2;
    }

    void GreyColorPick()
    {
        change.Play();
        arrow.Position = new Vector2(85,345);
        currentColor = 3;
    }

    void PurpleColorPick()
    {
        change.Play();
        arrow.Position = new Vector2(85,445);
        currentColor = 4;
    }

    void RedColorPick()
    {
        change.Play();
        arrow.Position = new Vector2(85,545);
        currentColor = 5;
    }

    void YellowColorPick()
    {
        change.Play();
        arrow.Position = new Vector2(85,645);
        currentColor = 6;
    }

    void PickNormal()
    {
        change.Play();
        arrow2.Position = new Vector2(85,835);
        currentType = "normal";
    }

    void PickArmor()
    {
        change.Play();
        arrow2.Position = new Vector2(85,905);
        currentType = "armor";
    }

    void PickImmune()
    {
        change.Play();
        arrow2.Position = new Vector2(85,975);
        currentType = "immune";
    }

    public override void _Process(float delta)
    {
        if (Input.IsMouseButtonPressed(1))
            mouseHeld = MOUSEBUTTON_LEFT;
        else if(Input.IsMouseButtonPressed(2))
            mouseHeld = MOUSEBUTTON_RIGHT;
        else
            mouseHeld = 0;
    }
}

using Godot;
using System;
using Godot.Collections;

public class Global : Node
{
    public static Composer composer;

    public static Vector2 screenSize;
    public static float musicVolume = 0;
    public static float sfxVolume = 0;

    public static string levelPath = "";

    public static int totalScore = 0;
    public static int totalTime = 0;
    public static int totalBricks = 0;
    public static int totalDeaths = 0;
    public static string levelName = "";
    public static string levelAuth = "";

    public static Dictionary<string, Godot.Collections.Array> webLevel = null;

    public override void _Ready()
    {
        composer = (Composer)GetNode("/root/Composer");
    }
}

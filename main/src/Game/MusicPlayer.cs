using Godot;
using Godot.Collections;
using System;

public class MusicPlayer : Node2D
{
    // This class is responsible for playing music during gameplay. It provides functions for playing random tracks or play tracks in a playlist


    [Signal]
    public delegate void MusicChanged(int newTrack);

    [Signal]
    public delegate void LoopChanged(bool newLoop);

    int currentTrack = 0;
    int prevTrack = -1;

    public bool canInteract = false;
    public bool isLooped = false;
    bool isStopped = false;

    Array<AudioStreamPlayer> tracks = new Array<AudioStreamPlayer>();
    public Array<string> trackNames = new Array<string>();

    public override void _Ready()
    {
        isStopped = false;

        for (int i = 0;i<GetChildCount();i++)
        {
            var child = GetChild(i);
            tracks.Add((AudioStreamPlayer)child);
            trackNames.Add(child.Name);
        }
    }

    public void PlayRandom(bool gameover=false)
    {
        if (gameover)
            isStopped = false;

        while (currentTrack == prevTrack)
        {
            currentTrack = Convert.ToInt32((GD.Randi() % (tracks.Count - 1)));
            if (currentTrack != prevTrack)
            {
                prevTrack = currentTrack;
                break;
            }
        }

        if (!isStopped)
        {
            EmitSignal("MusicChanged",currentTrack);
            tracks[currentTrack].VolumeDb = Global.musicVolume;
            tracks[currentTrack].Play();
        }
    }

    public void StopPlaying()
    {
        isStopped = true;
        tracks[currentTrack].Stop();
    }

    public void StartPlaying()
    {
        EmitSignal("MusicChanged",currentTrack);
        isStopped = false;
        tracks[currentTrack].VolumeDb = Global.musicVolume;
        tracks[currentTrack].Play();
    }

    void TrackFinished()
    {
        if (isStopped)
            return;

        if (!isLooped)
            PlayRandom();
        else
        {
            tracks[currentTrack].VolumeDb = Global.musicVolume;
            tracks[currentTrack].Play();
        }
    }

    void MusicVolumeChanged(float value)
    {
       Global.musicVolume = value;
       tracks[currentTrack].VolumeDb = Global.musicVolume;
    }

    void SfxVolumeChanged(float value)
    {
        Global.sfxVolume = value;
    }

    void PlayPrevTrack()
    {
        StopPlaying();

        currentTrack--;
        if (currentTrack < 0)
            currentTrack = tracks.Count - 1;

        prevTrack = currentTrack;

        GetTree().CreateTimer(0.1f).Connect("timeout",this,"StartPlaying");
    }

    void PlayNextTrack()
    {
        StopPlaying();

        currentTrack++;
        if (currentTrack > (tracks.Count - 1))
            currentTrack = 0;

        prevTrack = currentTrack;

        GetTree().CreateTimer(0.1f).Connect("timeout",this,"StartPlaying");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("mute") && canInteract)
        {
            if (isStopped)
                StartPlaying();
            else
                StopPlaying();
        }

        if (@event.IsActionPressed("loop") && canInteract)
        {
            isLooped = !isLooped;
            EmitSignal("LoopChanged",isLooped);
        }

        if (@event.IsActionPressed("next_track") && canInteract)
            PlayNextTrack();

        if (@event.IsActionPressed("prev_track") && canInteract)
            PlayPrevTrack();
    }
}

using Godot;
using System;

public class Level_3 : Node2D
{
    public override void _Process(float delta)
    {
        SoundController sound = GetNode<SoundController>("/root/SoundController");
        sound.ChangeMusic(3);       // changes music to boss level music (Level 3)
    }
}

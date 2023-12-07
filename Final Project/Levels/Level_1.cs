using Godot;
using System;

public class Level_1 : Node2D
{
    public override void _Process(float delta)
    {
        // changes music to forest music (Level 1)
        SoundController sound = GetNode<SoundController>("/root/SoundController");
        sound.ChangeMusic(1);
    }
}

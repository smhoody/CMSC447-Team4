using Godot;
using System;

public class Level_2 : Node2D
{
    // changes music to dungeon music (Level 2)
    public override void _Process(float delta)
    {
        SoundController sound = GetNode<SoundController>("/root/SoundController");
        sound.ChangeMusic(2);
    }
}

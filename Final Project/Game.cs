using Godot;
using System;

public class Game : Node2D
{
    MusicController music;
    public override void _Ready()
    {
        music = GetNode<MusicController>("MusicController");
        GetTree().ChangeScene("res://MainMenu.tscn");
        
    }

    public override void _Process(float delta)
    {

    }

}

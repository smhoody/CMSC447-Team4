using Godot;
using System;

public class Game : Node2D
{
    
    public override void _Ready()
    {
        GetTree().ChangeScene("res://Menus/MainMenu.tscn");     // when game first starts, the scene is changed to main menu
    }
}

using Godot;
using System;

public class DashCooldownLabel : ProgressBar
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
 public override void _Process(float delta)
 {
    Player p = (Player)GetNode("/root/Player");
    // GD.Print(p.dash_cooldown.TimeLeft);
    //display cooldown value as a percentage. Full bar = dash available
    this.Value = (1 - p.dash_cooldown.TimeLeft) * 100;
 }
}

using Godot;
using System;

public class DashCooldownLabel : ProgressBar
{
    public Player p;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        p = (Player)GetNode("/root/Player");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
 public override void _Process(float delta)
 {
    //display cooldown value as a percentage. Full bar = dash available
    this.Value = (1 - p.dash_cooldown.TimeLeft) * 100;
 }
}

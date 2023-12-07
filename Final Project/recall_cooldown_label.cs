using Godot;
using System;

public class recall_cooldown_label : ProgressBar
{
    public Player p;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // p = (Player)GetNode("/root/Player");
        // p = (Player)this.GetParent().GetParent().GetParent(); 
        p = (Player)this.GetParent().GetParent().GetParent().GetNode("Player");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
 public override void _Process(float delta)
 {
    //display cooldown value as a percentage. Full bar = recall available
    this.Value = (1 - p.recall_cooldown.TimeLeft / p.recall_cooldown_value) * 100;
 }
}


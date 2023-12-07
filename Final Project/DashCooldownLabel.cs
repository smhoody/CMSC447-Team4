using Godot;
using System;

public class DashCooldownLabel : ProgressBar
{
    public Player p;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // p = (Player)GetNode("res://Player.tscn");
        // Node n = this.GetParent();
        // CanvasLayer c = (CanvasLayer)n.GetParent();
        // p = (Player)c.GetParent(); //.GetParent().GetParent();
        //getParent = Panel, getParentx2 = HUD (CanvasLayer)
        p = (Player)this.GetParent().GetParent().GetParent().GetNode("Player");    //.GetChild(); 
        // GD.Print(n1.Name);
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
 public override void _Process(float delta)
 {
    //display cooldown value as a percentage. Full bar = dash available
    this.Value = (1 - p.dash_cooldown.TimeLeft / p.dash_cooldown_value) * 100;
 }
}

using Godot;
using System;

public class boss_health_bar : ProgressBar
{
    public BossEnemy b;
    public override void _Ready()
    {
        b = (BossEnemy)this.GetParent().GetParent().GetParent().GetNode("BossEnemy");
    }

    public override void _Process(float delta)
    {
        this.Value = b.health;
    }
}

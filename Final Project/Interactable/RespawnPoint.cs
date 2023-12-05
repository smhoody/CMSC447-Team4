using Godot;
using System;

public class RespawnPoint : Node2D 
{
    [Export] public bool spawnpoint = false;
    private bool activated = false;

    private AnimationPlayer animation;

    public override void _Ready()
    {
        animation = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void Activate()
    {
        GameManager.current_checkpoint = this;
        activated = true;
        animation.Play("Activated");
    }

    private void _on_Area2D_area_entered(Area2D area)
    {
        if(area.GetParent() is Player && !activated)
        {
            Activate();
        }
    }
}

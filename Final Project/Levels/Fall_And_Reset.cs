using Godot;
using System;

public class Fall_And_Reset : CollisionShape2D
{
    private void _on_Fall_Hitbox_area_entered(Area2D area)
    {
        if(area.GetParent() is Player)
        {
            GetTree().ChangeScene("res://Level_2.tscn");    // if the player falls into the collision area, the level resets
        }
    }
}

using Godot;
using System;

public class Fall_And_Reset : Area2D
{
    private void _on_Fall_Hitbox_body_entered(Node body)
    {
        if(body is Player)
        {
            GetTree().ChangeScene("res://Levels/Level_2.tscn");    // if the player falls into the collision area, the level resets
        }
    }
}

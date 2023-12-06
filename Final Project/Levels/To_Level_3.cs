using Godot;
using System;

public class To_Level_3 : CollisionShape2D
{
    private void _on_Hitbox_area_entered(Area2D area)
    {
        if(area.GetParent() is Player)
        {
            SoundController sound = GetNode<SoundController>("/root/SoundController");
            sound.StopMusicPlayer();                        // stops current music 
            GetTree().ChangeScene("res://Level_3.tscn");    // changes current scene to level 3
        }
    }
}

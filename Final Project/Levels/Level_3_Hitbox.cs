using Godot;
using System;

public class Level_3_Hitbox : Area2D
{
    private void _on_Level_3_Hitbox_body_entered(Node body)
    {
        if(body is Player)
        {
            SoundController sound = GetNode<SoundController>("/root/SoundController");
            sound.StopMusicPlayer();                        // stops current music
            GetTree().ChangeScene("res://Levels/Level_3.tscn");    // changes current scene to level 3
        }
    }
}

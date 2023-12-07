using Godot;
using System;

public class Level_2_Hitbox : Area2D
{

    private void _on_Level_2_Hitbox_body_entered(Node body)
    {
        if(body is Player)
        {
            SoundController sound = GetNode<SoundController>("/root/SoundController");
            sound.StopMusicPlayer();                        // stops current music
            GetTree().ChangeScene("res://Levels/Level_2.tscn");    // changes current scene to level 2
        }
    }
}

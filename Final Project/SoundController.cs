using Godot;
using System;
using System.IO;

public class SoundController : Node
{
    private AudioStreamPlayer menu_music;
    private AudioStreamPlayer level1_music;
    private AudioStreamPlayer level2_music;
    private AudioStreamPlayer level3_music;
    private AudioStreamPlayer confirm_sfx;
    private AudioStreamPlayer damage_enemy_sfx;

    public override void _Ready()
    {
        // Loads each music file to the corresponding AudioStream
        menu_music = GetNode<AudioStreamPlayer>("Menu_Music");
        menu_music.Stream = GD.Load<AudioStream>("res://Music/1 titles LOOP.ogg");

        level1_music = GetNode<AudioStreamPlayer>("Level1_Music");
        level1_music.Stream = GD.Load<AudioStream>("res://Music/11 forest LOOP.ogg");

        level2_music = GetNode<AudioStreamPlayer>("Level2_Music");
        level2_music.Stream = GD.Load<AudioStream>("res://Music/2 dungeon LOOP.ogg");

        level3_music = GetNode<AudioStreamPlayer>("Level3_Music");
        level3_music.Stream = GD.Load<AudioStream>("res://Music/14 BOSS y INITIAL.ogg");

        confirm_sfx = GetNode<AudioStreamPlayer>("Confirm_SFX");
        confirm_sfx.Stream = GD.Load<AudioStream>("res://SFX/10_UI_Menu_SFX/013_Confirm_03.wav");
        
        damage_enemy_sfx = GetNode<AudioStreamPlayer>("Damage_Enemy_SFX");
        damage_enemy_sfx.Stream = GD.Load<AudioStream>("res://SFX/12_Player_Movement_SFX/56_Attack_03.wav");
    }        

    public void StopMusicPlayer()
    {
        // Stops all music that is playing
        menu_music.Stop();
        level1_music.Stop();
        level2_music.Stop();
        level3_music.Stop();
    }
    public void ChangeMusic(int song)
    {
        switch(song)
        {
            case 0:
                if(!menu_music.Playing)     // Prevents game from infinitely restarting music
                    menu_music.Play();
                break;
            case 1:
                if(!level1_music.Playing)
                    level1_music.Play();
                break;
            case 2:
                if(!level2_music.Playing)
                    level2_music.Play();
                break;
            case 3:
                if(!level3_music.Playing)
                    level3_music.Play();
                break;
        }
    }

    public void PlaySFX(int sfx)
    {
        switch(sfx)
        {
            case 0: // Play menu selection SFX
                if(!confirm_sfx.Playing)    
                    confirm_sfx.Play();
                break;
            case 1:
                if(!damage_enemy_sfx.Playing)
                    damage_enemy_sfx.Play();
                break;
        }
    }
}

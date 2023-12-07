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
    private AudioStreamPlayer recall_sfx;
    private AudioStreamPlayer jump_sfx;
    private AudioStreamPlayer wall_sfx;
    private AudioStreamPlayer dash_sfx;
    private AudioStreamPlayer quick_attack_sfx;
    private AudioStreamPlayer heavy_attack_sfx;
    private AudioStreamPlayer walk_sfx;
    private AudioStreamPlayer take_damage_sfx;
    private AudioStreamPlayer spider_death_sfx;
    private AudioStreamPlayer player_death_sfx;

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

        recall_sfx = GetNode<AudioStreamPlayer>("Recall_SFX");
        recall_sfx.Stream = GD.Load<AudioStream>("res://SFX/12_Player_Movement_SFX/88_Teleport_02.wav");

        jump_sfx = GetNode<AudioStreamPlayer>("Jump_SFX");
        jump_sfx.Stream = GD.Load<AudioStream>("res://SFX/12_Player_Movement_SFX/30_Jump_03.wav");
        
        wall_sfx = GetNode<AudioStreamPlayer>("Wall_SFX");
        wall_sfx.Stream = GD.Load<AudioStream>("res://SFX/12_Player_Movement_SFX/42_Cling_climb_03.wav");

        dash_sfx = GetNode<AudioStreamPlayer>("Dash_SFX");
        dash_sfx.Stream = GD.Load<AudioStream>("res://SFX/10_Battle_SFX/35_Miss_Evade_02.wav");

        quick_attack_sfx = GetNode<AudioStreamPlayer>("QuickAttack_SFX");
        quick_attack_sfx.Stream = GD.Load<AudioStream>("res://SFX/10_Battle_SFX/51_Flee_02.wav");

        heavy_attack_sfx = GetNode<AudioStreamPlayer>("HeavyAttack_SFX");
        heavy_attack_sfx.Stream = GD.Load<AudioStream>("res://SFX/12_Player_Movement_SFX/56_Attack_03.wav");
        
        walk_sfx = GetNode<AudioStreamPlayer>("Walk_SFX");

        take_damage_sfx = GetNode<AudioStreamPlayer>("TakeDamage_SFX");

        spider_death_sfx = GetNode<AudioStreamPlayer>("SpiderDeath_SFX");

        player_death_sfx = GetNode<AudioStreamPlayer>("PlayerDeath_SFX");
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

    
    ///<summary>
    /// Audio Player for Sound Effects.
    /// <list type="bullet">
    /// <item> 0 = Confirm</item>
    /// <item> 1 = Damage Enemy</item>
    /// <item> 2 = Player Recall</item>
    /// <item> 3 = Player Jump</item>
    /// <item> 4 = Player Wall Jump</item>
    /// <item> 5 = Player Dash</item>
    /// <item> 6 = Player Quick Attack</item>
    /// <item> 7 = Player Heavy Attack</item>
    /// <item> 8 = Player Walk</item>
    /// <item> 9 = Player Take Damage</item>
    /// <item> 10 = Spider Death</item>
    /// <item> 11 = Player Death</item>
    /// </list>
    /// </summary>
    public void PlaySFX(int sfx)
    {
        switch(sfx)
        {
            case 0: // Play menu selection SFX
                if(!confirm_sfx.Playing) confirm_sfx.Play();
                break;
            case 1: // 
                if(!damage_enemy_sfx.Playing) damage_enemy_sfx.Play();
                break;
            case 2: // recall ability sound
                if(!recall_sfx.Playing) recall_sfx.Play();
                break;
            case 3: // jump sound
                jump_sfx.Play();
                break;
            case 4: // wall jump sound
                wall_sfx.Play();
                break;
            case 5: // dash sound
                dash_sfx.Play();
                break;
            case 6: // quick attack sound
                quick_attack_sfx.Play();
                break;
            case 7: // heavy attack sound
                heavy_attack_sfx.Play();
                break;
            case 8: // player walking sound
                walk_sfx.Play();
                break;
            case 9: // player taking damage sound
                take_damage_sfx.Play();
                break;
            case 10: // spider death sound
                spider_death_sfx.Play();
                break;
            case 11: // player death sound
                player_death_sfx.Play();
                break;
        }
    }
}

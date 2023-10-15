using Godot;
using System;

public class MusicController : Node
{
    private AudioStreamPlayer2D menu_music;
    public override void _Ready()
    {
        menu_music = GetNode<AudioStreamPlayer2D>("Menu Music");
    }        

    public void PlayMusic()
    {
        menu_music.Play();
    }
}

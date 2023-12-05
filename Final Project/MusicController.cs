using Godot;
using System;

public class MusicController : Node
{
    private AudioStreamPlayer menu_music;
    public override void _Ready()
    {
        menu_music = GetNode<AudioStreamPlayer>("Menu Music");
    }        

    public void PlayMusic()
    {
        menu_music.Play();
    }
}

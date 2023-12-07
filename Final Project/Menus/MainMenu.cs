using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public class MainMenu : MarginContainer
{
    // Declares node variables
    private Label selector_one, selector_two, selector_three;
    private int current_selection = 0;
    public override void _Ready()
    {
        selector_one = GetNode<Label>("CenterContainer/VBoxContainer/CenterContainer2/VBoxContainer/CenterContainer/HBoxContainer/Selector");
        selector_two = GetNode<Label>("CenterContainer/VBoxContainer/CenterContainer2/VBoxContainer/CenterContainer2/HBoxContainer/Selector");
        selector_three = GetNode<Label>("CenterContainer/VBoxContainer/CenterContainer2/VBoxContainer/CenterContainer3/HBoxContainer/Selector");
        SetCurrentSelection(current_selection);
        
    }

    public override void _Process(float delta)
    {
        // Initializes SoundController Autoload to play music/sfx
        SoundController sound = GetNode<SoundController>("/root/SoundController");
        sound.ChangeMusic(0);


        if(Input.IsActionJustPressed("down"))
        {
            current_selection++;
            current_selection %= 3;
            SetCurrentSelection(current_selection);
        }
        else if(Input.IsActionJustPressed("up"))
        {
            current_selection--;
            current_selection %= 3;
            if(current_selection < 0)
            {
                current_selection += 3;
            }
            SetCurrentSelection(current_selection);
        }
        else if(Input.IsActionJustPressed("ui_accept"))
        {
            sound.PlaySFX(0);
            HandleSelection(current_selection);
        }
    }

    public void SetCurrentSelection(int current_selection)
    {
        selector_one.Text = "";
        selector_two.Text = "";
        selector_three.Text = "";

        switch(current_selection)
        {
            case 0:
                selector_one.Text = ">";
                break;
            case 1: 
                selector_two.Text = ">";
                break;
            case 2:
                selector_three.Text = ">";
                break;
        }

    }

    public void HandleSelection(int current_selection)
    {
        SoundController sound = GetNode<SoundController>("/root/SoundController");
        switch(current_selection)
        {
            case 0:
                sound.StopMusicPlayer();
                Input.MouseMode = Input.MouseModeEnum.Hidden;
                GetTree().ChangeScene("res://Levels/Level_2.tscn");
                break;
            case 1:
                GetTree().ChangeScene("res://Menus/OptionsMenu.tscn");
                break;
            case 2:
                GetTree().Quit();
                break;
        }
    }
}

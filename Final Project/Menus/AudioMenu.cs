using Godot;
using System;

public class AudioMenu : Control
{
    // Declares all node variables
    private Label selector_one, selector_two, selector_three, selector_four;
    private HSlider master_slider, music_slider, sfx_slider;
    // Declares private variables
    private int master_index, music_index, sfx_index;
    private int current_selection = 0;
    private double slider_speed = 0.01;

    public override void _Ready()
    {
        // Initializes all node variables to corresponding node
        selector_one = GetNode<Label>("HBoxContainer/Selectors/Selector");
        selector_two = GetNode<Label>("HBoxContainer/Selectors/Selector2");
        selector_three = GetNode<Label>("HBoxContainer/Selectors/Selector3");
        selector_four = GetNode<Label>("HBoxContainer3/Selector4");

        master_slider = GetNode<HSlider>("HBoxContainer2/Sliders/Master");
        music_slider = GetNode<HSlider>("HBoxContainer2/Sliders/Music");
        sfx_slider = GetNode<HSlider>("HBoxContainer2/Sliders/SFX");

        master_index = AudioServer.GetBusIndex("Master");
        music_index = AudioServer.GetBusIndex("Music");
        sfx_index = AudioServer.GetBusIndex("SFX");

        SetCurrentSelection(current_selection);

        // saves the position of the slider values, so it's maintained when going back to Audio Menu
        master_slider.Value = GameManager.pos_master;
        music_slider.Value = GameManager.pos_music;
        sfx_slider.Value = GameManager.pos_sfx;
    }

    public override void _Process(float delta)
    {
        // Moves to next option, depending on keyboard down/up inputs
        if(Input.IsActionJustPressed("down"))
        {
            current_selection++;
            current_selection %= 4;
            SetCurrentSelection(current_selection);
        }
        else if(Input.IsActionJustPressed("up"))
        {
            current_selection--;
            current_selection %= 4;
            if(current_selection < 0)
            {
                current_selection += 4;
            }
            SetCurrentSelection(current_selection);
        }
        HandleSelection(current_selection);
    }
    public void SetCurrentSelection(int current_selection)
    {
        // Moves the ">" cursor to whatever menu selection is currently being hovered
        selector_one.Text = "";
        selector_two.Text = "";
        selector_three.Text = "";
        selector_four.Text = "";

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
            case 3:
                selector_four.Text = ">";
                break;
        }
    }
    public void HandleSelection(int current_selection)
    {
        SoundController sound = GetNode<SoundController>("/root/SoundController");
        if(current_selection == 0)
        {
            if(Input.IsActionPressed("right"))
            {
                master_slider.Value += slider_speed;
            }
            if(Input.IsActionPressed("left"))
            {
                master_slider.Value -= slider_speed;
            }
        }
        if(current_selection == 1)
        {
            if(Input.IsActionPressed("right"))
            {
                music_slider.Value += slider_speed;
            }
            if(Input.IsActionPressed("left"))
            {
                music_slider.Value -= slider_speed;
            }
        }
        if(current_selection == 2)
        {
            if(Input.IsActionPressed("right"))
            {
                sfx_slider.Value += slider_speed;
            }
            if(Input.IsActionPressed("left"))
            {
                sfx_slider.Value -= slider_speed;
            }
        }
        if(current_selection == 3 && Input.IsActionJustPressed("ui_accept"))
        {
            sound.PlaySFX(0);
            GameManager.pos_master = master_slider.Value;
            GameManager.pos_music = music_slider.Value;
            GameManager.pos_sfx = sfx_slider.Value;
            GetTree().ChangeScene("res://Menus/OptionsMenu.tscn");
        }
    }
    public void _on_slider_value_changed(float MyFloat, string slider)
    {
        // Changes the actual Bus volumes for master, music, sfx
        switch(slider)
        {
            case "Master":
                AudioServer.SetBusVolumeDb(master_index, GD.Linear2Db(MyFloat));
                break;
            case "Music":
                AudioServer.SetBusVolumeDb(music_index, GD.Linear2Db(MyFloat));
                break;
            case "SFX":
                AudioServer.SetBusVolumeDb(sfx_index, GD.Linear2Db(MyFloat));
                break;
            default:
                GD.Print("No valid slider specified");
                break;
        }
    }
}

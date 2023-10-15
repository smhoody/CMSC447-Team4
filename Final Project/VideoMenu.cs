using Godot;
using System;

public class VideoMenu : MarginContainer
{
    private Label selector_one, selector_two, selector_three;
    private int current_selection = 0;

    public override void _Ready()
    {
        selector_one = GetNode<Label>("CenterContainer/VBoxContainer/CenterContainer/HBoxContainer/Selector");
        selector_two = GetNode<Label>("CenterContainer/VBoxContainer/CenterContainer2/HBoxContainer/Selector");
        selector_three = GetNode<Label>("CenterContainer/VBoxContainer/CenterContainer3/HBoxContainer/Selector");
        SetCurrentSelection(current_selection);
    }

    public override void _Process(float delta)
    {
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

        switch(current_selection)
        {
            case 0:
                if(OS.WindowFullscreen)
                    OS.WindowFullscreen = false;
                else
                    OS.WindowFullscreen = true;
                break;
            case 1:
                if(OS.WindowBorderless)
                    OS.WindowBorderless = false;
                else
                    OS.WindowBorderless = true;
                break;
            case 2:
                GetTree().ChangeScene("res://OptionsMenu.tscn");
                break;
        } 
    }
}

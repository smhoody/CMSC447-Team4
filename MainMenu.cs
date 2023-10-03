using Godot;
using System;

public class MainMenu : MarginContainer
{
    
    private Label selector_one;
    private Label selector_two;
    private Label selector_three;
    private int current_selection = 0;

    public override void _Ready()
    {
        selector_one = GetNode<Label>("CenterContainer/VBoxContainer/CenterContainer2/VBoxContainer/CenterContainer/HBoxContainer/Selector");
        selector_two = GetNode<Label>("CenterContainer/VBoxContainer/CenterContainer2/VBoxContainer/CenterContainer2/HBoxContainer/Selector");
        selector_three = GetNode<Label>("CenterContainer/VBoxContainer/CenterContainer2/VBoxContainer/CenterContainer3/HBoxContainer/Selector");
        SetCurrentSelection(current_selection);
    }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if(Input.IsActionJustPressed("ui_down"))
        {
            current_selection++;
            current_selection %= 3;
            SetCurrentSelection(current_selection);
        }
        else if(Input.IsActionJustPressed("ui_up"))
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
                // Change to the name file path of the level file.
                GetTree().ChangeScene("res://Node2D.tscn");
                break;
            case 1:
                Console.WriteLine("Go To Options");
                break;
            case 2:
                GetTree().Quit();
                break;
        }
    }
}

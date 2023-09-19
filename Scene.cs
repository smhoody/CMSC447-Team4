using Godot;
using System;

public class Scene : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private Camera2D camera1;
    private Camera2D camera2;
    private KinematicBody2D player;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        player = GetNode<KinematicBody2D>("Player");
        camera1 = GetNode<Camera2D>("Camera1");
        camera2 = GetNode<Camera2D>("Camera2");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (player.GlobalPosition.x >= 1050) {
            camera1.Current = false;
            camera2.Current = true;
        } else {
            camera2.Current = false;
            camera1.Current = true;
        }
    }
}

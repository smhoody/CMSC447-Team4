using Godot;
using System;

public class Character : KinematicBody {
	
    [Export]
    float speed = 40f;
    [Export]
    float acceleration = 15f;
    [Export]
    float max_terminal_velocity = 54f;
    [Export]
    float jump_power = 20f;
    [Export]
    float gravity = 0.98f;

    private Vector3 velocity; 
    private float y_velocity;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta) 
    {

	}
    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        handle_movement(delta);
    }

    private async void handle_movement(float delta)
    {
        Vector3 direction = new Vector3(Vector3.Zero);

        if (Input.IsActionPressed("forward"))
            direction -= Transform.basis.z;
        if (Input.IsActionPressed("backward"))
            direction += Transform.basis.z;
        if (Input.IsActionPressed("left"))
            direction -= Transform.basis.x;
        if (Input.IsActionPressed("right"))
            direction += Transform.basis.x;
        
        direction = direction.Normalized();

        velocity = velocity.LinearInterpolate(direction * speed, acceleration*delta);

        if (IsOnFloor())
            y_velocity = -0.01f; // apply a small amount of downward force if on floor
        else
            y_velocity = Mathf.Clamp(y_velocity-gravity, -max_terminal_velocity, max_terminal_velocity);

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
            y_velocity = jump_power;
        
        velocity.y = y_velocity;
        velocity = MoveAndSlide(velocity, Vector3.Up);
    }
}

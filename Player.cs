using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export] public bool right = true;
    [Export] public int speed = 230;
    [Export] public int dash_speed = 300;
    [Export] public float gravity = 9.81f;
    [Export] public float jump_power = 500f;
    [Export] public float mass = 2f;
    [Export] public float acceleration = 20f;

    // member variables here
    public Vector2 velocity = new Vector2();
    public float y_velocity;
    private AnimatedSprite _animatedSprite;
    private RayCast2D groundray;
    private Timer timer;
    private int dash_accel = 25; 
    private int dash_counter = 0; 
    private bool dashing = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        groundray = (RayCast2D) FindNode("GroundRay"); // GetNode<RayCast2D>("GroundRay");
        groundray.CollideWithAreas = true;
        groundray.Enabled = true; 

        timer = GetNode<Timer>("Timer");
        timer.Connect("timeout",this,"OnTimerTimeout");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        velocity.y += gravity*mass;
        GetInput(delta);
        CheckAnimations(delta);
        velocity = MoveAndSlide(velocity);
        velocity.x = 0;
    }

    public override void _PhysicsProcess(float delta) {
        
    }

    public void GetInput(float delta) {
        if (Input.IsActionPressed("right")) {
            _animatedSprite.FlipH = false;
            velocity.x += 1;
        }
        else if (Input.IsActionPressed("left")) { 
            _animatedSprite.FlipH = true;
            velocity.x -= 1;
        }
        
        if (groundray.IsColliding()) {
            if (Input.IsActionJustPressed("jump")) {
                velocity.y -= jump_power;
            }
        }

        if (Input.IsActionJustPressed("dash")) {
            dashing = true;
            timer.Start();
        }

        if (dashing) {
            dash_counter++;
            speed = dash_speed * (dash_counter/dash_accel);
        } else {speed = 230;}
                    
        velocity.x *= speed;
        
    }

    public void CheckAnimations(float delta) {
        if (!groundray.IsColliding()) {
            _animatedSprite.Play("jump");
        } else {
            if (Input.IsActionPressed("right")) {
                _animatedSprite.Play("run");
            }
            else if (Input.IsActionPressed("left")) {
                _animatedSprite.Play("run");
            }
            else {
                _animatedSprite.Stop();
                _animatedSprite.Frame = 0;
            }
        }
    }

    private void OnTimerTimeout() {
        dash_counter = 0;
        dashing = false;
    }
}

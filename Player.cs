using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export] public bool right = true;
    [Export] public int speed = 230;
    [Export] public int dash_speed = 15;
    [Export] public float gravity = 9.81f;
    [Export] public float jump_power = 500f;
    [Export] public float mass = 2f;
    [Export] public float acceleration = 2f;

    // member variables here
    public Vector2 velocity = new Vector2();
    public float y_velocity;
    private AnimatedSprite _animatedSprite;
    private RayCast2D groundray;
    

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        groundray = (RayCast2D) FindNode("GroundRay"); // GetNode<RayCast2D>("GroundRay");
        groundray.CollideWithAreas = true;
        groundray.Enabled = true;  
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        velocity.y += gravity*mass;
        GetInput();
        CheckAnimations(delta);
        velocity = MoveAndSlide(velocity);
        velocity.x = 0;
    }

    public override void _PhysicsProcess(float delta) {
        
    }

    public void GetInput() {

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

        if (Input.IsActionJustPressed("dash") && Input.IsActionPressed("right")) {
            velocity.x += dash_speed;
        } 
        else if (Input.IsActionJustPressed("dash") && Input.IsActionPressed("left")) {
            velocity.x -= dash_speed;
        }


        velocity.x = velocity.x * speed;
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
}

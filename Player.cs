using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export] public bool right = true;
    [Export] public int speed = 230;
    [Export] public float gravity = 9.81f;
    [Export] public float jump_power = 650f;
    [Export] public float mass = 2f;
    [Export] public bool isJumping = false;

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
        velocity = MoveAndSlide(velocity);
        velocity.x = 0;
    }

    public override void _PhysicsProcess(float delta) {
        
    }

    public void GetInput() {

        if (Input.IsActionJustPressed("jump")) {
            velocity.y -= jump_power;
        }
        if (Input.IsActionPressed("right")) {
            _animatedSprite.FlipH = false;
            _animatedSprite.Play("run");
            velocity.x += 1;
        }
        else if (Input.IsActionPressed("left")) {
            _animatedSprite.FlipH = true;
            _animatedSprite.Play("run");
            velocity.x -= 1;
        }
        else {
            _animatedSprite.Stop();
            _animatedSprite.Frame = 0;
        }
        
        velocity.x = velocity.x * speed;
    }

    public void CheckAnimations(float delta) {
        if (!groundray.IsColliding() && !isJumping) {
            isJumping = true;
            _animatedSprite.Play("jump");
        }

    }
}

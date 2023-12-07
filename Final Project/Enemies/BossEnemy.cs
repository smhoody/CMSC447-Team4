using Godot;
using System;

public class BossEnemy : KinematicBody2D
{
    // Exported variables
    [Export] public float gravity = 9.81f;
    [Export] public float mass = 0.8f;
    [Export] public float jump_power = 400f;
    [Export] public float speed = 200f;
    [Export] public float attack_speed = 500f;
    [Export] public float follow_distance = 500f;
    [Export] public float attack_distance = 130f;
    [Export] public int damage = 12;


    private int moveDirection = -1;
    private Timer attack_cooldown;
    private Timer attack_duration;
    
    private Timer jump_cooldown;

    private Vector2 velocity = new Vector2();
    public Vector2 player_position = new Vector2();
    public Vector2 target_position = new Vector2();
    private AnimatedSprite _animatedSprite;
    private RayCast2D castDownRight;
    private RayCast2D castDownLeft;
    private RayCast2D castLookAhead;
    private KinematicBody2D player_node;
    private Player player;
    public int health = 1;
    public bool is_dead = false;
    public Timer take_damage_timer;
    private Hitbox hitbox;
    private CollisionShape2D hitbox_collision_obj;
    public bool attacking;
    public bool taking_damage;
    private CPUParticles2D death_particles;
    private Timer death_timer;
    public SoundController sound;

    public override void _Ready()
    {
        // Both used to check floor collisions
        castDownLeft = GetNode<RayCast2D>("DownLeftRay");
        castDownRight = GetNode<RayCast2D>("DownRightRay");

        // Used to check wall/player collisions
        castLookAhead = GetNode<RayCast2D>("LookRay"); 

        // Player sprite
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

        // Used to obtain the position of the Player
        player_node = GetParent().GetNode<KinematicBody2D>("Player");
        player = (Player)player_node;

        // timer for attack duration
        attack_duration = GetNode<Timer>("AttackDuration");

        // timer for attack cooldown
        attack_cooldown = GetNode<Timer>("AttackCooldown");
        attack_cooldown.OneShot = true;

        //Received Damage Timer
        take_damage_timer = GetNode<Timer>("TakeDamageTimer");

        // Boss hitbox
        hitbox = GetNode<Hitbox>("Hitbox");
        hitbox_collision_obj = hitbox.GetChild<CollisionShape2D>(0);
        
        hitbox.setDamage(damage); //set attack damage for Boss

        sound = GetNode<SoundController>("/root/SoundController");

        jump_cooldown = GetNode<Timer>("JumpCooldown");
        jump_cooldown.OneShot = true;

        //Death sequence nodes
        death_particles = GetNode<CPUParticles2D>("DeathParticles");
        death_timer = GetNode<Timer>("DeathTimer");
    }

    public override void _Process(float delta)
    {
         if(ProcessTurn())
        {
            moveDirection *= -1;
        }
        castLookAhead.CastTo = new Vector2(46*moveDirection, 0);
    }

    public override void _PhysicsProcess(float delta)
    {
        ApplyGravity(delta);
        HandleMovement(delta);
        CheckAnimations(delta);
    }

    public void ApplyGravity(float delta)
    {
        velocity.y += mass*gravity;
    }

    public void HandleMovement(float delta)
    {
        if (is_dead || player.hurtbox_collision_obj.Disabled) {return;} //no moving during death animation

        float relative_speed = speed;

        player_position = player.Position;
        target_position.x = player_position.x - this.Position.x;
        
        velocity.x = target_position.x > 0 ? speed : speed*-1;

        if(Position.DistanceTo(player_position) < follow_distance)
        {
            hitbox_collision_obj.Disabled = true;

            // if y position of player is higher than boss, the boss will jump
            if(player_position.y < Position.y && jump_cooldown.IsStopped())
            {
                velocity.y -= jump_power;
                jump_cooldown.Start();
            }

            //check if within range to attack player
            if (Position.DistanceTo(player_position) < attack_distance) {
                //if Enemy attack is available to use
                if(attack_cooldown.IsStopped() && !player.hurtbox_collision_obj.Disabled) {
                    hitbox_collision_obj.Disabled = false;
                    hitbox.setDamage(damage);
                    attack_cooldown.Start();
                    attacking = true;

                    double choice = GD.Randi();
                    if(choice % 2 == 0)
                        _animatedSprite.Play("attack_1");
                    else
                        _animatedSprite.Play("attack_2");
                    }
            } else {
                attacking = false;
            }

            velocity = MoveAndSlide(velocity);
        } 

    }

    private bool ProcessTurn()
    {
        if(castLookAhead.IsColliding())
        {
            return true;
        }
        if(moveDirection == -1)
        {
            return !castDownLeft.IsColliding();
        }
        else if(moveDirection == 1)
        {
            return !castDownRight.IsColliding();
        }
        return false;
    }

    public void CheckAnimations(float delta)
    {
        if (is_dead) { //if character has already died, play animation or remove the character
            death_particles.Emitting = true;

            //if the death animation is finished
            if (death_timer.IsStopped()) {
                QueueFree();
                GetTree().ChangeScene("res://VictoryScreen.tscn");
                sound.StopMusicPlayer();
                sound.ChangeMusic(4);
            }

            return; //return so that no other animation is played
        }

        //no animations needed once player is dead
        if (player.hurtbox_collision_obj.Disabled) {
            _animatedSprite.Stop();
            return;
        }
        //if taking damage animation is not finished, don't process any other animation
        if (!take_damage_timer.IsStopped()) {return;}

        //check if boss is attacking
        if (attacking) {
            return;
        }

        if(Position.DistanceTo(player_position) < follow_distance)
        {
            _animatedSprite.Play("run");
        }
        else
        {
            _animatedSprite.Play("idle");
        }

        if(player_position.x > Position.x)
        {
            _animatedSprite.FlipH = true;
        }
        else if(player_position.x < Position.x)
        {
            _animatedSprite.FlipH = false;
        }
    }

    /**
    Signal function for when an area enters this character's area
    */
    private void _on_Hurtbox_area_entered(Area2D area) {
        //Only allow Player to interact with this area
        if(area.GetParent() is Player) {
            CollisionShape2D col = area.GetChild<CollisionShape2D>(0);

            // EXIT IF:
            // 1) if not a hitbox (in which case area is a hurtbox)
            // 2) if incoming hitbox is not enabled
            if (!(area is Hitbox) || col.Disabled) {return;} 
            
            //Convert to hitbox type to get damage data
            Hitbox hitbox = (Hitbox) area;
            int damage = hitbox.getDamage();

            //deal damage to health
            health -= damage;
            
            sound.PlaySFX(1);

            if (take_damage_timer.IsStopped()) {
                take_damage_timer.Start(); //start timer for taking damage animation
                _animatedSprite.Play("take_damage");   
            } 
            GD.Print("boss hit " + damage.ToString() + " (" + health.ToString() + " HP)");    

            //check if character died
            if (health <= 0 && !is_dead) {
                Die();
            } else { //character not dead, begin cooldown timer
                //start cooldown timer for taking damage (player is immune for this duration)
                take_damage_timer.Start();
            }
        }
    }

    private void Die() {
        if (!is_dead) {  
            is_dead = true; //set dead flag to true
            death_timer.Start(); //begin timer for death animation 
            _animatedSprite.FlipV = true; //turn pup sprite upside down
            _animatedSprite.Position = new Vector2(-1, 5); //move it down so that it is laying on floor
            sound.PlaySFX(10); //play spider death sound
        }
    }

}

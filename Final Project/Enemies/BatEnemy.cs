using Godot;
using System;

public class BatEnemy : KinematicBody2D
{
    [Export] public float gravity = 9.81f;
    [Export] public float mass = 0.8f;
    [Export] public float dive_power = 300f;
    [Export] public float speed = 100f;
    [Export] public float attack_speed = 500f;
    [Export] public float follow_distance = 500f;
    [Export] public float attack_distance = 130f;
    [Export] public int damage = 25;
    [Export] public int health = 70;


    private int moveDirection = -1;
    private Timer attack_cooldown;
    private Timer attack_duration;
    

    private Vector2 velocity = new Vector2();
    public Vector2 player_position = new Vector2();
    public Vector2 target_position = new Vector2();
    private AnimatedSprite _animatedSprite;
    private RayCast2D castDown;
    private RayCast2D castLeft;
    private RayCast2D castRight;
    private RayCast2D castUp;
    private KinematicBody2D player_node;
    private Player player;
    public bool is_dead = false;
    public Timer take_damage_timer;
    private Hitbox hitbox;
    private CollisionShape2D hitbox_collision_obj;
    public bool attacking;
    public bool taking_damage;
    private CPUParticles2D death_particles;
    private Timer death_timer;
    private SoundController sound;
    public override void _Ready()
    {
        // Get SoundController Node for playing Spider sounds
        sound = GetNode<SoundController>("/root/SoundController");

        // Used to check Wall/Floor collisions
        castLeft = GetNode<RayCast2D>("LeftRay");
        castRight = GetNode<RayCast2D>("RightRay");
        castUp = GetNode<RayCast2D>("UpRay");
        castDown = GetNode<RayCast2D>("DownRay");

        // Bat sprite
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

        // Bat hitbox
        hitbox = GetNode<Hitbox>("Hitbox");
        hitbox_collision_obj = hitbox.GetChild<CollisionShape2D>(0);
        hitbox.setDamage(damage); //set attack damage for Bat

        //Death sequence nodes
        death_particles = GetNode<CPUParticles2D>("DeathParticles");
        death_timer = GetNode<Timer>("DeathTimer");
    }

    public override void _Process(float delta)
    {
        HandleMovement(delta);
        CheckAnimations(delta);
    }

    public void HandleMovement(float delta)
    {
        if (is_dead || player.hurtbox_collision_obj.Disabled) {return;} //no moving during death animation

        float relative_speed = speed;

        player_position = player_node.Position;
        target_position.x = player_position.x - this.Position.x;
        target_position.y = player_position.y - this.Position.y; //add 50 for target buffer
        
        velocity.x = target_position.x > 0 ? speed : speed*-1;
        velocity.y = target_position.y > 0 ? speed : speed*-1;
        //if bat is touching wall on right
        if(!castLeft.IsColliding() && castRight.IsColliding() && target_position.x > 0)
        {
            velocity.x = 0;
        }
        //if bat is touching wall on left
        else if(!castRight.IsColliding() && castLeft.IsColliding() && target_position.x < 0)
        {
            velocity.x = 0;
        }
        if(Position.DistanceTo(player_position) < follow_distance)
        {
            hitbox_collision_obj.Disabled = true;
            //check if within range to attack player
            if (Position.DistanceTo(player_position) < attack_distance) {
                //if Enemy attack is available to use & player hurtbox is enabled (not dead)
                if(attack_cooldown.IsStopped() && !player.hurtbox_collision_obj.Disabled) {
                    hitbox_collision_obj.Disabled = false;
                    hitbox.setDamage(damage);
                    attack_cooldown.Start();
                    attacking = true;
                }
            } else {
                attacking = false;
            }
            velocity = MoveAndSlide(velocity);
        }
    }

    public void CheckAnimations(float delta)
    {
        if (is_dead) { //if character has already died, play animation or remove the character
            death_particles.Emitting = true;
    
            //if the death animation is finished
            if (death_timer.IsStopped()) {
                QueueFree();
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

         //check if spider is attacking
        if (attacking) {
            _animatedSprite.Play("attack");
            return;
        }

        //default animation for Bat whether it is chasing or not
        _animatedSprite.Play("idle");
        
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
            
            if (take_damage_timer.IsStopped()) {
                take_damage_timer.Start(); //start timer for taking damage animation
                _animatedSprite.Play("take_damage");   
            } 
            GD.Print("bat hit " + damage.ToString() + " (" + health.ToString() + " HP)");    

            //check if character died
            if (health <= 0 && !is_dead) {
                Die();
            }
        }
    }

    private void Die() {
        //if the character just died
        if (!is_dead) {  
            is_dead = true; //set dead flag to true
            death_timer.Start(); //begin timer for death animation 
            _animatedSprite.FlipV = true;
            _animatedSprite.Position = new Vector2(-1, 29);
            sound.PlaySFX(10); //play bat death sound
        }

    }


}

using Godot;
using System;
using System.Diagnostics;
using System.Xml.Serialization;

public class Enemy : KinematicBody2D
{
    private const int walkSpeed = 80;
    private int speed, randomSign;
    private Vector2 velocityX, velocityZ;
    private Player player;
    readonly private RandomNumberGenerator rng = new RandomNumberGenerator();
    private float randomTimer = 0f, maxPositionX;
    private bool lockHorizontal = false, isAtLimits = false;
    private AnimatedSprite animatedSprite;

    public override void _Ready()
    {
        CallDeferred(nameof(AddPlayer));

        Scale = new Vector2(0.71f, 0.71f);
        velocityX = new Vector2(1, 0);
        velocityZ = new Vector2(1, 1);
        speed = walkSpeed;

        animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

        rng.Randomize();
    }

    private void AddPlayer()
    {
        player = GameManager.Instance.player;
        maxPositionX = player.maxPositionX;
    }

    public override void _Process(float delta)
    {
        // Ensures correct depth visibility based on scale
        ZIndex = Mathf.RoundToInt(Scale.x * 100);
   
    }

    public override void _PhysicsProcess(float delta)
    {
        randomTimer -= delta;
        if (randomTimer <= 0)
        {
            lockHorizontal = !lockHorizontal;
            PickNewDirection();
        }
            
        EnemyMovement(delta);
        //Vector2 direction = player.GlobalPosition - GlobalPosition;
        //direction = direction.Normalized();   
    }

    private void PickNewDirection()
    {
        if (!lockHorizontal)
        {
            randomSign = rng.Randf() < 0.5f ? 1 : -1;
            velocityX *= isAtLimits ? -1 : randomSign;
            velocityX = velocityX.Normalized() * speed;
            HorizontalAnimation();              
        }
        else
        {
            randomSign = rng.Randf() < 0.5f ? 1 : -1;
            velocityZ *= isAtLimits ? -1 : randomSign;
            velocityZ = velocityZ.Normalized() * speed * 0.01f;
            VerticalAnimation();      
        }

        randomTimer = rng.RandfRange(1.5f, 4);
    }

    private void EnemyMovement(float delta)
    {
        if (!lockHorizontal)
            Position += velocityX * delta;
        else
            Scale += velocityZ * delta * 0.2f;   

        // Check if enemy has reached level limits and change walking direction
        if (randomTimer > 0)
        {
            if (Mathf.Abs(Position.x) >= maxPositionX)
            {
                lockHorizontal = false; // Enemy will change direction on X axis
                isAtLimits = true;
                PickNewDirection();
            }  
            else if (Scale.x <= 0.5f || Scale.x >= 0.99f)
            {
                lockHorizontal = true; // Enemy will change direction on Z axis
                isAtLimits = true;
                PickNewDirection();
            }
            else    
                isAtLimits = false;       
        }
    }

    private void HorizontalAnimation()
    {
        string direction = velocityX > Vector2.Zero ? "_Right" : "_Left";

        string animationName = "Walk";
        animationName += $"{direction}";
        SetAnimation(animationName);
    }

    private void VerticalAnimation()
    {
        string direction = velocityZ > Vector2.Zero ? "_Front" : "_Back";

        string animationName = "Walk";
        animationName += $"{direction}";
        SetAnimation(animationName);
    }

    private void SetAnimation(string animationName)
    {
        if (animatedSprite.Animation != animationName)
            animatedSprite.Animation = animationName;
    }    
}
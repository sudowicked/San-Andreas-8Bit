using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export] public int walkSpeed = 80, runSpeed = 160;
    private int speed;

    private Vector2 screenSize;
    private AnimatedSprite animatedSprite;
    private Camera2D camera;
    private float maxPositionX;
    private bool weaponEquipped, isZooming;

    private enum Direction {Back, Front, Right, Left};
    private Direction currentDirection = Direction.Back;

    public override void _Ready()
    {
        Engine.TargetFps = 30;

        screenSize = GetViewportRect().Size;

        // Each background sprite has a width of 320px. We have 5 sprites in total so 1600px of width
        // In order to set the boundaries of where the player can move we need to first subtract the actual viewport width 
        // and then divide by 2 because the player is placed in the center of the screen
        maxPositionX = (1600 - screenSize.x) / 2f; // For 640px we get 480px - Works for any viewport width

        camera = GetNode<Camera2D>("Camera2D");
        animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

        // Setting initial player animation and spee
        SetAnimation("CJ_Idle_Back");
        speed = walkSpeed;
    }

    public override void _Process(float delta)
    {
        Vector2 velocity = GetMovementInput();

        // Updating player position
        Position += velocity * delta;

        HandleWeaponInput(velocity);
        ClampPlayer();
    }
    
    private Vector2 GetMovementInput()
    {
        Vector2 velocity = Vector2.Zero;

        if (Input.IsActionPressed("left"))
        {
            currentDirection = Direction.Left;
            velocity.x -= 1;
            HorizontalAnimation(isRight: false);
        }
        else if (Input.IsActionPressed("right"))
        {
            currentDirection = Direction.Right;
            velocity.x += 1;
            HorizontalAnimation(isRight: true);
        }
        else if (Input.IsActionPressed("forward"))
        {
            currentDirection = Direction.Back;
            isZooming = true;
            VerticalAnimation(isDown: false);
        }
        else if (Input.IsActionPressed("backwards"))
        {
            currentDirection = Direction.Front;
            isZooming = true;
            VerticalAnimation(isDown: true);
        }

        SetIdleAnimation(velocity);

        if (velocity.Length() > 0)
            velocity = velocity.Normalized() * speed;

        return velocity;
    }

    private void HandleWeaponInput(Vector2 velocity)
    {
        if (Input.IsActionPressed("equip") && !weaponEquipped)
        {
            weaponEquipped = true;
            SetIdleAnimation(velocity);
        }
        else if (Input.IsActionPressed("unequip") && weaponEquipped)
        {
            weaponEquipped = false;
            SetIdleAnimation(velocity);
        }
    }

    private void HorizontalAnimation(bool isRight)
    {
        bool running = Input.IsActionPressed("run");
        speed = running ? runSpeed : walkSpeed;

        string moveType = running ? "_Run" : "_Walk";
        string side = isRight ? "_Right" : "_Left";
        string rifle = weaponEquipped ? "_Rifle" : "";

        SetAnimation($"CJ{moveType}{side}{rifle}");
    }

    private void VerticalAnimation(bool isDown)
    {
        bool running = Input.IsActionPressed("run");
        float zoomStep = running ? 0.012f : 0.006f;
        camera.Zoom += new Vector2(zoomStep * (isDown ? 1 : -1), zoomStep * (isDown ? 1 : -1));

        string moveType = running ? "_Run" : "_Walk";
        string direction = isDown ? "_Front" : "_Back";
        string rifle = weaponEquipped ? "_Rifle" : "";

        SetAnimation($"CJ{moveType}{direction}{rifle}");
    }

    private void SetIdleAnimation(Vector2 velocity)
    {
        // Checking if player has stopped moving vertically
        if (Input.IsActionJustReleased("forward") || Input.IsActionJustReleased("backwards"))
            isZooming = false;

        if (velocity != Vector2.Zero || isZooming)
            return;

        string rifle = weaponEquipped ? "_Rifle" : "";
        string animationName = "CJ_Idle_";

        switch (currentDirection)
        {
            case Direction.Back:
                animationName += $"Back{rifle}";
                break;
            case Direction.Front:
                animationName += $"Front{rifle}";
                break;
            case Direction.Right:
                animationName += $"Right{rifle}";
                break;
            case Direction.Left:
                animationName += $"Left{rifle}";
                break;
        }

        SetAnimation(animationName);
    }

    private void SetAnimation(string animationName)
    {
        if (animatedSprite.Animation != animationName)
            animatedSprite.Animation = animationName;
    }

    private void ClampPlayer()
    {
        // Clamp player position
        Position = new Vector2(
            Mathf.Clamp(Position.x, -maxPositionX, maxPositionX),
            Mathf.Clamp(Position.y, -screenSize.y, screenSize.y)
        );

        // Clamp camera zoom
        camera.Zoom = new Vector2(
            Mathf.Clamp(camera.Zoom.x, 0.5f, 1),
            Mathf.Clamp(camera.Zoom.y, 0.5f, 1)
        );

        // Adjust player scale relative to zoom
        Scale = camera.Zoom;
    }
}

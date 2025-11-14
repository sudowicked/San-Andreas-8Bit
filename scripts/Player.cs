using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export] public int walkSpeed = 80, runSpeed = 160;
    private int speed;

    private Vector2 screenSize;
    private AnimatedSprite animatedSprite;
    private Sprite muzzleRight, muzzleLeft, muzzleFront, muzzleBack, currentMuzzle;
    private Camera2D camera;
    private float maxPositionX;
    private bool weaponEquipped, weaponAimed, weaponShoot, isZooming;
    private Timer timer;

    private enum Direction {Back, Front, Right, Left};
    private Direction currentDirection = Direction.Back;

    public override void _Ready()
    {
        Engine.TargetFps = 30;
        Input.MouseMode = Input.MouseModeEnum.Hidden;

        screenSize = GetViewportRect().Size;

        // Each background sprite has a width of 320px. We have 5 sprites in total so 1600px of width
        // In order to set the boundaries of where the player can move we need to first subtract the actual viewport width 
        // and then divide by 2 because the player is placed in the center of the screen
        maxPositionX = (1600 - screenSize.x) / 2f; // For 640px we get 480px - Works for any viewport width

        camera = GetNode<Camera2D>("Camera2D");
        animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        muzzleRight = GetNode<Sprite>("AnimatedSprite/MuzzleRight");
        muzzleLeft = GetNode<Sprite>("AnimatedSprite/MuzzleLeft");
        muzzleFront = GetNode<Sprite>("AnimatedSprite/MuzzleFront");
        muzzleBack = GetNode<Sprite>("AnimatedSprite/MuzzleBack");
        timer = GetNode<Timer>("MuzzleFlashTimer");

        // Setting initial player animation and speed
        SetAnimation("Idle_Back");
        speed = walkSpeed;
    }

    public override void _Process(float delta)
    {
        Vector2 velocity = GetMovementInput();

        // Updating player position
        Position += velocity * delta;

        WeaponEquip(velocity);
        ClampPlayer();
    }
    
    private Vector2 GetMovementInput()
    {
        Vector2 velocity = Vector2.Zero;

        if (!weaponShoot)
        {
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
        }

        SetIdleAnimation(velocity);

        if (velocity.Length() > 0)
            velocity = velocity.Normalized() * speed;

        return velocity; 
    }

    private void WeaponEquip(Vector2 velocity)
    {
        if (Input.IsActionPressed("equip") && !weaponEquipped)
        {
            weaponEquipped = true;
        }
        else if (Input.IsActionPressed("unequip") && weaponEquipped)
        {
            weaponEquipped = false;
            weaponAimed = false;
        }
        SetIdleAnimation(velocity);
        WeaponAim();
    }

    private void WeaponAim()
    {
        if (Input.IsActionJustPressed("aim") && weaponEquipped)
        {
            weaponAimed = !weaponAimed;
        }
        WeaponShoot();
    }

    private void WeaponShoot()
    {
        if (Input.IsActionJustPressed("shoot") && weaponAimed)
        {
            weaponShoot = true;

            // Stopping vertical movement when shooting
            isZooming = false;

            switch (currentDirection)
            {
                case Direction.Right:
                    currentMuzzle = muzzleRight;
                    break;
                case Direction.Left:
                    currentMuzzle = muzzleLeft;
                    break;
                case Direction.Front:
                    currentMuzzle = muzzleFront;
                    break;
                case Direction.Back:
                    currentMuzzle = muzzleBack;
                    break;
            }

            // Setting the timer for the muzzle flash animation
            timer.Connect("timeout", this, "MuzzleAnimation", new Godot.Collections.Array { currentMuzzle });
            timer.Start();
        }
        if (Input.IsActionJustReleased("shoot") && weaponAimed)
        {
            weaponShoot = false;
            timer.Stop();
            timer.Disconnect("timeout", this, "MuzzleAnimation");
            currentMuzzle.Visible = false;
        }
    }
    
    private void MuzzleAnimation(Sprite currentMuzzle)
    {

        currentMuzzle.Visible = !currentMuzzle.Visible;
        timer.Start();
    }

    private void HorizontalAnimation(bool isRight)
    {
        bool running = Input.IsActionPressed("run") && !weaponAimed;
        speed = running ? runSpeed : walkSpeed;

        string moveType = running ? "Run" : "Walk";
        string side = isRight ? "_Right" : "_Left";
        string rifle = weaponEquipped ? "_Rifle" : "";
        string aim = weaponAimed ? "_Aim" : "";

        SetAnimation($"{moveType}{side}{rifle}{aim}");
    }

    private void VerticalAnimation(bool isDown)
    {
        bool running = Input.IsActionPressed("run") && !weaponAimed;
        float zoomStep = running ? 0.012f : 0.006f;
        camera.Zoom += new Vector2(zoomStep * (isDown ? 1 : -1), zoomStep * (isDown ? 1 : -1));

        string moveType = running ? "Run" : "Walk";
        string direction = isDown ? "_Front" : "_Back";
        string rifle = weaponEquipped ? "_Rifle" : "";
        string aim = weaponAimed ? "_Aim" : "";

        SetAnimation($"{moveType}{direction}{rifle}{aim}");
    }

    private void SetIdleAnimation(Vector2 velocity)
    {
        // Checking if player has stopped moving vertically
        if (Input.IsActionJustReleased("forward") || Input.IsActionJustReleased("backwards"))
            isZooming = false;

        if (velocity != Vector2.Zero || isZooming)
            return;

        string animationName = "Idle_";
        string rifle = weaponEquipped ? "_Rifle" : "";
        string aim = weaponAimed ? "_Aim" : "";
        string shoot = weaponShoot ? "_Shoot" : "";

        switch (currentDirection)
        {
            case Direction.Back:
                animationName += $"Back{rifle}{aim}{shoot}";
                break;
            case Direction.Front:
                animationName += $"Front{rifle}{aim}{shoot}";
                break;
            case Direction.Right:
                animationName += $"Right{rifle}{aim}{shoot}";
                break;
            case Direction.Left:
                animationName += $"Left{rifle}{aim}{shoot}";
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

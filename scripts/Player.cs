using Godot;
using System;

public class Player : KinematicBody2D
{
    private const int walkSpeed = 80, runSpeed = 160, maxAmmoRounds = 50;
    private int speed, currenAmmoRounds, extraAmmoRounds, ammoToLoad;

    private Vector2 screenSize;
    private AnimatedSprite animatedSprite;
    private Sprite muzzleRight, muzzleLeft, muzzleFront, muzzleBack, currentMuzzle;
    private Camera2D camera;
    private float maxPositionX;
    private bool weaponEquipped, weaponAimed, isShooting, isZooming, isReloading;
    private Timer muzzleFlashTimer, ammoTimer, reloadTimer;
    private Label AmmoUI;

    private enum Direction {Back, Front, Right, Left};
    private Direction currentDirection = Direction.Back;

    public override void _Ready()
    {
        Engine.TargetFps = 30;
        Input.MouseMode = Input.MouseModeEnum.Hidden;

        screenSize = GetViewportRect().Size;

        // Each background sprite has a width of 320px. We have 5 sprites in total so 1600px of width.
        // In order to set the boundaries of where the player can move we need to first subtract the actual viewport width
        // and then divide by 2 because the player is placed in the center of the screen.
        maxPositionX = (1600 - screenSize.x) / 2f; // For 640px we get 480px - Works for any viewport width.

        camera = GetNode<Camera2D>("Camera2D");
        animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        muzzleRight = GetNode<Sprite>("AnimatedSprite/MuzzleRight");
        muzzleLeft = GetNode<Sprite>("AnimatedSprite/MuzzleLeft");
        muzzleFront = GetNode<Sprite>("AnimatedSprite/MuzzleFront");
        muzzleBack = GetNode<Sprite>("AnimatedSprite/MuzzleBack");
        muzzleFlashTimer = GetNode<Timer>("MuzzleFlashTimer");
        ammoTimer = GetNode<Timer>("AmmoTimer");
        reloadTimer = GetNode<Timer>("ReloadTimer");
        AmmoUI = GetNode<Label>("Ak47Icon/AmmoUI");

        // Connecting timers for function loops
        reloadTimer.Connect("timeout", this, "AmmoReload");
        ammoTimer.Connect("timeout", this, "AmmoShoot");
        muzzleFlashTimer.Connect("timeout", this, "MuzzleAnimation");

        // Setting initial player animation and speed
        SetAnimation("Idle_Back");
        speed = walkSpeed;

        extraAmmoRounds = maxAmmoRounds * 2;
        currenAmmoRounds = maxAmmoRounds;
    }

    public override void _Process(float delta)
    {
        Vector2 velocity = GetMovementInput();

        // Updating player position
        Position += velocity * delta;

        WeaponEquip(velocity);
        ClampPlayer();
        AddAmmoRounds();

        AmmoUI.Text = extraAmmoRounds.ToString() + "-" + currenAmmoRounds.ToString();
    }
    
    private Vector2 GetMovementInput()
    {
        Vector2 velocity = Vector2.Zero;

        if (!isShooting && !isReloading)
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
        if ((Input.IsActionPressed("equip") || Input.IsActionJustPressed("cycleWeapons")) && !weaponEquipped)
        {
            weaponEquipped = true;
        }
        else if ((Input.IsActionPressed("unequip") || Input.IsActionJustPressed("cycleWeapons")) && weaponEquipped)
        {
            weaponEquipped = false;
            weaponAimed = false;
        }
        SetIdleAnimation(velocity);
        WeaponAim();
        WeaponReload();
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
        if (Input.IsActionJustPressed("shoot") && weaponAimed && !isReloading && currenAmmoRounds > 0)
        {
            StartShooting();
        }

        if (Input.IsActionJustReleased("shoot") && isShooting)
        {
            StopShooting();
        }
    }

    private void WeaponReload()
    {
        if (Input.IsActionPressed("reload") && weaponEquipped && !isReloading && currenAmmoRounds != maxAmmoRounds)
        {
            StartReload();
        }
    }

    private void AddAmmoRounds()
    {
        if (Input.IsActionPressed("addAmmo"))
        {
            extraAmmoRounds += 200;
        }
    }

    private void StartShooting()
    {
        isShooting = true;
        isZooming = false;

        // Choose correct muzzle flash depending on the direction the player is facing
        switch (currentDirection)
        {
            case Direction.Right: currentMuzzle = muzzleRight; break;
            case Direction.Left: currentMuzzle = muzzleLeft; break;
            case Direction.Front: currentMuzzle = muzzleFront; break;
            case Direction.Back: currentMuzzle = muzzleBack; break;
        }

        // Shooting effects
        muzzleFlashTimer.Start();
        ammoTimer.Start();

        currentMuzzle.Visible = true;

        GD.Print("Shooting");
    }

    private void StopShooting()
    {
        isShooting = false;

        muzzleFlashTimer.Stop();
        ammoTimer.Stop();

        currentMuzzle.Visible = false;
    }
    
    private void AmmoShoot()
    {
        if (currenAmmoRounds > 0)
        {
            currenAmmoRounds -= 1; 

            if (currenAmmoRounds == 0)
            {
                OutOfAmmo();
            }
        }
    }

    // Either reload if there are extra ammo rounds in reserve or stop shooting if there are no extra ammo rounds left
    private void OutOfAmmo()
    {
        if (extraAmmoRounds > 0)
            StartReload();
        else
            StopShooting();
    }

    private void StartReload()
    {
        if (extraAmmoRounds > 0)
        {
            isReloading = true;
            StopShooting();
            reloadTimer.Start();

            GD.Print("Reloading..");   
        }
    }
    
    private void AmmoReload()
    {
        isReloading = false;
        
        // Calculate the amount of ammo required for reload:
        // Choose the minimum value between the actual ammo needed to reach max ammo rounds and the extra ammo rounds left in reserve.
        // If current ammo is 45 and the max current ammo is 50 that means we need 5 ammmo rounds from the reserve.
        // If the extra ammo rounds value is less than 5 we will add this value to our current ammo value, otherwise we will add 5.
        ammoToLoad = Math.Min(maxAmmoRounds - currenAmmoRounds, extraAmmoRounds);

        currenAmmoRounds += ammoToLoad;
        extraAmmoRounds -= ammoToLoad;
        
        weaponAimed = true;
        GD.Print("Reloaded");

        // Continue shooting if left click is still pressed
        if (Input.IsActionPressed("shoot"))
            StartShooting();
    }

    private void MuzzleAnimation()
    {
        currentMuzzle.Visible = !currentMuzzle.Visible;
        muzzleFlashTimer.Start(); // flicker effect
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
        string aim = (weaponAimed && !isShooting && !isReloading) ? "_Aim" : "";
        string shoot = isShooting  ? "_Shoot" : "";
        string reload = isReloading ? "_Reload" : "";

        switch (currentDirection)
        {
            case Direction.Back: animationName += $"Back{rifle}{aim}{shoot}{reload}"; break;
            case Direction.Front: animationName += $"Front{rifle}{aim}{shoot}{reload}"; break;
            case Direction.Right: animationName += $"Right{rifle}{aim}{shoot}{reload}"; break;
            case Direction.Left: animationName += $"Left{rifle}{aim}{shoot}{reload}"; break;
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

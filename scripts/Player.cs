using Godot;
using System;
using System.Diagnostics;

public class Player : KinematicBody2D
{
    [Signal]
    public delegate void WeaponChanged(string iconName, bool ammoVisible);
    private const int walkSpeed = 80, runSpeed = 160, maxAmmoRounds = 50;
    public int speed, currentAmmoRounds, extraAmmoRounds, ammoNeeded, ammoToLoad, money;
    private string iconName;
    private Vector2 velocity, screenSize;
    private AnimatedSprite animatedSprite;
    private Sprite muzzleRight, muzzleLeft, muzzleFront, muzzleBack, currentMuzzle;
    public Camera2D camera;
    public float maxPositionX;
    public bool weaponEquipped, weaponAimed, isShooting, isZooming, isReloading, inputEnabled = true, enteredSceneA = true, ammoVisible, sceneTransitioning = false;
    public Timer muzzleFlashTimer, ammoTimer, reloadTimer, enableInputTimer;

    public enum Direction {Back, Front, Right, Left};
    public Direction currentDirection = Direction.Back;

    public override void _Ready()
    {
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
        enableInputTimer = GetNode<Timer>("InputTimer");

        // Connecting timers for function loops
        reloadTimer.Connect("timeout", this, "AmmoReload");
        enableInputTimer.Connect("timeout", this, "EnableInput");
        ammoTimer.Connect("timeout", this, "AmmoShoot");
        muzzleFlashTimer.Connect("timeout", this, "MuzzleAnimation");

        // Setting initial player animation and speed
        SetAnimation("Idle_Back");
        speed = walkSpeed;
        camera.Zoom = new Vector2(0.71f, 0.71f);

        extraAmmoRounds = maxAmmoRounds * 23;
        currentAmmoRounds = maxAmmoRounds;
        money = 86587;
    }

    public override void _Process(float delta)
    {
        if (!inputEnabled) 
            return;

        velocity = GetMovementInput();

        // Updating player position
        Position += velocity * delta;

        // Ensures correct depth visibility based on scale
        ZIndex = Mathf.RoundToInt(Scale.x * 100);

        WeaponEquip();
        ClampPlayer();
        AddAmmoRounds();
        AddMoney();

        if (Input.IsActionJustPressed("quit"))
            GetTree().Quit();
    }
    
    private Vector2 GetMovementInput()
    {
        velocity = Vector2.Zero;
        if (!isShooting && !isReloading)
        {
            if (Input.IsActionPressed("left"))
            {
                currentDirection = Direction.Left;
                velocity.x = -1;
                HorizontalAnimation();
            }
            else if (Input.IsActionPressed("right"))
            {
                currentDirection = Direction.Right;
                velocity.x = 1;
                HorizontalAnimation();
            }
            else if (Input.IsActionPressed("forward"))
            {
                currentDirection = sceneTransitioning ? Direction.Front : Direction.Back;
                isZooming = true;
                VerticalAnimation();
            }
            else if (Input.IsActionPressed("backwards"))
            {
                currentDirection = sceneTransitioning ? Direction.Back : Direction.Front;
                isZooming = true;
                VerticalAnimation();
            }
        }

        SetIdleAnimation();

        if (velocity.Length() > 0)
            velocity = velocity.Normalized() * speed;

        return velocity; 
    }

    private void WeaponEquip()
    {
        if (!isReloading) 
        {
            if ((Input.IsActionPressed("equip") || Input.IsActionJustPressed("cycleWeapons")) && !weaponEquipped)
            {
                weaponEquipped = true;
                iconName = "ak47";
                ammoVisible = true;
                EmitSignal(nameof(WeaponChanged), iconName, ammoVisible);
            }
            else if ((Input.IsActionPressed("unequip") || Input.IsActionJustPressed("cycleWeapons")) && weaponEquipped)
            {
                weaponEquipped = false;
                weaponAimed = false;
                iconName = "fist";
                ammoVisible = false;
                EmitSignal(nameof(WeaponChanged), iconName, ammoVisible);
            }           
        }

        SetIdleAnimation();
        WeaponAim();
        WeaponReload();
    }

    private void WeaponAim()
    {
        if (Input.IsActionJustPressed("aim") && weaponEquipped)
            weaponAimed = !weaponAimed;

        WeaponShoot();
    }

    private void WeaponShoot()
    {
        if (Input.IsActionJustPressed("shoot") && weaponAimed && !isReloading && currentAmmoRounds > 0)
            StartShooting();

        if (Input.IsActionJustReleased("shoot") && isShooting)
            StopShooting();
    }

    private void WeaponReload()
    {
        if (Input.IsActionPressed("reload") && weaponEquipped && !isReloading && currentAmmoRounds != maxAmmoRounds)
            StartReload();
    }

    private void AddAmmoRounds()
    {
        if (Input.IsActionPressed("addAmmo") && weaponEquipped)
            extraAmmoRounds += 200;
    }

    public void AddMoney()
    {
        if (Input.IsActionPressed("addMoney"))
            money += 200;
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
        if (currentAmmoRounds > 0)
        {
            currentAmmoRounds -= 1; 

            if (currentAmmoRounds == 0)
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
        }
    }
    
    private void AmmoReload()
    {
        isReloading = false;
        weaponAimed = true;
        
        // Calculate the amount of ammo required for reload:
        // Choose the minimum value between the actual ammo needed to reach max ammo rounds and the extra ammo rounds left in reserve.
        // If current ammo is 45 and the max current ammo is 50 that means we need 5 ammmo rounds from the reserve.
        // If the extra ammo rounds value is less than 5 we will add this value to our current ammo value, otherwise we will add 5.
        ammoNeeded = maxAmmoRounds - currentAmmoRounds;
        ammoToLoad = Math.Min(ammoNeeded, extraAmmoRounds);

        currentAmmoRounds += ammoToLoad;
        extraAmmoRounds -= ammoToLoad;
        
        // Continue shooting if left click is still pressed
        if (Input.IsActionPressed("shoot"))
            StartShooting();
    }

    private void MuzzleAnimation()
    {
        currentMuzzle.Visible = !currentMuzzle.Visible;
        muzzleFlashTimer.Start(); // Flicker effect
    }

    private void EnableInput()
    {
        inputEnabled = true;
    }

    private void HorizontalAnimation()
    {
        bool running = Input.IsActionPressed("run") && !weaponAimed;
        bool facingRight = currentDirection == Direction.Right;
        speed = running ? runSpeed : walkSpeed;

        string moveType = running ? "Run" : "Walk";
        string side = facingRight ? "_Right" : "_Left";
        string rifle = weaponEquipped ? "_Rifle" : "";
        string aim = weaponAimed ? "_Aim" : "";

        if (Position.x > -maxPositionX && Position.x < maxPositionX)
            SetAnimation($"{moveType}{side}{rifle}{aim}");
    }

    public void VerticalAnimation()
    {
        bool running = Input.IsActionPressed("run") && !weaponAimed;
        bool facingFront = currentDirection == Direction.Front;
        float zoomStep = running ? 0.012f : 0.006f;
        camera.Zoom += new Vector2(zoomStep * (facingFront ? 1 : -1), zoomStep * (facingFront ? 1 : -1));

        string moveType = running ? "Run" : "Walk";
        string direction = facingFront ? "_Front" : "_Back";
        string rifle = weaponEquipped ? "_Rifle" : "";
        string aim = weaponAimed ? "_Aim" : "";

        if (camera.Zoom > new Vector2(0.5f, 0.5f))
            SetAnimation($"{moveType}{direction}{rifle}{aim}");

        if (camera.Zoom > new Vector2(0.99f, 0.99f) && !sceneTransitioning)
        {
            // Handle player transition from GameManager
            GameManager.Instance.PlayerSceneTransition();
        }
    }

    public void SetIdleAnimation()
    {
        // Checking if player has stopped moving vertically
        if (Input.IsActionJustReleased("forward") || Input.IsActionJustReleased("backwards"))
        {
            isZooming = false;
            sceneTransitioning = false;           
        }

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

    public void SetAnimation(string animationName)
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

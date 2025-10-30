using Godot;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

public class Player : KinematicBody2D
{
    [Export]
    public int Speed = 80;
    public Node2D background;
    public Vector2 screenSize;
    public AnimatedSprite animatedSprite;
    public Camera2D camera;
    public float maxPositionX = 0;
    public bool weaponEquipped = false;
    public string[] playerDirections = { "back", "front", "right", "left" };
    public string currentDirection;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Engine.TargetFps = 30;

        background = GetNode<Node2D>("../ParallaxBackground/ParallaxLayer/BackgroundPivot");
        screenSize = GetViewportRect().Size;

        // Each background sprite has a width of 320px. We have 5 sprites in total so 1600px of width.
        // In order to set the boundaries of where the player can move we need to first subtract the actual viewport width 
        // and then divide by 2 because the player is placed in the center of the screen
        maxPositionX = (1600 - screenSize.x) / 2; // For 640px we get 480px - Works for any viewport width

        camera = GetNode<Camera2D>("Camera2D");
        animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

        // Setting the current direction variable to "back"
        currentDirection = playerDirections[0];
        animatedSprite.Animation = "CJ_Idle_Back";
    }
    

    public void Rifle_Method()
    {
        // Weapon equip
        if (Input.IsActionPressed("rifle") && !weaponEquipped)
        {
            weaponEquipped = true;
            if (currentDirection == "back")
            {
                animatedSprite.Animation = "CJ_Idle_Rifle_Back";
            }
            else if (currentDirection == "front")
            {
                animatedSprite.Animation = "CJ_Idle_Rifle_Front";
            }
            else if (currentDirection == "right")
            {
                animatedSprite.FlipH = false;
                animatedSprite.Animation = "CJ_Idle_Rifle_Side";
            }
            else
            {
                animatedSprite.FlipH = false;
                animatedSprite.Animation = "CJ_Idle_Rifle_Side_Left";
            }
        }
        if (Input.IsActionPressed("unequip") && weaponEquipped)
        {
            weaponEquipped = false;
            if (currentDirection == "back")
            {
                animatedSprite.Animation = "CJ_Idle_Back";
            }
            else if (currentDirection == "front")
            {
                animatedSprite.Animation = "CJ_Idle_Front";
            }
            else if (currentDirection == "right")
            {
                animatedSprite.FlipH = false;
                animatedSprite.Animation = "CJ_Idle_Side";
            }
            else
            {
                animatedSprite.FlipH = true;
                animatedSprite.Animation = "CJ_Idle_Side";
            }
        }
    }

    public Vector2 GetInput()
    {
        var velocity = Vector2.Zero;

        // Keyboard movement logic
        if (Input.IsActionPressed("left"))
        {
            currentDirection = playerDirections[3];
            velocity.x -= 1;
            animatedSprite.FlipH = true;
            if (Input.IsActionPressed("run"))
            {
                Speed = 160;
                if (weaponEquipped)
                {
                    animatedSprite.FlipH = false;
                    animatedSprite.Animation = "CJ_Run_Side_Rifle_Left";
                }
                else
                {
                   animatedSprite.Animation = "CJ_Run_Side";  
                }       
            }
            else
            {
                Speed = 80;
                if (weaponEquipped)
                {
                    animatedSprite.FlipH = false;
                    animatedSprite.Animation = "CJ_Walk_Side_Rifle_Left";
                }
                else
                {
                    animatedSprite.Animation = "CJ_Walk_Side";                    
                }
            }
        }
        else if (Input.IsActionPressed("right"))
        {
            currentDirection = playerDirections[2];
            velocity.x += 1;
            animatedSprite.FlipH = false;
            if (Input.IsActionPressed("run"))
            {
                Speed = 160;
                if (weaponEquipped)
                {
                    animatedSprite.FlipH = false;
                    animatedSprite.Animation = "CJ_Run_Side_Rifle";
                }
                else
                {
                   animatedSprite.Animation = "CJ_Run_Side";  
                }   
            }
            else
            {
                Speed = 80;
                if (weaponEquipped)
                {
                    animatedSprite.Animation = "CJ_Walk_Side_Rifle";
                }
                else
                {
                    animatedSprite.Animation = "CJ_Walk_Side";                    
                }
            }
        }
        else if (Input.IsActionPressed("up"))
        {
            currentDirection = playerDirections[0];
            if (Input.IsActionPressed("run"))
            {
                camera.Zoom -= new Vector2(.012f, .012f);
                if (weaponEquipped)
                {
                    animatedSprite.Animation = "CJ_Run_Back_Rifle";
                }
                else
                {
                   animatedSprite.Animation = "CJ_Run_Back";  
                }   
            }
            else
            {
                camera.Zoom -= new Vector2(.006f, .006f);
                if (weaponEquipped)
                {
                    animatedSprite.Animation = "CJ_Walk_Back_Rifle";
                }
                else
                {
                    animatedSprite.Animation = "CJ_Walk_Back";                    
                }
            }

        }
        else if (Input.IsActionPressed("down"))
        {
            currentDirection = playerDirections[1];
            if (Input.IsActionPressed("run"))
            {
                camera.Zoom += new Vector2(.012f, .012f);
                if (weaponEquipped)
                {
                    animatedSprite.Animation = "CJ_Run_Front_Rifle";
                }
                else
                {
                   animatedSprite.Animation = "CJ_Run_Front";  
                }    
            }    
            else
            {
                camera.Zoom += new Vector2(.006f, .006f);
                if (weaponEquipped)
                {
                    animatedSprite.Animation = "CJ_Walk_Front_Rifle";
                }
                else
                {
                    animatedSprite.Animation = "CJ_Walk_Front";                    
                }
            }
        }




        // Returning to idle state after walking
        if (Input.IsActionJustReleased("left"))
        {
            if (weaponEquipped)
            {
                animatedSprite.FlipH = false;
                animatedSprite.Animation = "CJ_Idle_Rifle_Side_Left";
            }
            else
            {
                animatedSprite.Animation = "CJ_Idle_Side";
            }
        }
        else if (Input.IsActionJustReleased("right"))
        {
            if (weaponEquipped)
            {
                animatedSprite.Animation = "CJ_Idle_Rifle_Side";
            }
            else
            {
                animatedSprite.Animation = "CJ_Idle_Side";
            }
        } 
        else if (Input.IsActionJustReleased("up") && velocity.x == 0)
        {
            if (weaponEquipped)
            {
                animatedSprite.Animation = "CJ_Idle_Rifle_Back";
            }
            else
            {
                animatedSprite.Animation = "CJ_Idle_Back";
            }
        }
        else if (Input.IsActionJustReleased("down") && velocity.x == 0)
        {
            if (weaponEquipped)
            {
                animatedSprite.Animation = "CJ_Idle_Rifle_Front";
            }
            else
            {
                animatedSprite.Animation = "CJ_Idle_Front";
            }
        }

        return velocity;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        var velocity = GetInput();
        Rifle_Method();
        
        // Update player position
        if (velocity.Length() > 0)
        {
            velocity = velocity.Normalized() * Speed;
        }
        Position += velocity * delta;

        // Clamp player position and camera zoom
        Position = new Vector2(Mathf.Clamp(Position.x, -maxPositionX, maxPositionX), Mathf.Clamp(Position.y, -screenSize.y, screenSize.y));
        camera.Zoom = new Vector2(Mathf.Clamp(camera.Zoom.x, .5f, 1), Mathf.Clamp(camera.Zoom.y, .5f, 1));

        // Keep the player's position the same relative to the camera
        Scale = new Vector2(camera.Zoom.x, camera.Zoom.y);
        
    }
}

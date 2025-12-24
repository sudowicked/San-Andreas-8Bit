using Godot;
using System;

public class HUD : CanvasLayer
{
    private Label ammoLabel, hoursLabel, minutesLabel, moneyLabel;
    private Player player;
    private Timer minutesTimer;
    private int hours = 23, minutes = 55;
    private TextureRect weaponIcon;

    public override void _Ready()
    {
        ammoLabel = GetNode<Label>("HUDRoot/TopRightControl/WeaponIcon/AmmoControl/AmmoLabel");
        hoursLabel = GetNode<Label>("HUDRoot/TopRightControl/TimeControl/HoursLabel");
        minutesLabel = GetNode<Label>("HUDRoot/TopRightControl/TimeControl/MinutesLabel");
        moneyLabel = GetNode<Label>("HUDRoot/TopRightControl/MoneyControl/MoneyLabel");
        weaponIcon = GetNode<TextureRect>("HUDRoot/TopRightControl/WeaponIcon");
        player = GameManager.Instance.player;

        minutesTimer = GetNode<Timer>("HUDRoot/TopRightControl/TimeControl/MinutesLabel/MinutesTimer");
        minutesTimer.Connect("timeout", this, "MinutesFunction");

        // Connect to player WeaponChanged signal
        player.Connect(nameof(Player.WeaponChanged), this, nameof(OnWeaponChanged));
    }

    public override void _Process(float delta)
    {
        ammoLabel.Text = player.extraAmmoRounds.ToString() + "-" + player.currentAmmoRounds.ToString();
        hoursLabel.Text = hours.ToString().PadZeros(2);
        minutesLabel.Text = minutes.ToString().PadZeros(2);
        moneyLabel.Text = "$" + player.money.ToString().PadZeros(8);
    }

    private void OnWeaponChanged(string iconName, bool ammoVisible)
    {
        weaponIcon.Texture = GD.Load<Texture>($"res://UI/HUD/{iconName}_Pixelated.png");
        ammoLabel.Visible = ammoVisible;
    }

    private void MinutesFunction()
    {
        if (minutes == 59)
        {
            minutes = -1;
            HoursFunction();           
        }
        minutes ++;
    }

    private void HoursFunction()
    {
        if (hours == 23)
            hours = -1;
        hours ++;
    }


}

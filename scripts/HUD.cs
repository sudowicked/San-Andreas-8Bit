using Godot;
using System;

public class HUD : CanvasLayer
{
    private Label ammoLabel, hoursLabel, minutesLabel;
    private Player player;
    private Timer minutesTimer;
    private int hours = 23, minutes = 55;

    public override void _Ready()
    {
        ammoLabel = GetNode<Label>("HUDRoot/TopRightControl/WeaponTexture/AmmoControl/AmmoLabel");
        minutesLabel = GetNode<Label>("HUDRoot/TopRightControl/TimeControl/MinutesLabel");
        hoursLabel = GetNode<Label>("HUDRoot/TopRightControl/TimeControl/HoursLabel");
        player = GameManager.Instance.Player;

        minutesTimer = GetNode<Timer>("HUDRoot/TopRightControl/TimeControl/MinutesLabel/MinutesTimer");
        minutesTimer.Connect("timeout", this, "MinutesFunction");
    }

    public override void _Process(float delta)
    {
        ammoLabel.Text = player.extraAmmoRounds.ToString() + "-" + player.currentAmmoRounds.ToString();
        hoursLabel.Text = hours.ToString().PadZeros(2);
        minutesLabel.Text = minutes.ToString().PadZeros(2);
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

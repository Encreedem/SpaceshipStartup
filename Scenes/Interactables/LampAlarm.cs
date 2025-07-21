using Godot;
using System;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class LampAlarm : Node3D
	{
		#region Components & Nodes

		private AnimationPlayer animationPlayer;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		}

		#endregion

		#region Public Methods

		public void BlinkGreen()
		{
			StopCurrentAnimation();
			animationPlayer.Play("BlinkGreen");
		}

		public void PlayGreenAlarm()
		{
			StopCurrentAnimation();
			animationPlayer.Play("GreenAlarm");
		}

		public void PlayRedAlarm()
		{
			StopCurrentAnimation();
			animationPlayer.Play("RedAlarm");
		}

		public void PlayShortRedAlarm(bool stayOn = false)
		{
			StopCurrentAnimation();
			animationPlayer.Play(stayOn ? "ShortRedAlarmPersist" : "ShortRedAlarm");
		}

		/// <summary>
		/// Stops all alarms and displays a dark lamp.
		/// </summary>
		public void TurnOff()
		{
			animationPlayer.Play("RESET");
		}

		public void SetLampRed()
		{
			animationPlayer.Play("LampRed");
		}

		public void SetLampGreen()
		{
			animationPlayer.Play("LampGreen");
		}

		#endregion

		#region Private Methods

		private void StopCurrentAnimation()
		{
			if (animationPlayer.IsPlaying())
			{
				animationPlayer.Play("RESET");
			}
		}

		#endregion

	}
}
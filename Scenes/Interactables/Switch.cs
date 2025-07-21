using Godot;
using System;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class Switch : Node3D
	{
		#region Signals

		[Signal] public delegate void ToggledEventHandler(bool switchedOn);

		#endregion

		#region Components & Nodes

		private Sprite3D switchOff;
		private Sprite3D switchOn;
		private LampAlarm lamp;
		private AudioStreamPlayer3D flickAudio;

		#endregion

		#region Public Properties

		public bool SwitchedOn { get; private set; } = false;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			switchOff = GetNode<Sprite3D>("Switch OFF");
			switchOn = GetNode<Sprite3D>("Switch ON");
			lamp = GetNode<LampAlarm>("Lamp");
			flickAudio = GetNode<AudioStreamPlayer3D>("FlickAudio");
		}

		#endregion

		#region Public Methods

		public void PlayGreenAlarm()
		{
			lamp.PlayGreenAlarm();
		}

		public void PlayRedAlarm()
		{
			lamp.PlayRedAlarm();
		}

		public void PlayShortRedAlarm()
		{
			lamp.PlayShortRedAlarm();
		}

		/// <summary>
		/// Stops all alarms and displays a dark lamp.
		/// </summary>
		public void TurnOff()
		{
			lamp.TurnOff();
		}

		public void SetLampGreen()
		{
			lamp.SetLampGreen();
		}

		#endregion

		#region Events

		public void OnClicked()
		{
			SwitchedOn = !SwitchedOn;
			switchOff.Visible = !SwitchedOn;
			switchOn.Visible = SwitchedOn;

			flickAudio.Play();

			EmitSignal(SignalName.Toggled, SwitchedOn);
		}

		#endregion
	}
}
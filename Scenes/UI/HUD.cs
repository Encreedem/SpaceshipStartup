using Godot;
using System;

namespace SpaceshipStartup.Scenes.UI
{
	public partial class HUD : Control
	{
		#region Components & Nodes

		private Options options;
		private AudioStreamPlayer interactAudio;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			options = GetTree().Root.GetNode<Options>("Options");
			interactAudio = GetNode<AudioStreamPlayer>("InteractSound");
		}

		#endregion

		#region Events

		public void OnSettingsGuiInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton button && button.ButtonIndex == MouseButton.Left && button.Pressed)
			{
				options.Show();
				interactAudio.Play();
			}
		}

		#endregion
	}
}
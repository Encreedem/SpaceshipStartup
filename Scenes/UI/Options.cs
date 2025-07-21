using Godot;
using System;

namespace SpaceshipStartup.Scenes.UI
{
	public partial class Options : Control
	{
		#region Components & Nodes

		private VolumeSlider volumeSlider;
		private AudioStreamPlayer interactAudio;
		private TextureRect volumeIcon;
		private TextureRect volumeMutedIcon;

		#endregion

		#region Private Variables

		private double unmutedVolume;
		private bool suppressAudio = true;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			volumeSlider = GetNode<VolumeSlider>("Background/VolumeSlider");
			interactAudio = GetNode<AudioStreamPlayer>("InteractSound");
			volumeIcon = GetNode<TextureRect>("Background/VolumeIcon");
			volumeMutedIcon = GetNode<TextureRect>("Background/VolumeMutedIcon");

			unmutedVolume = volumeSlider.Value;
		}

		#endregion

		#region Events

		public void OnVolumeSliderValueChanged(float value)
		{
			if (value > 0)
			{
				unmutedVolume = value;

				volumeIcon?.Show();
				volumeMutedIcon?.Hide();
			}
			else
			{
				volumeIcon?.Hide();
				volumeMutedIcon?.Show();
			}

			if (!suppressAudio)
			{
				interactAudio.Play();
			}
		}

		public void OnVolumeIconGuiInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton button && button.ButtonIndex == MouseButton.Left && button.Pressed == true)
			{
				if (volumeSlider.Value > 0)
				{
					volumeSlider.Value = 0;
				}
				else if (unmutedVolume > 0)
				{
					volumeSlider.Value = unmutedVolume;
				}
				else
				{
					volumeSlider.Value = 0.5;
				}
			}
		}

		public void OnConfirmPressed()
		{
			interactAudio.Play();
			Hide();
		}

		public void OnSuppressAudioTimerTimeout()
		{
			suppressAudio = false;
		}

		#endregion
	}
}
using Godot;
using System;

public partial class VolumeSlider : HSlider
{
	#region Exports

	[Export] public string BusName { get; set; }

	#endregion

	#region Private Variables

	private int busIndex;

	#endregion

	#region Method Overrides

	public override void _Ready()
	{
		busIndex = AudioServer.GetBusIndex(BusName);
		Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(busIndex));
	}

	#endregion

	#region Events

	public void OnValueChanged(float value)
	{
		AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(value));
	}

	#endregion
}

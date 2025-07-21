using Godot;
using SpaceshipStartup.Scenes.Levels;
using System;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class LifeSupportFilter : Node3D
	{
		#region Events

		public void OnFilterInteractableClicked()
		{
			Level.Instance.SetPickedUpFilter();
			GetNode<AnimationPlayer>("AnimationPlayer").Play("PickUpFilter");
		}

		#endregion
	}
}
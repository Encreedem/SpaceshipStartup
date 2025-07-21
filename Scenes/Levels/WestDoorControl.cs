using Godot;
using SpaceshipStartup.Scenes.Levels;
using System;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class WestDoorControl : Node3D
	{
		#region Exports

		[Export] private Door door;

		#endregion

		#region Components & Nodes

		private AnimationPlayer animationPlayer;
		private Interactable3D interactable;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			interactable = GetNode<Interactable3D>("Interactable3D");
		}

		#endregion

		#region Public Methods

		public void SetDoorControlInteractable()
		{
			interactable.Interaction = Interaction.Interact;
		}

		#endregion

		#region Events

		public void OnClicked()
		{
			if (!Level.Instance.IsKeycardObtained)
				return;

			animationPlayer.Play("SwipeKeycard");
			interactable.Hide();
		}

		public void OnAnimationPlayerAnimationFinished(StringName animName)
		{
			if (animName == "SwipeKeycard")
			{
				door.Locked = false;
				animationPlayer.Play("DropKeycard");
			}
		}

		#endregion
	}
}
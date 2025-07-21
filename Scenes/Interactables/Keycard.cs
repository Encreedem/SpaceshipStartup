using Godot;
using SpaceshipStartup.Scenes.Levels;
using System;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class Keycard : Node3D
	{
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

		#region Events

		public void OnClicked()
		{
			animationPlayer.Play("PickUp");
			interactable.Hide();
			Level.Instance.SetPickedUpKeycard();
		}

		#endregion
	}
}
using Godot;
using System;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class Chest : Node3D
	{
		#region Components & Nodes

		private Interactable3D interactable;
		private AnimationPlayer animationPlayer;
		private OmniLight3D light;
		private Node3D contained;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			interactable = GetNode<Interactable3D>("Interactable3D");
			animationPlayer = GetNode<AnimationPlayer>("Prop_Chest2/AnimationPlayer");
			light = GetNode<OmniLight3D>("Prop_Chest2/GreenLight");
			contained = GetNode<Node3D>("Contained");
		}

		#endregion

		#region Events

		public void OnClicked()
		{
			interactable.Hide();
			animationPlayer.Play("Chest_Open");
			light.Hide();
			contained.Show();
		}

		#endregion
	}
}
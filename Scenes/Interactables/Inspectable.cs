using Godot;
using SpaceshipStartup.Scenes.Levels;
using System;

namespace SpaceshipStartup.Scenes.Interactables
{
	/* A puzzle or rack or something the player can interact with.
	 * Interaction zooms in to make more minute adjustments with the underlying elements.
	 * 
	 * This Inspectable handles the zooming in and setting the current camera.
	 * The Level handles returning, setting the original camera and will call this class' Exit().
	 */
	public partial class Inspectable : Node3D
	{
		#region Signals

		[Signal] public delegate void ExitedEventHandler();

		#endregion

		#region Components & Nodes

		private Camera3D zoomCamera;
		private Interactable3D zoomInteract;

		#endregion

		#region Public Properties

		public bool CanInspect = true;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			zoomCamera = GetNode<Camera3D>("Camera3D");
			zoomInteract = GetNode<Interactable3D>("ZoomInteract");
		}

		#endregion

		#region Public Methods

		public void Exit()
		{
			zoomInteract.Show();
		}

		public void DisableInspection()
		{
			zoomInteract.Show();
			zoomInteract.Interaction = Interaction.None;
			CanInspect = false;
		}

		#endregion

		#region Events

		public void OnZoomInteractClicked()
		{
			if (!CanInspect)
				return;

			Level.Instance.SetCurrentInspectable(this);
			zoomCamera.Current = true;
			zoomInteract.Hide();
		}

		#endregion
	}
}
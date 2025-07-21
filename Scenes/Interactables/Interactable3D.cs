using Godot;
using SpaceshipStartup.Scenes.Levels;
using System;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class Interactable3D : Area3D
	{
		#region Signals

		[Signal] public delegate void ClickedEventHandler();

		#endregion

		#region Exports

		[Export] public Interaction Interaction { get; set; }

		#endregion

		#region Public Properties

		public MapNode InteractableFromMapNode { get; set; }

		#endregion

		#region Private Methods

		private bool PlayerCanInteract() => Player.Instance.CurrentMapNode == InteractableFromMapNode;

		#endregion

		#region Events

		public void OnInputEvent(Node camera, InputEvent @event, Vector3 eventPosition, Vector3 normal, int shapeIdx)
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonMask == MouseButtonMask.Left && mouseButton.Pressed && PlayerCanInteract())
			{
				EmitSignal(SignalName.Clicked);
			}
		}

		public void OnMouseEntered()
		{
			Level.Instance.SetObservedInteractable(this);
		}

		public void OnMouseExited()
		{
			Level.Instance.RemoveObservedInteractable(this);
		}

		#endregion
	}
}
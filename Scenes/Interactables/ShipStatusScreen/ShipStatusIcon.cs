using Godot;
using System;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class ShipStatusIcon : Node3D
	{
		#region Enumns

		public enum Status { Check, Exclamation, X }

		#endregion

		#region Components & Nodes

		private Sprite3D check;
		private Sprite3D exclamation;
		private Sprite3D x;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			check = GetNode<Sprite3D>("Check");
			exclamation = GetNode<Sprite3D>("Exclamation");
			x = GetNode<Sprite3D>("X");
		}

		#endregion

		#region Public Methods

		public void SetStatus(Status status)
		{
			switch (status)
			{
				case Status.Check:
					SetStatusCheck();
					break;
				case Status.Exclamation:
					SetStatusExclamation();
					break;
				case Status.X:
					SetStatusX();
					break;
			}
		}

		public void SetStatusCheck()
		{
			check.Show();
			exclamation.Hide();
			x.Hide();
		}

		public void SetStatusExclamation()
		{
			check.Hide();
			exclamation.Show();
			x.Hide();
		}

		public void SetStatusX()
		{
			check.Hide();
			exclamation.Hide();
			x.Show();
		}

		#endregion
	}
}
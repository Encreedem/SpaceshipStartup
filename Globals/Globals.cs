using Godot;
using System;

namespace SpaceshipStartup
{
	[Flags]
	public enum Cardinal
	{
		None = 0,
		North = 1 << 0,
		East = 1 << 1,
		South = 1 << 2,
		West = 1 << 3,
	}

	public enum Interaction
	{
		None,
		Inspect,
		Interact,
		Unknown,
		MoveForward,
		GoBack,
		TurnLeft,
		TurnRight,
	}

	public static class CardinalUtils
	{
		#region Conversions

		/// <summary>
		/// Returns the Vector3 corresponding to a given cardinal
		/// </summary>
		public static Vector3 CardinalVector(Cardinal cardinal) => cardinal switch
		{
			Cardinal.North => Vector3.Forward,
			Cardinal.East => Vector3.Right,
			Cardinal.South => Vector3.Back,
			Cardinal.West => Vector3.Left,
			_ => throw new ArgumentOutOfRangeException(nameof(cardinal), $"Not expected cardinal value: {cardinal}"),
		};

		/// <summary>
		/// Returns the Vector3I corresponding to a given cardinal
		/// </summary>
		public static Vector3I CardinalVectorInt(Cardinal cardinal) => cardinal switch
		{
			Cardinal.North => Vector3I.Forward,
			Cardinal.East => Vector3I.Right,
			Cardinal.South => Vector3I.Back,
			Cardinal.West => Vector3I.Left,
			_ => throw new ArgumentOutOfRangeException(nameof(cardinal), $"Not expected cardinal value: {cardinal}"),
		};

		/// <summary>
		/// Returns the <see cref="Cardinal"/>(s) 90° clockwise from the parameter.
		/// </summary>
		public static Cardinal Clockwise(Cardinal cardinal) => (Cardinal)((((uint)cardinal) << 1) % (((uint)Cardinal.West << 1) - 1));

		/// <summary>
		/// Returns the <see cref="Cardinal"/>(s) 90° counterclockwise from the parameter.
		/// </summary>
		public static Cardinal Counterclockwise(Cardinal cardinal) =>
			(Cardinal)(((uint)cardinal >> 1) + (cardinal.HasFlag(Cardinal.North) ? (uint)Cardinal.West : 0));

		public static Cardinal Opposite(Cardinal cardinal) => Clockwise(Clockwise(cardinal));

		/// <summary>
		/// Returns the Cardinal something is facing corresponding to their Y global rotations in degrees.
		/// </summary>
		public static Cardinal RotationCardinal(float yRotation) => Mathf.RoundToInt(yRotation) switch
		{
			0 => Cardinal.North,
			90 => Cardinal.West,
			-180 => Cardinal.South,
			180 => Cardinal.South,
			-90 => Cardinal.East,
			_ => throw new ArgumentOutOfRangeException($"Unexpected yRotation {yRotation}"),
		};
		#endregion
	}

	public static class DegreeUtils
	{
		/// <summary>
		/// Converts degrees to radians.
		/// </summary>
		public static float DegToRad(float deg)
		{
			return deg * (MathF.PI / 180);
		}
	}

	public partial class Globals : GodotObject
	{
	}
}
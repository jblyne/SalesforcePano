using UnityEngine;
using System.Collections;

public static class TransformExtension {

	//Returns the rotated Vector3 using a Quaterion
	public static Vector3 RotateAroundPivot( this Vector3 Point, Vector3 Pivot, Quaternion Angle ) {
		return Angle * ( Point - Pivot ) + Pivot;
	}
	//Returns the rotated Vector3 using Euler
	public static Vector3 RotateAroundPivot( this Vector3 Point, Vector3 Pivot, Vector3 Euler ) {
		return RotateAroundPivot( Point, Pivot, Quaternion.Euler( Euler ) );
	}
	//Rotates the Transform's position using a Quaterion
	public static void RotateAroundPivot( this Transform Me, Vector3 Pivot, Quaternion Angle ) {
		Me.position = Me.position.RotateAroundPivot( Pivot, Angle );
	}
	//Rotates the Transform's position using Euler
	public static void RotateAroundPivot( this Transform Me, Vector3 Pivot, Vector3 Euler ) {
		Me.position = Me.position.RotateAroundPivot( Pivot, Quaternion.Euler( Euler ) );
	}
}

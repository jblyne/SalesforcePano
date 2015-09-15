using UnityEngine;
using System;
using System.Collections;

// Simple class to handle the conversion of a string date to a number
// of different data types, and vice versa.

public static class DateFormat {
	
	private static string[] months = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};

	public static string WrittenDate( DateTime _arg ) {
		return _arg.Day.ToString() + " " + months[_arg.Month -1] + " " + _arg.Year.ToString();
	}

	public static int[] IntegerDate( string _arg ) {
		string[] dateTime = _arg.Split( ' ' );
		string[] date = dateTime[0].Split( ':' );
		int[] ret = new int[3];
		
		ret[0] = Convert.ToInt32( date[2] );
		ret[1] = Convert.ToInt32( date[1] );
		ret[2] = Convert.ToInt32( date[0] );

		return ret;
	}

	public static DateTime DateTime( string _arg ) {
		int[] date = IntegerDate( _arg );

		return new DateTime( date[2], date[1], date[0] );
	}
}
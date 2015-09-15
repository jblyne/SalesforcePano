package com.sherif.cardboard3d.bitmaphandler;

import android.content.Context;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.util.Log;

/**
 * Created by Sherif Salem on 01-Jul-15.
 * Will query the Proximity sensor on the device.
 */
public class ProximityChecker implements SensorEventListener{
	private SensorManager m_SensorManager;
	private Sensor m_Sensor;
	private Context m_Context;

	private float m_Distance;

	public float Distance() {
		return m_Distance;
	}

	public ProximityChecker(Context _context) {
		Log.d("Proximity checker", "Initialising...");
		m_Distance = -1.0f;
		m_Context = _context;
		m_SensorManager = (SensorManager)m_Context.getSystemService(Context.SENSOR_SERVICE);
		m_Sensor = m_SensorManager.getDefaultSensor(Sensor.TYPE_PROXIMITY);
		m_SensorManager.registerListener(this, m_Sensor, SensorManager.SENSOR_DELAY_GAME);
	}

	@Override
	public final void onAccuracyChanged(Sensor sensor, int accuracy) {
		// Do something here if sensor accuracy changes.
	}

	@Override
	public final void onSensorChanged(SensorEvent event) {
		Log.d("Proximity checker", "SensorChanged triggered. Value: " + event.values[0]);
		m_Distance = event.values[0];
		// Do something with this sensor data.
	}
}

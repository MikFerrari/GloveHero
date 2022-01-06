//+++ Import libraries +++//
#include "AK09918.h"
#include "ICM20600.h"
#include <Wire.h>
//+++ End Import libraries +++//


//+++ MACROs definition +++//
#define NELEMS(x)  (int) (sizeof(x) / sizeof((x)[0]))
//+++ End MACROs definition +++//


//+++ Define objects +++//
AK09918_err_type_t err;
AK09918 ak09918;
ICM20600 icm20600(true);
//+++ End Define ojects +++//


//+++ Define variables for IMU readings +++//
int32_t x, y, z;
int16_t acc_x, acc_y, acc_z;
int16_t gyro_x, gyro_y, gyro_z;
int32_t offset_x, offset_y, offset_z;
double roll, pitch, heading;

// Find the magnetic declination at your location: http://www.magnetic-declination.com/
double declination_shenzhen = -1.38;

//+++ End Define variables for IMU readings +++//


//+++ Define pins +++//

// Reading extensimeter voltage
int ext_thumb_pin = A1;
int ext_index_pin = A2;
int ext_middle_pin = A3;
int ext_ring_pin = A4;
int ext_little_pin = A5;

// Vibrator motor transistor
int motorPin = A6;

//+++ End Define pins +++//


//+++ Define variables to hold extensimeter voltages +++//
double ext_thumb_val = 0;
double ext_index_val = 0;
double ext_middle_val = 0;
double ext_ring_val = 0;
double ext_little_val = 0;
//+++ End Define variables to hold extensimeter voltages +++//


//+++ Define variables for signal processing [unsigned char (0-255) to save memory] +++//

const float fs = 30; // Desired approximate frequency
const float win_time = 0.26; // Desired window length
const unsigned char Nsamples = 8; // N samples in the window: ceil(win_time* fs) -> MUST BE HARDCODED
unsigned char counter = 0; // Counter to build windows
const unsigned char overlapFactor = 5; // Overlap percenct for shifting windows
const unsigned char Nshift = ceil(Nsamples/overlapFactor); // N samples to overlap
bool flag = false; // Flag to manage offset removal
const unsigned char NumberOfSignals = 14; // N measured signals
float offset[NumberOfSignals] = {0}; // Offset for each signal
const String classes[7] = {"P2","P3","P4","P5","F","S","N"}; // Possible classes to predict
String predicted_class = ""; // Predicted class
const unsigned char NumberOfSignalsKept = 8; // N sigmals used to predict
double data_new[NumberOfSignalsKept] = {0}; // Array to store new sensor reading
double data_window[Nsamples][NumberOfSignalsKept] = {0}; // Matrix to store a window of readings
double features[NumberOfSignalsKept] = {0}; // Array to store features extracted from a single window
double features_calib[NumberOfSignalsKept] = {0}; // Array to store features extracted from a single window (during calibration)

const float dt = 1; // Calibration window length [s]
const unsigned char Nsamples_calib = round(dt*fs); // N calibration samples
const unsigned char Ntrials = 5; // N trials for each finger
const unsigned char Ngestures = 4; // N gestures to calibrate
const unsigned char Nthresh = 5; // N thresholds to calibrate

double thresholds[Nthresh] = {683.1, 451.0,434.5, 400.4,	597.6}; // Tuned by default on Riccardo

//+++ End Define variables for signal processing +++//


//+++ Function definitions +++//

// IMU calibration
void calibrate(uint32_t timeout, int32_t *offsetx, int32_t *offsety, int32_t*offsetz)
{
  int32_t value_x_min = 0;
  int32_t value_x_max = 0;
  int32_t value_y_min = 0;
  int32_t value_y_max = 0;
  int32_t value_z_min = 0;
  int32_t value_z_max = 0;
  uint32_t timeStart = 0;
 
  ak09918.getData(&x, &y, &z);
 
  value_x_min = x;
  value_x_max = x;
  value_y_min = y;
  value_y_max = y;
  value_z_min = z;
  value_z_max = z;
  delay(100);

  timeStart = millis();
 
  while((millis() - timeStart) < timeout)
  {
	ak09918.getData(&x, &y, &z);
 
	/* Update x-Axis max/min value */
	if(value_x_min > x)
	{
	  value_x_min = x;
	  // Serial.print("Update value_x_min: ");
	  // Serial.println(value_x_min);
 
	} 
	else if(value_x_max < x)
	{
	  value_x_max = x;
	  // Serial.print("update value_x_max: ");
	  // Serial.println(value_x_max);
	}
 
	/* Update y-Axis max/min value */
	if(value_y_min > y)
	{
	  value_y_min = y;
	  // Serial.print("Update value_y_min: ");
	  // Serial.println(value_y_min);
 
	} 
	else if(value_y_max < y)
	{
	  value_y_max = y;
	  // Serial.print("update value_y_max: ");
	  // Serial.println(value_y_max);
	}
 
	/* Update z-Axis max/min value */
	if(value_z_min > z)
	{
	  value_z_min = z;
	  // Serial.print("Update value_z_min: ");
	  // Serial.println(value_z_min);
 
	} 
	else if(value_z_max < z)
	{
	  value_z_max = z;
	  // Serial.print("update value_z_max: ");
	  // Serial.println(value_z_max);
	}
 
	Serial.print(".");
	delay(100);
 
  }
 
  *offsetx = value_x_min + (value_x_max - value_x_min)/2;
  *offsety = value_y_min + (value_y_max - value_y_min)/2;
  *offsetz = value_z_min + (value_z_max - value_z_min)/2;
}

// IMU reading
void IMU_reading()
{
	// Get acceleration [mg]
	acc_x = icm20600.getAccelerationX();
	acc_y = icm20600.getAccelerationY();
	acc_z = icm20600.getAccelerationZ();

	// Get gyroscope values [dps]
	gyro_x = icm20600.getGyroscopeX();
	gyro_y = icm20600.getGyroscopeY();
	gyro_z = icm20600.getGyroscopeZ();

	// Get magnetometer data [uT]
	ak09918.getData(&x, &y, &z);
	x = x - offset_x;
	y = y - offset_y;
	z = z - offset_z;

	// Roll/pitch in radians
	roll = 57.3 * (atan2((float)acc_y, (float)acc_z));
	pitch = 57.3 * (atan2(-(float)acc_x, sqrt((float)acc_y * acc_y + (float)acc_z * acc_z)));

	// Heading
	double Xheading = x * cos(pitch) + y * sin(roll) * sin(pitch) + z * cos(roll) * sin(pitch);
	double Yheading = y * cos(roll) - z * sin(pitch);
	heading = 180 + 57.3 * atan2(Yheading, Xheading) + declination_shenzhen;
}

// Extensimeter reading
void ext_reading()
{
	ext_thumb_val = analogRead(ext_thumb_pin);
	ext_index_val = analogRead(ext_index_pin);
	ext_middle_val = analogRead(ext_middle_pin);
	ext_ring_val = analogRead(ext_ring_pin);
	ext_little_val = analogRead(ext_little_pin);
}

// Create array with a new sensor reading
void create_new_data()
{
	data_new[0] = acc_x;
	data_new[1] = acc_y;
	data_new[2] = acc_z;
	data_new[3] = ext_thumb_val;
	data_new[4] = ext_index_val;
	data_new[5] = ext_middle_val;
	data_new[6] = ext_ring_val;
	data_new[7] = ext_little_val;
}

// Remove offset from data (first value ever recorded at rest)
void remove_offset()
{
	if (flag == false && counter == 1)
	{
		for (int i = 0; i < NumberOfSignalsKept; i++)
		{
			offset[i] = data_window[1][i];
		}
		flag = true;
	}

	for (int i = 0; i < NumberOfSignalsKept; i++)
	{
		data_new[i] = data_new[i] - offset[i];
	}

	for (int i = 0; i < NumberOfSignalsKept; i++)
	{
		data_window[counter][i] = data_new[i];
	}
}

// Move sliding window by Nshift samples
void shift_window()
{
	for (int i = 0; i < Nshift; i++)
	{
		for (int j = 0; j < NumberOfSignalsKept; j++)
		{
			data_window[i][j] = 0;
		}
	}
	for (int i = Nshift; i < Nsamples; i++)
	{
		for (int j = 0; j < NumberOfSignalsKept; j++)
		{
			data_window[i][j] = data_window[i + Nshift][j];
		}
	}
	for (int i = Nsamples; i < Nsamples - Nshift; i--)
	{
		for (int j = 0; j < NumberOfSignalsKept; j++)
		{
			data_window[i][j] = 0;
		}
	}
}

// Compute array of features from a given window of recorded data [acquisition]
void extract_feature(double data_window[][NumberOfSignalsKept])
{
	for (int i = 0; i < NumberOfSignalsKept; i++)
	{
		double data_window_column[Nsamples] = {0};
		int n = 0;
		for (int j = i; j < Nsamples * NumberOfSignalsKept; j += NumberOfSignalsKept)
		{
			data_window_column[n++] = data_window[i][j];
		}

		if (i <= 2)
		{
			features[i] = mean(data_window_column,Nsamples);
		}
		else
		{
			features[i] = min_generic(data_window_column);
		}
	}
}

// Compute array of features from a given window of recorded data [calibration]
void extract_feature_calib(double data_window[][NumberOfSignalsKept])
{
	for (int i = 0; i < NumberOfSignalsKept; i++)
	{
		double data_window_column[Nsamples] = { 0 };
		int n = 0;
		for (int j = i; j < Nsamples * NumberOfSignalsKept; j += NumberOfSignalsKept)
		{
			data_window_column[n++] = data_window[i][j];
		}

		if (i <= 2)
		{
			features_calib[i] = mean(data_window_column, Nsamples);
		}
		else
		{
			features_calib[i] = min_generic(data_window_column);
		}
	}
}

// Compute min value from n-dimension array
double min_generic(double data_array[])
{
  double min_val = data_array[0];
  for(int i = 0; i < NELEMS(data_array); i++)
  {
	if (data_array[i] < min_val)
	{
	  min_val = data_array[i];
	}
  }
  return min_val;
}

// Compute max value from n-dimension array
double max_generic(double data_array[])
{
  double max_val = data_array[0];
  for(int i = 0; i < NELEMS(data_array); i++)
  {
	if (data_array[i] > max_val)
	{
	  max_val = data_array[i];
	}
  }
  return max_val; 
}

// Compute mean value from n-dimension array
double mean(double data_array[], int N)
{
	double avg = 0;
	for (int i = 0; i < N; i++)
	{
		avg += data_array[i];
	}
  return avg/N;
}

// Perform interactive calibration of the threshold for each finger
void calibrate_thresholds()
{
	for (int k = 0; k < Ngestures; k++)
	{
		String msg = String(k+2);
		Serial.println(msg);
		bool wait = true;
		while (wait)
		{
			if (!Serial.available() || Serial.read() == k+2)
			{
				continue;
			}
			Serial.read(); // Here Unity sends an "A"
			wait = false;
		}

		double recorded_features[Ntrials][NumberOfSignalsKept] = {0};
		for (int j = 0; j < Ntrials; j++)
		{
			double data_calib[Nsamples_calib][NumberOfSignalsKept] = {0};
			for (int i = 0; i < Nsamples_calib; i++)
			{
				IMU_reading();
				ext_reading();
				create_new_data();
				for (int v = 0; v < NumberOfSignalsKept; v++)
				{
					data_calib[i][v] = data_new[v];
				}
			}
			extract_feature_calib(data_calib);
			for (int i = 0; i < NumberOfSignalsKept; i++)
			{
				recorded_features[j][i] = features_calib[i];
			}

			Serial.println(String(j));
			bool wait = true;
			if (j < Ntrials - 1)
			{
				while (wait)
				{
					if (!Serial.available() || Serial.read() == j)
					{
						continue;
					}
					Serial.read(); // Here Unity sends a "N"
					wait = false;
				}
			}
		}

		for (int i = 0; i < NumberOfSignalsKept; i++)
		{
			double features_array[Ntrials] = {0};
			int n = 0;
			for (int j = i; j < Ntrials * NumberOfSignalsKept; j += NumberOfSignalsKept)
			{
				features_array[n++] = recorded_features[i][j];
			}

			if (i > 2)
			{
				thresholds[i] = 1.1*mean(features_array, Ntrials);
				// features[i] = (range(data_window_column)-meanTrain[i])/stdTrain[i]; // With normalization
			}
		}
		// Serial.println("Q"); // Tell Unity that all fingers have been calibrated
	}
}

// Predict current class based on the calibrated thresholds
String predict_class(double features[], double thresholds[])
{
  String predicted_class = "";

  if (features[0] > 0 || features[1] > 0)
	{
		predicted_class = "S";
	}
	else if (features[4] < thresholds[1] && features[6] < thresholds[3] && features[7] < thresholds[4])
	{
		predicted_class = "F";
	}
	else if (features[4] < thresholds[1])
	{
		predicted_class = "P2";
	}
	else if (features[5] < thresholds[2])
	{
		predicted_class = "P3";
	}
	else if (features[6] < thresholds[3])
	{
		predicted_class = "P4";
	}
	else if (features[7] < thresholds[4]) // && features[3] < thresholds[0])
	{
		predicted_class = "P5";
	}
	else
	{
		predicted_class = "N";
	}

  return predicted_class;
}

//+++ End Function definitions +++//


//+++ Initialization +++//
void setup()
{    
	// Declare the extensimeter pins as INPUTS
	pinMode(ext_thumb_pin, INPUT);
	pinMode(ext_index_pin, INPUT);
	pinMode(ext_middle_pin, INPUT);
	pinMode(ext_ring_pin, INPUT);
	pinMode(ext_little_pin, INPUT);

	// Declare the motor pin as OUTPUT
	pinMode(motorPin, OUTPUT);
	
	// Join I2C bus (I2Cdev library doesn't do this automatically)
	Wire.begin();

	// Initialize IMU objects
	err = ak09918.initialize();
	icm20600.initialize();
	ak09918.switchMode(AK09918_POWER_DOWN);
	ak09918.switchMode(AK09918_CONTINUOUS_100HZ);

	// Initialize serial communication
	Serial.begin(19200);

	// Calibrate IMU
	err = ak09918.isDataReady();
	while (err != AK09918_ERR_OK) 
	{
		Serial.println("Waiting Sensor");
		delay(100);
		err = ak09918.isDataReady();
	}
 
	Serial.println("Start calibration after 2 seconds.");
	delay(2000);
	calibrate(10000, &offset_x, &offset_y, &offset_z);
	Serial.println("");

	// Print a sample of the default command
	Serial.println("N");
}

//+++ End Initialization +++//


//+++ Main loop +++//
void loop()
{
	/*
	// SECTION UNDER DEVELOPMENT
	char command = '0';
	if (Serial.available())
	{
		command = Serial.read();
	}

	while (command != 'V' && command != 'I')
	{
		command = Serial.read();
	}
	
	// Possibly perform calibration UPON USER'S REQUEST
	if (command == 'C')
	{
	  delay(500);
	  calibrate_thresholds();
	  delay(500);
	}
	
	// Possibly VIBRATE when the predicted class is correct
	if (command == 'V')
	{
		analogWrite(motorPin, 165); // Vibrate
		delay(75);  // Duration of the vibration
		analogWrite(motorPin, 0);  // Stop vibrating
	}
	*/

	// Read IMU values
	IMU_reading();

	// Read the voltage from the extensimeters
	ext_reading();

	// Create array with new data
	create_new_data();
	
	// Adjust for offset, keep desired data and group them into an array
	remove_offset();

	// Count items that will form the data window
	counter += 1;

	// Preprocess and predict over a window
	if (counter >= Nsamples)
	{
	  // Take each column of the window and extract the corresponding feature
		extract_feature(data_window);
	  
	  // Compare with thresholds and predict      
	  predicted_class = predict_class(features,thresholds);
	  Serial.println(predicted_class);
	  
	  // Shift window
		shift_window();
	  
		// Update counter
	  counter = max(0,Nsamples-Nshift);
	}
	  
	// Delay to obtain freq ~= 30 Hz
	// delay(33.333); 
}

//+++ End Main loop +++//

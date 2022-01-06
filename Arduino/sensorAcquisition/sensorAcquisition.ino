//+++ Import libraries +++//
#include "AK09918.h"
#include "ICM20600.h"
#include <Wire.h>
//+++ End Import libraries +++//


//+++ Define objects +++//
AK09918_err_type_t err;
AK09918 ak09918;
ICM20600 icm20600(true);
//+++ End Define objects +++//


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

//+++ End Define pins +++//


//+++ Define variables to hold extensimeter voltages +++//
double ext_thumb_val = 0;
double ext_index_val = 0;
double ext_middle_val = 0;
double ext_ring_val = 0;
double ext_little_val = 0;
//+++ End Define variables to hold extensimeter voltages +++//


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
    
    // Join I2C bus (I2Cdev library doesn't do this automatically)
    Wire.begin();

    // Initialize IMU objects
    err = ak09918.initialize();
    icm20600.initialize();
    ak09918.switchMode(AK09918_POWER_DOWN);
    ak09918.switchMode(AK09918_CONTINUOUS_100HZ);

    // Initialize serial communication
    Serial.begin(115200);

    // Calibrate IMU
    err = ak09918.isDataReady();
    while (err != AK09918_ERR_OK) 
    {
        Serial.println("Waiting Sensor");
        delay(100);
        err = ak09918.isDataReady();
    }
 
    Serial.println("Start figure-8 calibration after 2 seconds.");
    delay(2000);
    calibrate(10000, &offset_x, &offset_y, &offset_z);
    Serial.println("");
}

//+++ End Initialization +++//
 

//+++ Main loop +++//
void loop()
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
    roll = 57.3*(atan2((float)acc_y, (float)acc_z));
    pitch = 57.3*(atan2(-(float)acc_x, sqrt((float)acc_y*acc_y+(float)acc_z*acc_z)));
    
    // Heading
    double Xheading = x * cos(pitch) + y * sin(roll) * sin(pitch) + z * cos(roll) * sin(pitch);
    double Yheading = y * cos(roll) - z * sin(pitch);
    heading = 180 + 57.3*atan2(Yheading, Xheading) + declination_shenzhen;


    // Read the voltage from the extensimeters
    ext_thumb_val = analogRead(ext_thumb_pin);
    ext_index_val = analogRead(ext_index_pin);
    ext_middle_val = analogRead(ext_middle_pin);
    ext_ring_val = analogRead(ext_ring_pin);
    ext_little_val = analogRead(ext_little_pin);
    

    // Print all values to the Serial communication device
    Serial.print(acc_x);
    Serial.print(",");
    Serial.print(acc_y);
    Serial.print(",");
    Serial.print(acc_z);

    Serial.print(",");

    Serial.print(gyro_x);
    Serial.print(",");
    Serial.print(gyro_y);
    Serial.print(",");
    Serial.print(gyro_z);

    Serial.print(",");

    /*
    Serial.print(x);
    Serial.print(",");
    Serial.print(y);
    Serial.print(",");
    Serial.print(z);

    Serial.print(",");
    */
    
    Serial.print(roll);
    Serial.print(",");
    Serial.print(pitch);
    Serial.print(",");
    Serial.print(heading);

    Serial.print(",");
    
    Serial.print(ext_thumb_val);
    Serial.print(",");
    Serial.print(ext_index_val);
    Serial.print(",");
    Serial.print(ext_middle_val);
    Serial.print(",");
    Serial.print(ext_ring_val);
    Serial.print(",");
    Serial.println(ext_little_val);

    // Delay to obtain freq ~= 30 Hz
    delay(33.333);
}

//+++ End Main loop +++//

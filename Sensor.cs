using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudantCRUD
{
    #region Sensor
    /// <summary>
    /// Sensor class 
    /// this class is used to manage data from sensors
    /// </summary>
    public class Sensor
    {
        public string time;
        public string dspl;
        //public string _rev;
        public int temp;
        public int hmdt;

        static Random R = new Random();
        static string[] sensorNames = new[] { "sensorA", "sensorB", "sensorC", "sensorD", "sensorE" };

        public static Sensor Generate()
        {
            return new Sensor { time = DateTime.UtcNow.ToString(), dspl = sensorNames[R.Next(sensorNames.Length)], temp = R.Next(70, 150), hmdt = R.Next(30, 70) };
        }

        public static Sensor Update(Sensor sensor, string revision)
        {
            return new Sensor { time = sensor.time, dspl = sensor.dspl, temp = sensor.temp, hmdt = sensor.hmdt };
        }
    }

    #endregion Sensor
}

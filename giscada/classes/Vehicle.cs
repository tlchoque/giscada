using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace giscada.classes
{
    public class Vehicle
    {
        public Position location;
        public string plate;
        public string speed;
        public string date;

        public Vehicle(Position _location, string _plate, string _speed,string _date)
        {
            location = _location;
            plate = _plate;
            speed = _speed;
            date = _date;
        }

        public Vehicle() {}
    }


}
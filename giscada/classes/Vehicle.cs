using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace giscada.classes
{
    //public class Vehicle
    //{
    //    public Position location;
    //    public string plate;
    //    public string speed;
    //    public string date;

    //    public Vehicle(Position _location, string _plate, string _speed,string _date)
    //    {
    //        location = _location;
    //        plate = _plate;
    //        speed = _speed;
    //        date = _date;
    //    }

    //    public Vehicle() {}
    //}


    public class Vehicle
    {
        public string Avl { get; set; }
        public string Timestamp { get; set; }
        public string Speed { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }

        public Vehicle() { }
    }

}
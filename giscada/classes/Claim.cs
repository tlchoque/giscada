using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;


namespace giscada.classes
{
    public class Claim
    {
        public string code;
        public Position location;
        public DateTime date;
        public string quality;// clase
        public string type;
        public string description;
        public string supply;
        public string consumption;
        public string client;

        public Claim(string _code,Position _location, DateTime _date, string _quality,
            string _type, string _description, string _supply, string _consumption, string _client) {
            code = _code;
            location = _location;
            date = _date;
            quality = _quality;
            type = _type;
            description = _description;
            supply = _supply;
            consumption = _consumption;
            client = _client;
        }

        public Claim() {}
    }
}
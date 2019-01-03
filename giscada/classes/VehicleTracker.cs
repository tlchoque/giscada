using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;

using System.Data;
using System.Xml;

using System.IO;
using System.Net;

using Newtonsoft.Json.Linq;

namespace giscada.classes
{
    public class VehicleTracker
    {
        public List<Vehicle> vehicleList;
        public Dictionary<string, Vehicle> vehicleHash;

        public VehicleTracker() {
            vehicleList = new List<Vehicle>();
            vehicleHash = new Dictionary<string, Vehicle>();
        }

        //public void InitializeVehicles()
        //{
        //    string user = "demo@electrotacna";
        //    string pass = "@Demo17@";
        //    ServiceReference1.WebServiceClienteSoapClient wsMov = new ServiceReference1.WebServiceClienteSoapClient();
        //    string dataRec = wsMov.ReceiveDataXML(user, pass, "108");
        //    XmlDocument xmlDoc = new XmlDocument();
        //    xmlDoc.LoadXml(dataRec);

        //    XmlNode moviles = xmlDoc.ChildNodes.Item(0);
        //    //foreach (XmlNode chldNode in moviles.ChildNodes)
        //    //{

        //    foreach (XmlNode chldNode in moviles.ChildNodes)
        //     {
        //        if (chldNode.ChildNodes.Count < 3) continue;
        //        string lat = chldNode.SelectSingleNode("lat").InnerText;
        //        string lon = chldNode.SelectSingleNode("lon").InnerText;
        //        string plate = chldNode.SelectSingleNode("pat").InnerText;
        //        string speed = chldNode.SelectSingleNode("vel").InnerText;
        //        string date = chldNode.SelectSingleNode("fh").InnerText;
        //        string dirm = chldNode.SelectSingleNode("dirm").InnerText;

        //        Vehicle v = new Vehicle(new Position(lat, lon), plate, speed, date);
        //        vehicles.Add(v);
        //        vehicleHash[plate] = v;
        //    }
        //}

        //public bool NewPositions()
        //{
        //    bool changed = false;
        //    string user = "demo@electrotacna";
        //    string pass = "@Demo17@";
        //    ServiceReference1.WebServiceClienteSoapClient wsMov = new ServiceReference1.WebServiceClienteSoapClient();
        //    string dataRec = wsMov.ReceiveDataXML(user, pass, "108");
        //    XmlDocument xmlDoc = new XmlDocument();
        //    xmlDoc.LoadXml(dataRec);

        //    XmlNode moviles = xmlDoc.ChildNodes.Item(0);
        //    foreach (XmlNode chldNode in moviles.ChildNodes)
        //    {
        //        if (chldNode.ChildNodes.Count < 3) continue;
        //        string lat = chldNode.SelectSingleNode("lat").InnerText;
        //        string lon = chldNode.SelectSingleNode("lon").InnerText;
        //        string plate = chldNode.SelectSingleNode("pat").InnerText;
        //        Position newPosition = new Position(lat, lon);
        //        if (vehicleHash[plate].location != newPosition) {
        //            vehicleHash[plate].location = newPosition;
        //            changed = true;
        //        }
        //    }
        //    return changed;
        //}

        //public string InitializeLayer() {
        //    string user = "demo@electrotacna";
        //    string pass = "@Demo17@";
        //    ServiceReference1.WebServiceClienteSoapClient wsMov = new ServiceReference1.WebServiceClienteSoapClient();
        //    string dataRec = wsMov.ReceiveDataXML(user, pass, "108");
        //    XmlDocument xmlDoc = new XmlDocument();
        //    xmlDoc.LoadXml(dataRec);

        //    var model = new FeatureCollection();

        //    XmlNode moviles = xmlDoc.ChildNodes.Item(0);
        //    foreach (XmlNode chldNode in moviles.ChildNodes)
        //    {
        //        if (chldNode.ChildNodes.Count < 3) continue;
        //        string lat = chldNode.SelectSingleNode("lat").InnerText;
        //        string lon = chldNode.SelectSingleNode("lon").InnerText;
        //        string plate = chldNode.SelectSingleNode("pat").InnerText;

        //        Position newPosition = new Position(lat, lon);
        //        Vehicle v = vehicleHash[plate];
        //        v.location = newPosition;

        //        var geom = new Point(v.location);
        //        var props = new Dictionary<string, object>
        //        {
        //            { "plate", v.plate },
        //            { "speed", v.speed },
        //            { "date", v.date }
        //        };
        //        var feature = new GeoJSON.Net.Feature.Feature(geom, props);
        //        model.Features.Add(feature);
        //    }
        //    var actualjson = JsonConvert.SerializeObject(model);
        //    return actualjson;
        //}


        public string GET(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    // log errorText
                }
                throw;
            }
        }

        public void InitializeVehicles() {
            string json = GET("http://else-general.gpsgoldcar.com/avl/last-position/?avl_unit_group_id=4");
            Vehicle[] vehicles = JsonConvert.DeserializeObject<Vehicle[]>(json);
            for (int i = 0; i < vehicles.Length; ++i) {
                Vehicle v = vehicles[i];
                if (string.IsNullOrWhiteSpace(v.Latitude)) continue;
                vehicleList.Add(v);
                vehicleHash[v.Avl] = v;
            }
        }

        public bool CheckVehicleUpdates() {
            bool change = false;
            HashSet<string> codeHash = new HashSet<string>();
            string json = GET("http://else-general.gpsgoldcar.com/avl/last-position/?avl_unit_group_id=4");
            Vehicle[] vehicles = JsonConvert.DeserializeObject<Vehicle[]>(json);

            for (int i = 0; i < vehicles.Length; ++i) {
                Vehicle v = vehicles[i];
                if (string.IsNullOrWhiteSpace(v.Latitude)) continue;
                Vehicle vs = new Vehicle();
                if (!vehicleHash.TryGetValue(v.Avl, out vs)) {
                    change = true;
                    if (string.IsNullOrWhiteSpace(v.Latitude)) continue;
                    vehicleList.Add(v);
                    vehicleHash[v.Avl] = v;
                }           
                
                if (vs.Timestamp != v.Timestamp)
                {
                    change = true;
                    vs.Timestamp = v.Timestamp;
                    vs.Speed = v.Speed;
                    vs.Latitude = v.Latitude;
                    vs.Longitude = v.Longitude;
                }     

                codeHash.Add(v.Avl);
            }

            //remove cars that dont exist any more
            foreach (string code in vehicleHash.Keys.ToList())
            {
                if (!codeHash.Contains(code))
                {
                    vehicleHash.Remove(code);
                    change = true;
                }
            }
            return change;
        }

        public string GetCurrentVehicleValues() {
            var model = new FeatureCollection();
            foreach (Vehicle v in vehicleHash.Values) {
                var geom = new Point( new Position( v.Latitude, v.Longitude));
                var props = new Dictionary<string, object>
                {
                    { "avl", v.Avl },
                    { "tim", v.Timestamp },
                    { "spe", v.Speed }
                };
                var feature = new GeoJSON.Net.Feature.Feature(geom, props);
                model.Features.Add(feature);
            }
            var vehiclejson = JsonConvert.SerializeObject(model);
            return vehiclejson;
        }
    }
}
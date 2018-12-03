using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;

using System.Data;
using System.Xml;

namespace giscada.classes
{
    public class VehicleTracker
    {
        public List<Vehicle> vehicles;
        public Dictionary<string, Vehicle> vehicleHash;

        public VehicleTracker() {
            vehicles = new List<Vehicle>();
            vehicleHash = new Dictionary<string, Vehicle>();
        }

        public void InitializeVehicles()
        {
            string user = "demo@electrotacna";
            string pass = "@Demo17@";
            ServiceReference1.WebServiceClienteSoapClient wsMov = new ServiceReference1.WebServiceClienteSoapClient();
            string dataRec = wsMov.ReceiveDataXML(user, pass, "108");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(dataRec);

            XmlNode moviles = xmlDoc.ChildNodes.Item(0);
            //foreach (XmlNode chldNode in moviles.ChildNodes)
            //{

            foreach (XmlNode chldNode in moviles.ChildNodes)
             {
                if (chldNode.ChildNodes.Count < 3) continue;
                string lat = chldNode.SelectSingleNode("lat").InnerText;
                string lon = chldNode.SelectSingleNode("lon").InnerText;
                string plate = chldNode.SelectSingleNode("pat").InnerText;
                string speed = chldNode.SelectSingleNode("vel").InnerText;
                string date = chldNode.SelectSingleNode("fh").InnerText;
                string dirm = chldNode.SelectSingleNode("dirm").InnerText;

                Vehicle v = new Vehicle(new Position(lat, lon), plate, speed, date);
                vehicles.Add(v);
                vehicleHash[plate] = v;
            }
        }

        public bool NewPositions()
        {
            bool changed = false;
            string user = "demo@electrotacna";
            string pass = "@Demo17@";
            ServiceReference1.WebServiceClienteSoapClient wsMov = new ServiceReference1.WebServiceClienteSoapClient();
            string dataRec = wsMov.ReceiveDataXML(user, pass, "108");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(dataRec);

            XmlNode moviles = xmlDoc.ChildNodes.Item(0);
            foreach (XmlNode chldNode in moviles.ChildNodes)
            {
                if (chldNode.ChildNodes.Count < 3) continue;
                string lat = chldNode.SelectSingleNode("lat").InnerText;
                string lon = chldNode.SelectSingleNode("lon").InnerText;
                string plate = chldNode.SelectSingleNode("pat").InnerText;
                Position newPosition = new Position(lat, lon);
                if (vehicleHash[plate].location != newPosition) {
                    vehicleHash[plate].location = newPosition;
                    changed = true;
                }
            }
            return changed;
        }

        public string InitializeLayer() {
            string user = "demo@electrotacna";
            string pass = "@Demo17@";
            ServiceReference1.WebServiceClienteSoapClient wsMov = new ServiceReference1.WebServiceClienteSoapClient();
            string dataRec = wsMov.ReceiveDataXML(user, pass, "108");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(dataRec);

            var model = new FeatureCollection();

            XmlNode moviles = xmlDoc.ChildNodes.Item(0);
            foreach (XmlNode chldNode in moviles.ChildNodes)
            {
                if (chldNode.ChildNodes.Count < 3) continue;
                string lat = chldNode.SelectSingleNode("lat").InnerText;
                string lon = chldNode.SelectSingleNode("lon").InnerText;
                string plate = chldNode.SelectSingleNode("pat").InnerText;

                Position newPosition = new Position(lat, lon);
                Vehicle v = vehicleHash[plate];
                v.location = newPosition;

                var geom = new Point(v.location);
                var props = new Dictionary<string, object>
                {
                    { "plate", v.plate },
                    { "speed", v.speed },
                    { "date", v.date }
                };
                var feature = new GeoJSON.Net.Feature.Feature(geom, props);
                model.Features.Add(feature);
            }
            var actualjson = JsonConvert.SerializeObject(model);
            return actualjson;
        }
    }
}
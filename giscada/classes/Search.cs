using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.SqlClient;

using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

using Newtonsoft.Json;

using System.IO;
using System.Xml;

using zenOn;
namespace giscada.classes
{

    public class TopLocation {
        public string Name;
        public string Category;
        public Position Position;
        public string Feeder;
        public TopLocation( string _Category,string _Name, Position _Position, string _Feeder) {
            if (_Category == "B") Category = "Protección";
            else if (_Category == "S") Category = "Subestación";
            else if (_Category == "C") Category = "Cliente";
            else Category = "Poste";
            Name = _Name;
            Position = _Position;
            Feeder = _Feeder;
        }
    }


    public class Search : DataAccess
    {
        public Position GetBreakerPosition(string name, string feeder) {
            DataTable row = ExecuteSelectQuery("select * from tramo where Etiprot = '"+ name + "' and codali = '"+feeder+"'");
            double latitud = Convert.ToDouble(row.Rows[0]["vany1"].ToString());
            double longitud = Convert.ToDouble(row.Rows[0]["vanx1"].ToString());
            return new Position(latitud, longitud);
        }

        public Position GetSubstationPosition(string name)
        {
            DataTable row = ExecuteSelectQuery("select * from els_2018.subestacion where id = '"+name+"'");
            Position p = ToLatLon(Convert.ToDouble(row.Rows[0]["utm_x"]), Convert.ToDouble(row.Rows[0]["utm_y"]), "19S");
            return p;
        }

        public Position GetClientPosition(string name)
        {
            DataTable row = ExecuteSelectQuery("select * from els_2018.cliente where contrato = '" + name + "'");
            Position p = ToLatLon(Convert.ToDouble(row.Rows[0]["utm_x"]), Convert.ToDouble(row.Rows[0]["utm_y"]), "19S");
            return p;
        }

        public Position GetPolePosition(string name)
        {
            DataTable row = ExecuteSelectQuery("select * from els_2018.poste where etiqueta = '" + name + "'");
            Position p = ToLatLon(Convert.ToDouble(row.Rows[0]["utm_x"]), Convert.ToDouble(row.Rows[0]["utm_y"]), "19S");
            return p;
        }

        public List<TopLocation> GetTopResults(string substring)
        {
            List<TopLocation> TopResults = new List<TopLocation>();
            DataTable  results= ExecuteSelectQuery("select top 10 * from SearchLabel where label like '" + substring + "%' order by label asc");
            for (int i = 0; i < results.Rows.Count; ++i)
            {
                string cat = results.Rows[i]["category"].ToString();
                string name = results.Rows[i]["label"].ToString();
                Position p;
                if ( cat == "B" )
                {
                    p = new Position(Convert.ToDouble( results.Rows[i]["latitude"].ToString()), Convert.ToDouble(results.Rows[i]["longitude"].ToString()) );
                }
                else {
                    p = ToLatLon(Convert.ToDouble(results.Rows[i]["longitude"]), Convert.ToDouble(results.Rows[i]["latitude"]), "19S");
                }               
                string feeder = results.Rows[i]["feeder"].ToString();
                TopResults.Add(new TopLocation(cat,name, p,feeder));
            }
            return TopResults;
        }
    }
}
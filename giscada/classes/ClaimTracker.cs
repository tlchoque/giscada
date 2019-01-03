using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.SqlClient;

using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

using Newtonsoft.Json;

namespace giscada.classes
{
    public class ClaimTracker : DataAccess
    {
        class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }

        public List<Claim> claimList;
        public SortedDictionary<string, Claim> claimHash;
        //public OrderedDictionary claimHash;

        public ClaimTracker()
        {
            claimList = new List<Claim>();
            claimHash = new SortedDictionary<string, Claim>(new DescendingComparer<string>());
            //claimHash = new OrderedDictionary();
        }

        public void InitializeClaims()
        {
            DateTime past = DateTime.Now.AddDays(-2);
            string date1 = past.Year.ToString() + past.Month.ToString("00") + past.Day.ToString("00");
            string date2 = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");            
            DataTable claims = ExecuteCommercialQuery("exec RptSer_ListaReclamosPendientes '"+date1+"','"+date2+"'");
            for (int i = 0; i < claims.Rows.Count; ++i)
            {
                if ( string.IsNullOrWhiteSpace( claims.Rows[i]["Latitud"].ToString()) ) continue;
                string code = claims.Rows[i]["CodigoReclamo"].ToString();        
                Position location = new Position(claims.Rows[i]["Latitud"].ToString(), claims.Rows[i]["Longitud"].ToString());
                DateTime date = Convert.ToDateTime(claims.Rows[i]["FechaRegistroReclamo"].ToString());
                string quality = claims.Rows[i]["NombreClaseReclamo"].ToString();
                string type = claims.Rows[i]["NombreTipoReclamo"].ToString();
                string description = claims.Rows[i]["DescripcionReclamo"].ToString();
                string supply = claims.Rows[i]["CodigoSuministro"].ToString();
                string consumption = claims.Rows[i]["Ultimo_L_CEA"].ToString();
                string client = claims.Rows[i]["NombreSuministro"].ToString();
                Claim c = new Claim(code,location, date, quality,type, description, supply, consumption, client);
                claimList.Add(c);
                claimHash[code] = c;
            }
        }


        public bool CheckUpdates() {
            // check if the cliam is open or close, check the type feature
            bool change = false;
            // check if the code is still in the hash
            HashSet<string> codeHash = new HashSet<string>();

            DateTime past = DateTime.Now.AddDays(-2);
            string date1 = past.Year.ToString() + past.Month.ToString("00") + past.Day.ToString("00");
            string date2 = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");
            DataTable claims = ExecuteCommercialQuery("exec RptSer_ListaReclamosPendientes '" + date1 + "','" + date2 + "'");

            for (int i = 0; i < claims.Rows.Count; ++i) {
                if (string.IsNullOrWhiteSpace(claims.Rows[i]["Latitud"].ToString())) continue;
                string code = claims.Rows[i]["CodigoReclamo"].ToString();
                Claim c = new Claim();
                if (!claimHash.TryGetValue(code, out c)) {
                    change = true;
                    // here we need to create a new claim for the dictionary
                    Position location = new Position(claims.Rows[i]["Latitud"].ToString(), claims.Rows[i]["Longitud"].ToString());
                    DateTime date = Convert.ToDateTime(claims.Rows[i]["FechaRegistroReclamo"].ToString());
                    string quality = claims.Rows[i]["NombreClaseReclamo"].ToString();
                    string type = claims.Rows[i]["NombreTipoReclamo"].ToString();
                    string description = claims.Rows[i]["DescripcionReclamo"].ToString();
                    string supply = claims.Rows[i]["CodigoSuministro"].ToString();
                    string consumption = claims.Rows[i]["Ultimo_L_CEA"].ToString();
                    string client = claims.Rows[i]["NombreSuministro"].ToString();
                    Claim caux = new Claim(code, location, date, quality, type, description, supply, consumption, client);
                    claimList.Add(caux);
                    claimHash[code] = caux;                    
                }
                codeHash.Add(code);
                //else if (c.quality == "cerrado") {
                //    change = true;
                //}

                //check the pair that should not exist

            }
            foreach (string code in claimHash.Keys.ToList()) {
                if (!codeHash.Contains(code)) {
                    claimHash.Remove(code);
                    change = true;
                }
            }

            return change;
        }

        public string GetCurrentValues() {
            var model = new FeatureCollection();
            foreach (Claim c in claimHash.Values)
            {
                var geom = new Point(c.location);
                var props = new Dictionary<string, object>
                {
                    { "cod", c.code },
                    { "dat", c.date },
                    { "qua", c.quality },
                    { "typ", c.type },
                    { "des", c.description },
                    { "sup", c.supply },
                    { "con", c.consumption},
                    { "cli", c.client }
                };
                var feature = new GeoJSON.Net.Feature.Feature(geom, props);
                model.Features.Add(feature);
            }
            var claimjson = JsonConvert.SerializeObject(model);
            return claimjson;
        }
    }
}
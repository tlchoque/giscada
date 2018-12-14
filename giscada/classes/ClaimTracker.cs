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
        public List<Claim> claimList;
        public Dictionary<string, Claim> claimHash;

        public ClaimTracker()
        {
            claimList = new List<Claim>();
            claimHash = new Dictionary<string, Claim>();
        }

        public void InitializeClaims()
        {
            DataTable claims = ExecuteSelectQuery("select * from reclamo");
            for (int i = 0; i < claims.Rows.Count; ++i)
            {
                string code = claims.Rows[i]["codigo"].ToString();
                Position location = new Position(claims.Rows[i]["latitud"].ToString(), claims.Rows[i]["longitud"].ToString());
                DateTime date = Convert.ToDateTime(claims.Rows[i]["fecha"].ToString());
                string quality = claims.Rows[i]["clase"].ToString();
                string type = claims.Rows[i]["tipo"].ToString();
                string description = claims.Rows[i]["descripcion"].ToString();
                string supply = claims.Rows[i]["suministro"].ToString();
                string consumption = claims.Rows[i]["consumo"].ToString();
                string client = claims.Rows[i]["cliente"].ToString();
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
            DataTable claims = ExecuteSelectQuery("select * from reclamo");
            for (int i = 0; i < claims.Rows.Count; ++i) {
                string code = claims.Rows[i]["codigo"].ToString();
                Claim c = new Claim();
                if (!claimHash.TryGetValue(code, out c)) {
                    change = true;
                    // here we need to create a new claim for the dictionary
                    Position location = new Position(claims.Rows[i]["latitud"].ToString(), claims.Rows[i]["longitud"].ToString());
                    DateTime date = Convert.ToDateTime(claims.Rows[i]["fecha"].ToString());
                    string quality = claims.Rows[i]["clase"].ToString();
                    string type = claims.Rows[i]["tipo"].ToString();
                    string description = claims.Rows[i]["descripcion"].ToString();
                    string supply = claims.Rows[i]["suministro"].ToString();
                    string consumption = claims.Rows[i]["consumo"].ToString();
                    string client = claims.Rows[i]["cliente"].ToString();
                    Claim caux = new Claim(code, location, date, quality, type, description, supply, consumption, client);
                    claimList.Add(caux);
                    claimHash[code] = caux;                    
                }
                else if (c.quality == "cerrado") {
                    change = true;
                }
                //check the pair that should not exist
                codeHash.Add(code);
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
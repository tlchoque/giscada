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
    public class FeederBreakers:DataAccess
    {
        public Project proj;
        public Dictionary<string, Graph<Node>> feederHash;
        public FeatureCollection lineFeatures;
        public FeatureCollection breakerFeatures;
        public readonly Dictionary<int, Breaker> breakerHash;

        public FeederBreakers(Project _proj = null)
        {
            proj = _proj;
            feederHash = new Dictionary<string, Graph<Node>>();
            lineFeatures = new FeatureCollection();
            breakerFeatures = new FeatureCollection();
            breakerHash = new Dictionary<int, Breaker>();
        }

        public void Clear()
        {
            feederHash.Clear();
            breakerHash.Clear();
            lineFeatures = new FeatureCollection();
        }

        public void CreateLineFeature(string feeder, List<Position> geom, Breaker breaker)
        {
            if (geom.Count() < 2) return;
            var lineString = new LineString(geom);
            var props = new Dictionary<string, object>
                {
                    //{ "type", "mv_line" },
                    { "feed", feeder },
                    { "name", breaker.name },
                };
            var feature = new GeoJSON.Net.Feature.Feature(lineString, props);
            lineFeatures.Features.Add(feature);
        }

        public Feature CreateBreakerFeature(string feeder, Breaker breaker)
        {
            var geom = new Point(breaker.node.position);
            var props = new Dictionary<string, object>
                {
                    //{ "type", "breaker" },
                    { "feed", feeder },
                    { "name", breaker.name },
                    { "short", breaker.shortName},
                };
            var feature = new Feature(geom, props);
            return feature;
        }

        public IVariable GetVariableFromZenon(string name)
        {
            IVariable var = null;
            if (proj != null) var = proj.Variables().Item(name);
            return var;
        }

        public void DFS(Graph<Node> G)
        {
            Stack<Node> stack = new Stack<Node>();
            stack.Push(G.head);
            bool newLine = false;
            Stack<Breaker> breakerStack = new Stack<Breaker>();
            breakerStack.Push(G.breakerGraph.head);

            List<Position> geom = new List<Position>();
            while (stack.Count != 0)
            {
                Node current = stack.Pop();
                if (newLine)
                {
                    newLine = false;
                    geom.Clear();
                    geom.Add(current.parent.position);
                }
                geom.Add(current.position);
                if (current.isBreaker)
                {
                    newLine = true;
                    Breaker top = breakerStack.Pop();
                    CreateLineFeature(G.name, geom, top);

                    Breaker aux = current.breaker;
                    aux.parent = top;
                    foreach (Node child in current.children)
                        breakerStack.Push(aux);
                    //why it is repeated
                    breakerHash.Add(aux.lineId, aux);

                    top.children.AddFirst(aux);
                    var bfeature = CreateBreakerFeature(G.name, aux);
                    //lineFeatures.Features.Add(bfeature);
                    breakerFeatures.Features.Add(bfeature);
                }
                else if (current.children.Count != 1)
                {
                    newLine = true;
                    Breaker top = breakerStack.Pop();
                    CreateLineFeature(G.name, geom, top);

                    foreach (Node child in current.children)
                        breakerStack.Push(top);

                }
                foreach (Node child in current.children)
                    stack.Push(child);
            }
        }

        public void Generate()
        {
            //DataTable feeders = ExecuteSelectQuery("select distinct codali,nodoi,vany1,vanx1 from tramo where nodoi = 0");    
            DataTable feeders = ExecuteSelectQuery("select distinct codali,codigotraa,vany1,vanx1 from tramo where nodoi = 0");
            for (int i = 0; i < feeders.Rows.Count; ++i)
            {
                string feeder = feeders.Rows[i][0].ToString();
                //int id = Int32.Parse(feeders.Rows[i]["nodoi"].ToString());}
                int id = Int32.Parse(feeders.Rows[i]["codigotraa"].ToString());
                Position pos = new Position(feeders.Rows[i]["vany1"].ToString(), feeders.Rows[i]["vanx1"].ToString());
                IVariable zfeeder = GetVariableFromZenon(feeder);

                Graph<Node> nodes = new Graph<Node>(feeder);
                Graph<Breaker> breakers = new Graph<Breaker>();                

                //create the start breaker for each feeder
                Node headNode = new Node(pos, true);                
                Breaker headBreaker = new Breaker(feeder,id, zfeeder);
                
                feederHash[feeder] = nodes;
                nodes.head = headNode;
                nodes.breakerGraph = breakers;
                nodes.nodeHash[id] = headNode;

                headNode.breaker = headBreaker;
                headBreaker.node = headNode;

                breakers.head = headBreaker;
            }

            //fill the hashes of each feeder
            Node sourceNode = new Node();
            Node endNode = new Node();
            DataTable lines = ExecuteSelectQuery("select * from tramo where nodoi != nodof");            
            for (int i = 0; i < lines.Rows.Count; ++i)
            {
                string feeder = lines.Rows[i][1].ToString();
                int source = Int32.Parse(lines.Rows[i]["codigotraa"].ToString());
                int end = Int32.Parse(lines.Rows[i]["codigotra"].ToString());
                bool isBreaker = Convert.ToBoolean(Int32.Parse(lines.Rows[i]["EquipProt"].ToString()));
                string breakerName = feeder + "_" + lines.Rows[i]["Etiprot"].ToString();
                IVariable zbreaker = GetVariableFromZenon(breakerName);
                Breaker br = new Breaker(breakerName,source,zbreaker);

                Graph<Node> G = feederHash[feeder];
                Dictionary<int, Node> NodeHash = G.nodeHash;
                if (!NodeHash.TryGetValue(source, out sourceNode))
                {
                    Position pos = new Position(lines.Rows[i]["vany1"].ToString(), lines.Rows[i]["vanx1"].ToString());
                    sourceNode = new Node(pos, isBreaker);
                    NodeHash.Add(source, sourceNode);
                    sourceNode.breaker = br;
                    br.node = sourceNode;
                }
                else
                {
                    sourceNode.isBreaker = isBreaker;
                    sourceNode.breaker = br;
                    br.node = sourceNode;
                }
                if (!NodeHash.TryGetValue(end, out endNode))
                {
                    Position pos = new Position(lines.Rows[i]["vany2"].ToString(), lines.Rows[i]["vanx2"].ToString());
                    endNode = new Node(pos, false);
                    NodeHash.Add(end, endNode);
                }
                endNode.parent = sourceNode;    
                sourceNode.children.AddFirst(endNode);
            }

            foreach (Graph<Node> G in feederHash.Values)
                DFS(G);
            
            //File.WriteAllText(@"C:\cygwin64\home\tony\giscada\mv_line.geojson", JsonConvert.SerializeObject(lineFeatures));
            //File.WriteAllText(@"C:\cygwin64\home\tony\giscada\breaker.geojson", JsonConvert.SerializeObject(breakerFeatures));
        }

        public void UpdateLineStatusByGraph(Breaker b)
        {
            Stack<Breaker> stack = new Stack<Breaker>();
            stack.Push(b);
            while (stack.Count != 0)
            {
                Breaker current = stack.Pop();
                if (current.parent == null) current.lineStatus = current.status;
                else current.lineStatus = current.parent.lineStatus * current.status;

                foreach (Breaker child in current.children)
                    stack.Push(child);
            }
        }

        public void InfoForQGIS()
        {
            FeatureCollection lv_line = new FeatureCollection();
            FeatureCollection lv_service_line = new FeatureCollection();
            FeatureCollection substation = new FeatureCollection();
            FeatureCollection substation_text = new FeatureCollection();
            FeatureCollection mv_pole = new FeatureCollection();
            FeatureCollection mv_pole_text = new FeatureCollection();
            FeatureCollection lv_pole = new FeatureCollection();
            FeatureCollection lv_pole_text = new FeatureCollection();
            FeatureCollection client = new FeatureCollection();
            FeatureCollection client_text = new FeatureCollection();

            FeatureCollection aux = new FeatureCollection();
            FeatureCollection aux_text = new FeatureCollection();

            string feeder_ex = "('O-145','O-242','O-243')";
            DataTable btlines = ExecuteSelectQuery("select codigo,circuito,alm,sed,ckto,utm from els_2018.linea where estado = 'Existente'and tipo = 'BT' and uso != 'Servicio' and sed !=''");
            //DataTable btlines = ExecuteSelectQuery("select codigo,circuito,alm,sed,ckto,utm from els_2018.linea where estado = 'Existente'and tipo = 'BT' and sed !='' and uso != 'Servicio' and alm in "+ feeder_ex);
            for (int i = 0; i < btlines.Rows.Count; ++i)
            {
                List<Position> geom = new List<Position>();
                string utm = btlines.Rows[i]["utm"].ToString();
                string[] strArr = utm.Split(' ');
                for (int count = 0; count <= strArr.Length - 1; count++)
                {
                    string[] pos = strArr[count].Split(',');
                    Position p = ToLatLon(Convert.ToDouble(pos[0]), Convert.ToDouble(pos[1]), "19S");
                    geom.Add(p);
                }
                if (geom.Count() < 2) continue;

                var lineString = new LineString(geom);
                //here we need the information about the circuit
                var props = new Dictionary<string, object>
                {
                    { "feed", btlines.Rows[i]["alm"].ToString() },
                    { "sub", btlines.Rows[i]["sed"].ToString() },
                    { "cir", btlines.Rows[i]["ckto"].ToString() }
                };
                var feature = new GeoJSON.Net.Feature.Feature(lineString, props);
                lv_line.Features.Add(feature);
            }

            DataTable btsevicelines = ExecuteSelectQuery("select codigo,circuito,alm,sed,ckto,utm from els_2018.linea where estado = 'Existente'and tipo = 'BT' and uso = 'Servicio' and sed !=''");
            //DataTable btsevicelines = ExecuteSelectQuery("select codigo,circuito,alm,sed,ckto,utm from els_2018.linea where estado = 'Existente'and tipo = 'BT' and sed !='' and uso = 'Servicio' and alm in " + feeder_ex);
            for (int i = 0; i < btsevicelines.Rows.Count; ++i)
            {
                List<Position> geom = new List<Position>();
                string utm = btsevicelines.Rows[i]["utm"].ToString();
                string[] strArr = utm.Split(' ');
                for (int count = 0; count <= strArr.Length - 1; count++)
                {
                    string[] pos = strArr[count].Split(',');
                    Position p = ToLatLon(Convert.ToDouble(pos[0]), Convert.ToDouble(pos[1]), "19S");
                    geom.Add(p);
                }
                if (geom.Count() < 2) continue;

                var lineString = new LineString(geom);
                var props = new Dictionary<string, object>
                {
                    //{ "type", "lv_service_line" },
                    { "feed", btsevicelines.Rows[i]["alm"].ToString() },
                    { "sub", btsevicelines.Rows[i]["sed"].ToString() },
                    { "cir", btsevicelines.Rows[i]["ckto"].ToString() }
                };
                var feature = new GeoJSON.Net.Feature.Feature(lineString, props);
                lv_service_line.Features.Add(feature);
            }

            DataTable substations = ExecuteSelectQuery("select codigo,id,etiqueta,alm,utm_x,utm_y,utm_s_x,utm_s_y from els_2018.subestacion where estado = 'Existente' and id != '' and etiqueta != '' and utm_s_x != 0");
            //DataTable substations = ExecuteSelectQuery("select codigo,id,etiqueta,alm,utm_x,utm_y,utm_s_x,utm_s_y from els_2018.subestacion where estado = 'Existente' and id != '' and etiqueta != '' and utm_s_x != 0 and alm IN " + feeder_ex);
            for (int i = 0; i < substations.Rows.Count; ++i)
            {
                Position p = ToLatLon(Convert.ToDouble(substations.Rows[i]["utm_x"]), Convert.ToDouble(substations.Rows[i]["utm_y"]), "19S");
                var geom = new Point(p);
                var props = new Dictionary<string, object>
                {
                    //{ "type", "sub" },
                    { "feed", substations.Rows[i]["alm"].ToString() },
                    { "name", substations.Rows[i]["id"].ToString() },
                };
                var feature = new GeoJSON.Net.Feature.Feature(geom, props);
                substation.Features.Add(feature);

                //label feature
                Position p_lbl = ToLatLon(Convert.ToDouble(substations.Rows[i]["utm_s_x"]), Convert.ToDouble(substations.Rows[i]["utm_s_y"]), "19S");
                var geom_lbl = new Point(p_lbl);
                var props_lbl = new Dictionary<string, object>
                {
                    //{ "type", "sub_lbl" },
                    { "feed", substations.Rows[i]["alm"].ToString() },
                    { "name", substations.Rows[i]["id"].ToString() },
                };
                var feature_lbl = new GeoJSON.Net.Feature.Feature(geom_lbl, props_lbl);
                substation_text.Features.Add(feature_lbl);
            }

            DataTable poles = ExecuteSelectQuery("select tipo_uso, etiqueta, alm, sed, utm_x, utm_y, utm_s_x, utm_s_y from els_2018.poste where estado = 'Existente' and utm_x != 0 and (tipo_uso = 'Red Secundaria' or tipo_uso = 'Red Primaria' or tipo_uso = 'Alumbrado Publico')");
            //DataTable poles = ExecuteSelectQuery("select tipo_uso, etiqueta, alm, sed, utm_x, utm_y, utm_s_x, utm_s_y from els_2018.poste where estado = 'Existente' and utm_x != 0 and (tipo_uso = 'Red Secundaria' or tipo_uso = 'Red Primaria' or tipo_uso = 'Alumbrado Publico') and alm IN " + feeder_ex);
            for (int i = 0; i < poles.Rows.Count; ++i)
            {
                string type,type_lbl;
                if (poles.Rows[i]["tipo_uso"].ToString() == "Red Primaria") {
                    type = "mv_pole"; type_lbl = "mv_pole_lbl"; aux = mv_pole; aux_text = mv_pole_text;
                }
                else {
                    type = "lv_pole"; type_lbl = "lv_pole_lbl"; aux = lv_pole; aux_text = lv_pole_text;
                }
                Position p = ToLatLon(Convert.ToDouble(poles.Rows[i]["utm_x"]), Convert.ToDouble(poles.Rows[i]["utm_y"]), "19S");
                var geom = new Point(p);
                var props = new Dictionary<string, object>
                {
                    //{ "type", type },
                    { "feed", poles.Rows[i]["alm"].ToString() },
                    { "name", poles.Rows[i]["etiqueta"].ToString() },
                };
                var feature = new GeoJSON.Net.Feature.Feature(geom, props);
                aux.Features.Add(feature);

                //for labels
                Position p_lbl = ToLatLon(Convert.ToDouble(poles.Rows[i]["utm_s_x"]), Convert.ToDouble(poles.Rows[i]["utm_s_y"]), "19S");
                var geom_lbl = new Point(p_lbl);
                var props_lbl = new Dictionary<string, object>
                {
                    //{ "type", type_lbl },
                    { "feed", poles.Rows[i]["alm"].ToString() },
                    { "name", poles.Rows[i]["etiqueta"].ToString() },
                };
                var feature_lbl = new GeoJSON.Net.Feature.Feature(geom_lbl, props_lbl);
                aux_text.Features.Add(feature_lbl);
            }

            DataTable clients = ExecuteSelectQuery("select circuito,alm,sed,ckto,suministro,utm_x,utm_y,utm_s_x,utm_s_y from els_2018.punto_servicio where estado = 'Existente' and suministro != ' '");
            //DataTable clients = ExecuteSelectQuery("select circuito,alm,sed,ckto,suministro,utm_x,utm_y,utm_s_x,utm_s_y from els_2018.punto_servicio where estado = 'Existente' and suministro != ' ' and alm IN " + feeder_ex);
            for (int i = 0; i < clients.Rows.Count; ++i)
            {
                Position p = ToLatLon(Convert.ToDouble(clients.Rows[i]["utm_x"]), Convert.ToDouble(clients.Rows[i]["utm_y"]), "19S");
                var geom = new Point(p);
                var props = new Dictionary<string, object>
                {
                    //{ "type", "client" },
                    { "feed", clients.Rows[i]["alm"].ToString() },
                    { "name", clients.Rows[i]["ckto"].ToString() },
                };
                var feature = new GeoJSON.Net.Feature.Feature(geom, props);
                client.Features.Add(feature);

                //labels
                Position p_lbl = ToLatLon(Convert.ToDouble(clients.Rows[i]["utm_s_x"]), Convert.ToDouble(clients.Rows[i]["utm_s_y"]), "19S");
                var geom_lbl = new Point(p_lbl);
                string sum = clients.Rows[i]["suministro"].ToString();
                sum = sum.Replace(" ", "\n");
                var props_lbl = new Dictionary<string, object>
                {
                    //{ "type", "client_lbl" },
                    { "feed", clients.Rows[i]["alm"].ToString() },
                    { "name", sum },
                };
                var feature_lbl = new GeoJSON.Net.Feature.Feature(geom_lbl, props_lbl);
                client_text.Features.Add(feature_lbl);
            }
            
            File.WriteAllText(@"C:\cygwin64\home\tony\giscada\lv_line.geojson", JsonConvert.SerializeObject(lv_line));
            File.WriteAllText(@"C:\cygwin64\home\tony\giscada\lv_service_line.geojson", JsonConvert.SerializeObject(lv_service_line));
            File.WriteAllText(@"C:\cygwin64\home\tony\giscada\substation.geojson", JsonConvert.SerializeObject(substation));
            File.WriteAllText(@"C:\cygwin64\home\tony\giscada\substation_text.geojson", JsonConvert.SerializeObject(substation_text));
            File.WriteAllText(@"C:\cygwin64\home\tony\giscada\mv_pole.geojson", JsonConvert.SerializeObject(mv_pole));
            File.WriteAllText(@"C:\cygwin64\home\tony\giscada\mv_pole_text.geojson", JsonConvert.SerializeObject(mv_pole_text));
            File.WriteAllText(@"C:\cygwin64\home\tony\giscada\lv_pole.geojson", JsonConvert.SerializeObject(lv_pole));
            File.WriteAllText(@"C:\cygwin64\home\tony\giscada\lv_pole_text.geojson", JsonConvert.SerializeObject(lv_pole_text));
            File.WriteAllText(@"C:\cygwin64\home\tony\giscada\client.geojson", JsonConvert.SerializeObject(client));
            File.WriteAllText(@"C:\cygwin64\home\tony\giscada\client_text.geojson", JsonConvert.SerializeObject(client_text));
        }

        public List<ShortBreaker> GetTouchedBreakers(Breaker b, ref List<ShortSub> shortsubs)
        {
            //HERE change type of variable for zenon variables values
            UpdateLineStatusByGraph(b);

            //her we should work with a hash where the key is the id substation and the value is its state 
            //Dictionary<int,int> subs = b.GetSubstationsAffected();
            Dictionary<int, int> subs = new Dictionary<int, int>();
            List<ShortBreaker> TouchedBreakers = new List<ShortBreaker>();
            foreach (Breaker br in breakerHash.Values)
            {
                if (br.lineStatus == 0)
                {   // just take the opened breakers
                    // just take the substations of openened breakers
                    GetSubstationsAffected(br, subs);
                    TouchedBreakers.Add(new ShortBreaker(br.name));
                }
            }
            shortsubs = GetShortSubs(subs);
            return TouchedBreakers;
        }

        public void GetSubstationsAffected(Breaker b,Dictionary<int, int> substationStateHash)
        {
            string[] sn = b.name.Split('_');
            string feeder = sn[0];
            // hash all the lines in feeder, codigotraa with the codigotra
            Dictionary<int, List<int>> lineHash = new Dictionary<int, List<int>>();
            DataTable lines = ExecuteSelectQuery("select * from tramo where codali = '" + feeder + "'");
            for (int i = 0; i < lines.Rows.Count; ++i)
            {
                int source = Int32.Parse(lines.Rows[i]["codigotraa"].ToString());
                int end = Int32.Parse(lines.Rows[i]["codigotra"].ToString());
                if (!lineHash.TryGetValue(source, out List<int> list))
                {
                    list = new List<int>();
                    lineHash.Add(source, list);
                }
                list.Add(end);
            }
            // hash all substation in feeder 
            Dictionary<int, int> subHash = new Dictionary<int, int>();
            DataTable sub = ExecuteSelectQuery("select * from sed where codali ='" + feeder + "' and codsed not like '%[-]%' ");
            for (int i = 0; i < sub.Rows.Count; ++i)
            {
                int lineId = Int32.Parse(sub.Rows[i]["nodo"].ToString());
                int subId = Int32.Parse(sub.Rows[i]["codsed"].ToString());
                subHash[lineId] = subId;
            }

            //below code is working
            //Stack<int> lineIds = new Stack<int>();
            //lineIds.Push(b.lineId);
            //while (lineIds.Count != 0)
            //{
            //    int current = lineIds.Pop();
            //    if (lineHash.TryGetValue(current, out List<int> nextLine))
            //    {
            //        foreach (int line in nextLine)
            //        {
            //            if (subHash.TryGetValue(line, out int subId))
            //            {
            //                ShortSub s = new ShortSub(subId.ToString());
            //                //subs.Add(s);
            //                substationStateHash[subId] = b.lineStatus;
            //            }
            //            lineIds.Push(line);
            //        }
            //    }
            //}

            //complete dfs
            Stack<LineStatus> lineStatusStack = new Stack<LineStatus>();
            lineStatusStack.Push(new LineStatus(b.lineId,b.lineStatus));
            while (lineStatusStack.Count != 0)
            {
                LineStatus current = lineStatusStack.Pop();
                if (lineHash.TryGetValue(current.lineId, out List<int> nextLine))
                {
                    foreach (int line in nextLine) {
                        if (subHash.TryGetValue(line, out int subId))
                        {
                            ShortSub s = new ShortSub(subId.ToString());
                            //subs.Add(s);
                            // this line is for aviding next updates from different breaker that affect the same subtation
                            if (!substationStateHash.ContainsKey(subId)) {
                                substationStateHash[subId] = current.lineStatus;
                            }
                        }
                        //if lineId is found take the linestatur from the current
                        //if lineId is found as a breaker take the linestatus from the breaker
                        int ls = current.lineStatus;
                        if (breakerHash.TryGetValue(line, out Breaker foundBreaker)) {
                            ls = foundBreaker.lineStatus;
                        }
                        lineStatusStack.Push(new LineStatus(line, ls));
                    }
                }
            }
            //return substationStateHash;
        }

        public List<ShortSub> GetShortSubs(Dictionary<int, int> subs) {
            List<ShortSub> shortSubs = new List<ShortSub>();
            foreach (int b in subs.Keys) {
                if (subs[b] == 0) {
                    shortSubs.Add(new ShortSub(b.ToString()));
                }
            }
            return shortSubs;
        }

        public List<ShortBreaker> GetInitialTouchedBreakers(ref List<ShortSub> shortsubs)
        {
            //her we should work with a hash where the key is the id substation and the value is its state 
            //Dictionary<int,int> subs = b.GetSubstationsAffected();
            Dictionary<int, int> subs = new Dictionary<int, int>();
            List<ShortBreaker> TouchedBreakers = new List<ShortBreaker>();
            foreach (Breaker br in breakerHash.Values)
            {
                if (br.lineStatus == 0)
                {   // just take the opened breakers
                    // just take the substations of openened breakers
                    GetSubstationsAffected(br, subs);
                    TouchedBreakers.Add(new ShortBreaker(br.name));
                }
            }
            shortsubs = GetShortSubs(subs);
            return TouchedBreakers;
        }

    }
}
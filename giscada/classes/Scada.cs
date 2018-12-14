using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

using System.Runtime.InteropServices;
using zenOn;

using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json; 

using System.IO;

using System.Data;
using System.Xml;

namespace giscada.classes
{
    public class Scada
    {
        private static zenOn.Project proj;
        private static FeederBreakers F = new FeederBreakers();
        private static VehicleTracker V = new VehicleTracker();
        private static ClaimTracker C = new ClaimTracker();

        private readonly static Lazy<Scada> _instance = new Lazy<Scada>(() => new Scada(GlobalHost.ConnectionManager.GetHubContext<ScadaHub>().Clients));
        private readonly ConcurrentDictionary<int, Variable> _zvariables = new ConcurrentDictionary<int, Variable>();

        //variables for scada
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(250);        
        private readonly Timer _timer;
        private readonly object _updateStatus = new object();
        private readonly object _checkStatus = new object();
        private volatile bool _updatingBreakerStatus = false;

        //variables for vehicles 
        private readonly TimeSpan _updateIntervalVehicle = TimeSpan.FromMilliseconds(2000);
        private readonly Timer _vehicletimer;
        private readonly object _checkStatusVehicle = new object();
        private volatile bool _updatingvehicleStatus = false;

        //variables for claims 
        private readonly TimeSpan _updateIntervalClaim = TimeSpan.FromMilliseconds(250);
        private readonly Timer _claimtimer;
        private readonly object _checkStatusClaim = new object();
        private volatile bool _updatingclaimStatus = false;


        private static List<ShortBreaker> ShortBreakers = new List<ShortBreaker>();


        public string Value { get; set; }

        public static Scada Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        private Scada(IHubConnectionContext<dynamic> clients)
        {
            try {
                Clients = clients;
                zenOn.Application zapp = null;
                Type acType = Type.GetTypeFromProgID("zenOn.Application");
                zapp = (zenOn.Application)Activator.CreateInstance(acType, true);
                //zapp = (zenOn.Application)Marshal.GetActiveObject("zenOn.Application");
                //zapp = (zenOn.Application)Marshal.GetActiveObject
                proj = zapp.Projects().Item(0);

                //initialize network
                F.proj = proj;
                F.Generate();
                //F.InfoForQGIS();

                //here we initialize the vehicles
                //V.InitializeVehicles();

                //initialize claims
                C.InitializeClaims();


                _timer = new Timer(CheckVariables, null, _updateInterval, _updateInterval);
                //_vehicletimer = new Timer(CheckVehicles, null, _updateIntervalVehicle, _updateIntervalVehicle);
                _claimtimer = new Timer(CheckClaims, null, _updateIntervalClaim, _updateIntervalClaim);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message + ex.InnerException?.Message);
                return;
            }
        }

        private void UpdateValues(object state)
        {
            lock (_updateStatus)
            {
                List<Variable> variableList = new List<Variable>();
                for (int i = 0; i < 65; ++i)
                {
                    Variable iv = _zvariables[i];
                    Variable var = new Variable
                    {
                        Name = iv.Name.ToString(),
                        Value = iv.ZenonVariable.get_Value(0),
                        ZenonVariable = iv.ZenonVariable
                    };
                    variableList.Add(var);
                }
                BroadcastVariableValue(variableList);
            }
        }

        private void BroadcastVariableValue(List<Variable> variableValues)
        {
            Clients.All.updateValues(variableValues);
        }

        private void CheckVariables(object state)
        {
            lock (_checkStatus)
            {
                if (!_updatingBreakerStatus)
                {
                    _updatingBreakerStatus = true;
                    foreach (Breaker b in F.breakerHash.Values)
                    {
                        var newValue = Int32.Parse(b.zenonVariable.get_Value(0).ToString()); ;
                        if (b.status != newValue)
                        {
                            // trigger updating 
                            b.status = newValue;
                            //Dictionary<int, int> subs = new Dictionary<int, int>();
                            List<ShortSub> subs = new List<ShortSub>();
                            // get the affected breakers and substations
                            //List<Breaker> breakers = F.GetTouchedBreakers(b);
                            List<ShortBreaker> breakers = F.GetTouchedBreakers(b, ref subs);
                            ShortBreakers = breakers;
                            if (breakers != null)
                            {
                                BroadcastColorLines(breakers);
                                //we should boadcast lv lines, maybe some o them are kept closed
                                BroadcastLvLines(subs);
                            }
                            // update  b.value (process at the end because it is usefull for updatecolorLines)
                            //it is necessary to create a method for Network
                            // this will work for every breaker 
                            //receive(Breaker)
                            //return List<Breaker>
                        }
                    }
                    _updatingBreakerStatus = false;
                }
            }
        }

        private void BroadcastColorLines(List<ShortBreaker> breakers)
        {
            Clients.All.updateColorLines(breakers);
        }

        private void BroadcastLvLines(List<ShortSub> shortSubs)
        {
            Clients.All.updateLvLines(shortSubs);
        }

        public List<ShortBreaker> GetInitialOpenedBreakers()
        {
            List<ShortSub> subs = new List<ShortSub>();
            List<ShortBreaker> breakers = F.GetInitialTouchedBreakers(ref subs);
            ShortBreakers = breakers;
            return ShortBreakers;
        }

        public List<ShortSub> GetInitialOpenedSubstations()
        {
            List<ShortSub> subs = new List<ShortSub>();
            List<ShortBreaker> breakers = F.GetInitialTouchedBreakers(ref subs);
            return subs;
        }
        
        public bool OpenBreaker(string name)
        {
            IVariable v = proj.Variables().Item(name);
            if (v.IsOnline())
            {
                v.set_Value(0, 0);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CloseBreaker(string name)
        {
            IVariable v = proj.Variables().Item(name);
            if (v.IsOnline())
            {
                v.set_Value(0, 1);
                return true;
            }
            else
            {
                return false;
            }
        }

        // vehicle functions

        private void CheckVehicles(object state)
        {
            lock (_checkStatusVehicle)
            {
                if (!_updatingvehicleStatus)
                {
                    _updatingvehicleStatus = true;
                    if (V.NewPositions())
                    {
                        BroadcastVehicles();
                    }
                    _updatingvehicleStatus = false;
                }
            }
        }

        private void BroadcastVehicles()
        {
            var model = new FeatureCollection();
            foreach (Vehicle v in V.vehicleHash.Values) {
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
            var vehiclejson = JsonConvert.SerializeObject(model);
            Clients.All.updateVehicles(vehiclejson);
        }

        public string GetInitialLayer()
        {
            return V.InitializeLayer();
        }

        // claim functions
        private void CheckClaims(object state)
        {
            lock (_checkStatusClaim)
            {
                if (!_updatingclaimStatus)
                {
                    _updatingclaimStatus = true;
                    if (C.CheckUpdates())
                    {
                        BroadcastClaims();
                    }
                    _updatingclaimStatus = false;
                }
            }  
        }

        private void BroadcastClaims() {
            Clients.All.updateClaims(C.GetCurrentValues());
        }

        public string GetInitialClaimLayer()
        {
            return C.GetCurrentValues();
        }
    }
}
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace giscada.classes
{
    [HubName("zenonVariableValues")]
    public class ScadaHub : Hub
    {
        private readonly Scada _zenonVariables;
        public ScadaHub() : this(Scada.Instance) { }

        public ScadaHub(Scada ZenonVariables)
        {
            _zenonVariables = ZenonVariables;
        }

        public string GetInitialLayer()
        {
            return _zenonVariables.GetInitialLayer();
        }

        public List<ShortBreaker> GetInitialOpenedBreakers()
        {
            return _zenonVariables.GetInitialOpenedBreakers();
        }

        public List<ShortSub> GetInitialOpenedSubstations()
        {
            return _zenonVariables.GetInitialOpenedSubstations();
        }

        //for breaker control
        public bool OpenBreaker(string name) {
            return _zenonVariables.OpenBreaker(name);
        }

        public bool CloseBreaker(string name)
        {
            return _zenonVariables.CloseBreaker(name);
        }
    }
}
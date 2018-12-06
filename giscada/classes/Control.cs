using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

using dlmsV0;

namespace giscada.classes
{
    
    public class Control : Hub
    {
        private readonly Scada _zenonVariables;
        public Control() : this(Scada.Instance) { }
        public Control(Scada ZenonVariables)
        {
            _zenonVariables = ZenonVariables;
        }

        //DlmsLayer T = new DlmsLayer("192.168.4.111", 22);

        //T.AssociationRequest();
    }
}
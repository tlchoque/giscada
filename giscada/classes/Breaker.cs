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
    public class Breaker : DataAccess
    {
        public Breaker parent;
        public LinkedList<Breaker> children;
        public string name;//when zenon scada is not active
        public Node node;
        public IVariable zenonVariable;
        public int status;//1-> closed, 0-> Opened, -1 not online
        public int lineStatus;

        public string shortName;
        public int lineId;

        public Breaker(string _name,int _lineId, IVariable _zenonVariable = null)
        {
            children = new LinkedList<Breaker>();
            name = _name;
            lineId = _lineId;

            string[] sn = _name.Split('_');
            if (sn.Count() > 1) shortName = sn[1];
            else shortName = sn[0];
            parent = null;

            zenonVariable = _zenonVariable;
            if (_zenonVariable == null || !_zenonVariable.IsOnline())
            {
                status = -1;
                lineStatus = -1;
            }
            else
            {
                name = _zenonVariable.Name;
                status = Int32.Parse(_zenonVariable.get_Value(0).ToString());
                InitializeLineStatus();
            }
        }
        public Breaker() { }

        public void InitializeLineStatus()
        {
            if (parent == null) lineStatus = status;
            else lineStatus = parent.lineStatus * status;
        }

    }

    public class ShortBreaker
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public ShortBreaker(string _name)
        {
            Name = _name;
        }
    }

    public class ShortSub {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public ShortSub(string _name)
        {
            Name = _name;
        }
    }

    public class LineStatus
    {
        public int lineId;
        public int lineStatus;
        public LineStatus(int _lineId, int _lineStatus)
        {
            lineId = _lineId;
            lineStatus = _lineStatus;
        }
    }
}
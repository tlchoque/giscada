using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zenOn;

namespace giscada.classes
{
    public class Variable
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private object _value;
        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private IVariable _zenonVariable;
        public IVariable ZenonVariable
        {
            get { return _zenonVariable; }
            set { _zenonVariable = value; }
        }
    }
}
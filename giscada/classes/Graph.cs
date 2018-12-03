using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace giscada.classes
{
    public class Graph<T>
    {
        public T head;
        public Dictionary<int, T> nodeHash;
        public string name;
        public Graph<Breaker> breakerGraph;
        public Graph(string _name=null)
        {
            name = _name;
            nodeHash = new Dictionary<int, T>();
        }
    }
}
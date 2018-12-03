using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using zenOn;


namespace giscada.classes
{
    public class Node
    {
        public Position position;
        public bool isBreaker;
        public Node parent;
        public LinkedList<Node> children;
        public Breaker breaker;
        public Node(Position _position, bool _isBreaker)
        {
            position = _position;
            isBreaker = _isBreaker;
            children = new LinkedList<Node>();
            parent = null;
        }
        public Node() { }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend
{
    public class Graph 
    {
        public /*IReadOnly*/List<GraphNode> Nodes 
        {
            get => _nodes/*.AsReadOnly()*/;
        }
        public /*IReadOnly*/List<GraphEdge> Edges 
        {
            get => _edges/*.AsReadOnly()*/;
        }

        private List<GraphNode> _nodes = new List<GraphNode>();
        private List<GraphEdge> _edges = new List<GraphEdge>();

        private string _uri;
        private string _username;
        private string _password;

        public Graph(string uri = "neo4j://cloud-vm-42-36.doc.ic.ac.uk:7687", string username = "neo4j", string password = "s3cr3t")
        {
            _uri = uri;
            _username = username;
            _password = password;

            using (var database = new DatabaseView(_uri, _username, _password))
            {
                _nodes = database.AllNodes();
                _edges = database.AllEdges(_nodes);
            }

            foreach (var edge in _edges)
                edge.Parent.AddEdge(edge);
        }
    }
}
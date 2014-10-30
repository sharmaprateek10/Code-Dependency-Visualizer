using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GraphVisualization
{
    class Graph
    {
        private Dictionary<string, HashSet<string>> adjList = new Dictionary<string, HashSet<string>>();
        private Dictionary<string, HashSet<string>> undirectedGraph = new Dictionary<string, HashSet<string>>();
        public Graph()
        {

        }

        public Dictionary<string, HashSet<string>> UndirectedGraph
        {
            get { return undirectedGraph; }
        }
        public List<string> VertexList
        {
            get { return adjList.Keys.ToList(); }
        }

        public void addVertex(string name)
        {
            if (!adjList.ContainsKey(name))
            {
                adjList.Add(name, new HashSet<string>());
            }

            if (!undirectedGraph.ContainsKey(name))
            {
                undirectedGraph.Add(name, new HashSet<string>());
            }
        }

        public void addNeighbor(string source, string target)
        {
            adjList[source].Add(target);
            undirectedGraph[source].Add(target);
            HashSet<string> set = null;
            if (!adjList.ContainsKey(target))
            {
                set = new HashSet<string>();
                adjList.Add(target, set);
            }
            else
            {
                adjList[target].Add(source);
            }
        }

        public HashSet<string> getNeighbors(string vertexName)
        {
            return adjList[vertexName];
        }
    }
}

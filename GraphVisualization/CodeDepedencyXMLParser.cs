using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Xml.Linq;

namespace GraphVisualization
{
    class CodeDepedencyXMLParser
    {
        private string fileName;
        private Graph visualizationGraph;
        public CodeDepedencyXMLParser(string fileName)
        {
            this.fileName = fileName;
            visualizationGraph = new Graph();
        }

        public Graph VisualGraph
        {
            get { return this.visualizationGraph; }
        }
        public void parseXML()
        {
            if (File.Exists(fileName))
            {
                XDocument document = XDocument.Load(fileName);
                var dependencies = from dependency in document.Descendants("dependencies").Elements()
                                   select dependency;
                foreach (XElement dependencyValue in dependencies)
                {
                    string fileSource = dependencyValue.Attribute("file_name").Value;
                    string fileDependencies = dependencyValue.Attribute("depends_on").Value;
                    visualizationGraph.addVertex(fileSource);
                    if (fileDependencies.Length > 0)
                    {
                        string[] fileTargets = fileDependencies.Split(',');
                        foreach (string target in fileTargets)
                        {
                            visualizationGraph.addNeighbor(fileSource, target);
                        }
                    }
                }
            }
        }
    }
}

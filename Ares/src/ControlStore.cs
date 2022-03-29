using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Ares.Core
{
    internal class ControlStore : IEnumerable<ControlPoint>
    {
        private Dictionary<int, ControlPoint> _controlDict;
        private int _scale;
        private string _filePath;

        public int Count => _controlDict.Count;
        public ControlPoint this[int id] => _controlDict[id];
        
        public ControlStore(string filePath)
        {
            XmlDocument doc = new();
            doc.Load(filePath);

            _filePath = filePath;

            XmlNode eventDetails = doc.FirstChild.FirstChild;

            _scale = Convert.ToInt32(eventDetails.ChildNodes[3].Attributes[0].Value);

            for (int i = 1; i < doc.FirstChild.ChildNodes.Count; i++)
            {
                XmlNode node = doc.FirstChild.ChildNodes[i];
                if (node.Name == "control")
                {
                    ControlPoint c = new ControlPoint(node);

                    _controlDict.Add(c.ID, c);
                }

            }
        }

        public IEnumerator<ControlPoint> GetEnumerator()
        {
            foreach (KeyValuePair<int, ControlPoint> c in _controlDict)
                yield return c.Value;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(int id)
        {
            return _controlDict.ContainsKey(id);
        }


        public float DistanceBetweenControls(ControlPoint a, ControlPoint b)
        {
            return DistBtwnCtrls(a, b) * (float)(_scale / 1000f);
        }
        float DistBtwnCtrls(ControlPoint a, ControlPoint b)
        {
            return (float)Math.Sqrt(Math.Pow(b.Pos.X - a.Pos.X, 2) + Math.Pow(b.Pos.Y - a.Pos.Y, 2));
        }

        public bool SaveCourses(List<Course> courses, string filePath)
        {
            try
            {
                CreateXml(courses, filePath);
                return true;
            } catch { return false; }
        }

        void CreateXml(List<Course> courses, string filePath)
        {

        }
    } 
}

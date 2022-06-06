using System.Collections;
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
            _controlDict = new();
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
            List<string> courseStr = new();
            string controlStr = "";
            int cControlID = 1;
            int courseID = 1;
            
            foreach (Course c in courses)
            {
                string title = "";
                courseStr.Add(CourseStr(courseID, cControlID, _scale, new() { title }));
                courseID++;
                
                for (int i = 0; i < c.Count; i++)
                {
                    if (i == c.Count - 1)
                        controlStr += EndControlStr(cControlID, c[i].ID);
                    else
                        controlStr += ControlStr(cControlID, cControlID + 1, c[i].ID);

                    cControlID++;
                }
            }

            string text = File.ReadAllText(_filePath);

            int loc = text.IndexOf("<course id=");
            text = text.Substring(0, loc);

            foreach (string s in courseStr)
                text += s;

            text += controlStr;
            text += "</course-scribe-event>";

            File.WriteAllText(filePath, text);
        }
        
        string ControlStr(int c1, int c2, int id)
        {
            return "<course-control id=\"" + c1.ToString() + "\" control=\"" + id.ToString() + "\">"
                + Environment.NewLine
                + "<next course-control=\"" + c2.ToString() + "\" />"
                + Environment.NewLine
                + "</course-control>"
                + Environment.NewLine;
        }
        string EndControlStr(int c1, int id)
        {
            return "<course-control id=\"" + c1.ToString() + "\" control=\"" + id.ToString() + "\" />"
                + Environment.NewLine;
        }
        string CourseStr(int id, int c1, int scale, List<string> data)
        {
            string s = "";
            foreach (string d in data)
            { s += d + "|"; }
            try
            { s = s.Substring(0, s.Length - 1); }
            catch { }


            return
                "<course id=\"" + id.ToString() + "\" kind=\"normal\" order=\"" + id.ToString() + "\">"
                + Environment.NewLine
                + "<name>Course " + id.ToString() + "</name>"
                + Environment.NewLine
                + "<secondary-title>" + s + "</secondary-title>"
                + Environment.NewLine
                + "<labels label-kind=\"sequence\" />"
                + Environment.NewLine
                + "<first course-control=\"" + c1.ToString() + "\" />"
                + Environment.NewLine
                + "<print-area automatic=\"true\" restrict-to-page-size=\"true\" "
                + "left=\"16.7639771\" top=\"219.328979\" right=\"313.689972\" bottom=\"9.270966\" page-width=\"827\" page-height=\"1169\" page-margins=\"0\" page-landscape=\"true\" />"
                + Environment.NewLine
                + "<options print-scale=\"" + scale.ToString() + "\" description-kind=\"symbols\" />"
                + Environment.NewLine
                + "</course>"
                + Environment.NewLine;
        }
    } 
}

using System.Drawing;
using System.Xml;

namespace Ares.Core
{
    internal class ControlPoint
    {
        private int _id;
        private int _code;
        private PointF _pos;
        private ControlPointType _type;

        public int ID => _id;
        public int Code => _code;
        public PointF Pos => _pos;
        public ControlPointType Type => _type;

        public ControlPoint(XmlNode node)
        {
            string id = node.Attributes[0].Value;
            string type = node.Attributes[1].Value;
            switch (type.ToLower())
            {
                case "normal": _type = ControlPointType.Normal; break;
                case "start": _type = ControlPointType.Start; break;
                case "finish": _type = ControlPointType.Finish; break;
                default: throw new ArgumentException("Unrecognised Control Type");
            }


            string code;
            string x, y;
            if (_type == 0)
            {
                code = node.ChildNodes[0].InnerText;
                x = node.ChildNodes[1].Attributes[0].Value;
                y = node.ChildNodes[1].Attributes[1].Value;
            }
            else
            {
                code = "-1";
                x = node.ChildNodes[0].Attributes[0].Value;
                y = node.ChildNodes[0].Attributes[1].Value;

            }

            _id = Convert.ToInt32(id);
            _code = Convert.ToInt32(code);
            _pos = new PointF(
                (float)Convert.ToDouble(x),
                (float)Convert.ToDouble(y));

        }

        public ControlPoint()
        {
            _id = -1;
            _code = -1;
        }
    }

    internal enum ControlPointType { Normal, Start, Finish }
}

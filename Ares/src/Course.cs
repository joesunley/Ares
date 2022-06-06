using System.Collections;

namespace Ares.Core
{
    internal class Course : IEnumerable<ControlPoint>
    {
        private List<ControlPoint> _controls;

        public int Count => _controls.Count;

        public Course(List<ControlPoint>? controls = null)
        {
            _controls = controls ?? new();
        }

        public ControlPoint this[int index] => _controls[index];
        public IEnumerator<ControlPoint> GetEnumerator()
        {
            foreach (ControlPoint c in _controls)
                yield return c;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public float Length(ControlStore store)
        {
            float len = 0;

            for (int i = 1; i < _controls.Count; i++)
                len += store.DistanceBetweenControls(_controls[i - 1], _controls[i]);

            return len;
        }

        public void Add(ControlPoint control) => _controls.Add(control);

        public override string ToString()
        {
            string str = "";

            foreach (ControlPoint c in _controls)
                str += "," + c.ID.ToString();

            return str.Substring(1);
        }
    }
}

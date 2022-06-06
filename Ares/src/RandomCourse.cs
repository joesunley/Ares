using System.Drawing;

namespace Ares.Core
{
    internal class RandomCourse
    {
        private Course _course;

        private ControlStore _controlStore;
        private int _courseLen;
        private Random _random;
        private int _angleTolerance;
        private float _randomControlCutoff;
        private int _lastControlMaxDistance;
        private float _densityCutOff;
        private LegLengths _legLengths;
        private LegProbabilities _legProbabilities;

        public RandomCourse(RandomCourseBuilder builder, Random rand)
        {
            _course = new();
            _random = rand;
            
            _controlStore = builder.ControlStore;
            _courseLen = builder.CourseLen;
            _angleTolerance = builder.AngleTolerance;
            _randomControlCutoff = builder.RandomControlCutoff;
            _lastControlMaxDistance = builder.LastControlMaxDistance;
            _densityCutOff = builder.DensityCutOff;
            _legLengths = builder.LegLengths;
            _legProbabilities = builder.LegProbabilities;
        }

        public Course CreateCourse()
        {
            CreateRndCourse();

            return _course;
        }

        private void CreateRndCourse()
        {
            try
            {
                List<ControlPoint> valid = new();

                foreach (ControlPoint c in _controlStore)
                    if (c.Type == ControlPointType.Start)
                        valid.Add(c);

                _course.Add(valid[_random.Next(valid.Count)]);
            } 
            catch { return; }

            try
            {
                LegLength l = new(0, _legLengths.ShortMax);
                List<ControlPoint> valid = new();

                foreach (ControlPoint c in _controlStore)
                {
                    float legDist = _controlStore.DistanceBetweenControls(c, _course.Last());

                    if (legDist >= l.Minimum && legDist <= l.Maximum
                            && c.Type == ControlPointType.Normal)
                        valid.Add(c);
                }

                if (valid.Count != 0)
                    _course.Add(valid[_random.Next(valid.Count)]);
            } 
            catch { return; }

            float courseLen = _controlStore.DistanceBetweenControls(_course[0], _course[1]);

            while (courseLen < (_courseLen * _randomControlCutoff))
            {
                List<ControlPoint> valid = ChooseValidControls();

                if (valid.Count != 0)
                    _course.Add(valid[_random.Next(valid.Count)]);
                else
                {
                    // Choose Nearest Control

                    ControlPoint nearest = new();
                    float dist = float.MaxValue;

                    foreach (ControlPoint c in _controlStore)
                    {
                        float legDist = _controlStore.DistanceBetweenControls(_course.Last(), c);

                        if (legDist < dist && c.Type == ControlPointType.Normal && !_course.Contains(c))
                        {
                            nearest = c;
                            dist = legDist;
                        }
                    }

                    _course.Add(nearest);
                }

                float legLen = _controlStore.DistanceBetweenControls(_course[_course.Count - 2], _course.Last());
                courseLen += legLen;
            }

            ControlPoint finish;

            try
            {
                List<ControlPoint> valid = new();

                foreach (ControlPoint c in _controlStore)
                    if (c.Type == ControlPointType.Finish)
                        valid.Add(c);

                finish = valid[_random.Next(valid.Count)];
            }
            catch { return; }

            bool cont = true;
            while (cont)
            {
                List<ControlPoint> valid = ChooseValidControls();

                if (valid.Count != 0)
                {
                    int count;
                    if (valid.Count >= 10)
                        count = 10;
                    else
                        count = Convert.ToInt32(Math.Ceiling(valid.Count / 2.0));

                    List<ControlPoint> chosen = new();

                    for (int i = 0; i < count; i++)
                    {
                        int rnd = _random.Next(valid.Count);

                        chosen.Add(valid[rnd]);
                        valid.RemoveAt(rnd);
                    }

                    ControlPoint direct = new();
                    double angle = 0;

                    foreach (ControlPoint c in chosen)
                    {
                        double ang = AngleBetweenThree(_course.Last(), c, finish);
                        if (ang > 180)
                            ang = 360 - ang;

                        if (ang > angle)
                        {
                            angle = ang;
                            direct = c;
                        }
                    }

                    _course.Add(direct);
                }
                else
                {
                    ControlPoint nearest = new();
                    float dist = float.MaxValue;

                    foreach (ControlPoint c in _controlStore)
                    {
                        float legDist = _controlStore.DistanceBetweenControls(_course.Last(), c);

                        if (legDist < dist && !_course.Contains(c))
                        {
                            nearest = c;
                            dist = legDist;
                        }
                    }

                    _course.Add(nearest);
                }

                float distt = _controlStore.DistanceBetweenControls(_course.Last(), finish);
                if (distt <= _lastControlMaxDistance)
                    cont = false;

                if (_course.Length(_controlStore) > courseLen + 300)
                    return;
            }

            _course.Add(finish);
        }

        private List<ControlPoint> ChooseValidControls()
        {
            LegLength legLen = ChooseLegLengths();
            List<ControlPoint> valid = new();

            foreach (ControlPoint c in _controlStore)
            {
                if (c.Type == ControlPointType.Normal && !_course.Contains(c))
                {
                    float legDist = _controlStore.DistanceBetweenControls(c, _course.Last());

                    if (legDist >= legLen.Minimum && legDist <= legLen.Maximum)
                    {
                        double angle = AngleBetweenThree(_course[_course.Count - 2], _course[_course.Count - 1], c);

                        if (angle >= (180 - _angleTolerance) && angle <= (180 + _angleTolerance))
                            valid.Add(c);
                    }
                }
                
            }

            return valid;
        }

        private LegLength ChooseLegLengths()
        {
            float rnd = (float)_random.NextDouble();

            if (rnd <= _legProbabilities.VeryShort)
                return new(0, _legLengths.VeryShortMax);
            else if (rnd <= _legProbabilities.ShortSum())
                return new(_legLengths.VeryShortMax, _legLengths.ShortMax);
            else if (rnd <= _legProbabilities.MediumSum())
                return new(_legLengths.ShortMax, _legLengths.MediumMax);
            else
                return new(_legLengths.MediumMax, int.MaxValue);
        }

        private double AngleBetweenThree(ControlPoint a, ControlPoint b, ControlPoint c)
        {
            double 
                ab = DistBtwnPoints(a.Pos, b.Pos),
                bc = DistBtwnPoints(b.Pos, c.Pos),
                ac = DistBtwnPoints(a.Pos, c.Pos);
            
            double
                top = (ab * ab) + (bc * bc) - (ac * ac),
                bottom = 2 * ab * bc;

            return Math.Acos(top / bottom) * (180 / Math.PI);
        }

        private double DistBtwnPoints(PointF a, PointF b)
            => Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));


        
    }

    internal class RandomCourseBuilder
    {
        public ControlStore ControlStore { get; private set; }
        public int CourseLen { get; private set; }
        public int AngleTolerance { get; private set; }
        public float RandomControlCutoff { get; private set; }
        public int LastControlMaxDistance { get; private set; }
        public float DensityCutOff { get; private set; }
        public LegLengths LegLengths { get; private set; }
        public LegProbabilities LegProbabilities { get; private set; }

        public RandomCourseBuilder(ControlStore controlStore, int courseLength)
        {
            ControlStore = controlStore;
            CourseLen = courseLength;

            AngleTolerance = 75;
            RandomControlCutoff = 0.7f;
            LastControlMaxDistance = 200;
            DensityCutOff = 1f;
            LegLengths = new(250, 600, 1100);
            LegProbabilities = new(0.2f, 0.4f, 0.3f, 0.1f);
        }

        
        public RandomCourseBuilder SetAngleTolerance(int value)
        {
            AngleTolerance = value;
            return this;
        }
        public RandomCourseBuilder SetRandomControlCutOff(float value)
        {
            RandomControlCutoff = value;
            return this;
        }
        public RandomCourseBuilder SetLastControlMaxDistance(int value)
        {
            LastControlMaxDistance = value;
            return this;
        }
        public RandomCourseBuilder SetDensityCutOff(float value)
        {
            DensityCutOff = value;
            return this;
        }
        public RandomCourseBuilder SetLegLengths(LegLengths value)
        {
            LegLengths = value;
            return this;
        }
        public RandomCourseBuilder SetLegProbabilities(LegProbabilities value)
        {
            LegProbabilities = value;
            return this;
        }
    }
}

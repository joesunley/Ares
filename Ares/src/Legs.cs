namespace Ares.Core
{
    internal record LegLengths(int VeryShortMax, int ShortMax, int MediumMax);
    internal record LegProbabilities(float VeryShort, float Short, float Medium, float Long)
    {
        public float ShortSum() => VeryShort + Short;
        public float MediumSum() => VeryShort + Short + Medium;
    }
    internal record LegLength(int Minimum, int Maximum);
}

namespace Ares
{
    using System.Collections.Concurrent;
    using Core;
    using System.Threading;
    public class Program
    {
        static string filePath = @"";
        static ConcurrentQueue<Course> courses = new();
        static ControlStore store;
        static RandomCourseBuilder builder;
        static Random random;
        static int courseCount, threadCount;
        static int totalCount = 0;
        static int controlCount = -1;
        
        public static void Main()
        {
            random = new();

            Input();
            Start();
            Save();
        }
        
        static void Start()
        {
            List<Thread> threads = new();

            for (int i = 0; i < threadCount; i++)
            {
                threads.Add(new Thread(() => ThreadRunner()));
                threads.Last().Start();
            }
        }
        
        static void ThreadRunner()
        {
            int valid = 0;
            
            while (valid < (courseCount / threadCount))
            {
                Course c = new RandomCourse(builder, random).CreateCourse();

                if (Validate(c))
                {
                    valid++;
                    courses.Enqueue(c);
                }
                totalCount++;

                Console.WriteLine($"{totalCount}\t{courses.Count}\t{c.Length(store).ToString("F0")}\t{c.Count}");
            }
        }

        static bool Validate(Course c)
        {
            if (!(c.Length(store) >= builder.CourseLen - 250
                && c.Length(store) <= builder.CourseLen + 250))
                return false;

            if (controlCount != -1 && c.Count != controlCount + 2)
                return false;

            if (c.Last().Type != ControlPointType.Finish)
                return false;

            return true;
        }
        
        static void Input()
        {
            Console.Write("Enter path to PurplePen file: ");
            filePath = Console.ReadLine() ?? "";

            Console.Write("Enter the course length: ");
            int len = int.Parse(Console.ReadLine() ?? "7000");

            store = new(filePath);
            builder = new(store, len);
            
            
            Console.Write("Do you want to limit the number of controls (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                Console.Write("Enter the number of controls: ");
                controlCount = int.Parse(Console.ReadLine() ?? "-1");
            }

            Console.Write("Do you want to modify advanced details (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                Console.Clear();

                Console.WriteLine("Leave the following blank for the default value\n");

                Console.Write("Set the Angle Tolerance (default: 75): ");
                builder.SetAngleTolerance(int.Parse(Console.ReadLine() ?? "75"));

                Console.Write("Set the Random Control Cut Off (default: 0.7): ");
                builder.SetRandomControlCutOff(float.Parse(Console.ReadLine() ?? "0.7"));

                Console.Write("Set the Maximum Density Value (default: 1): ");
                builder.SetDensityCutOff(float.Parse(Console.ReadLine() ?? "1"));

                Console.Write("Do you want to modify the Leg Length Values (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    Console.Clear();

                    Console.WriteLine("Leave the following blank for the default value\n");

                    Console.Write("Set the VeryShort Leg Length Maximum (default: 250): ");
                    int vShort = int.Parse(Console.ReadLine() ?? "250");

                    Console.Write("Set the Short Leg Length Maximum (default: 600): ");
                    int shor = int.Parse(Console.ReadLine() ?? "600");

                    Console.Write("Set the Medium Leg Length Maximum (default: 1200): ");
                    int med = int.Parse(Console.ReadLine() ?? "1200");

                    builder.SetLegLengths(new(vShort, shor, med));
                }

                Console.Write("Do you want to modify the Leg Probabilities (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    Console.Clear();

                    Console.WriteLine("Leave the following blank for the default value\n");

                    Console.Write("Set the VeryShort Leg Probability (default: 0.2): ");
                    float vShort = float.Parse(Console.ReadLine() ?? "0.2");

                    Console.Write("Set the Short Leg Probability (default: 0.4): ");
                    float shor = float.Parse(Console.ReadLine() ?? "0.4");

                    Console.Write("Set the Medium Leg Probability (default: 0.3): ");
                    float med = float.Parse(Console.ReadLine() ?? "0.3");

                    Console.Write("Set the Long Leg Probability (default: 0.1): ");
                    float lon = float.Parse(Console.ReadLine() ?? "0.1");

                    builder.SetLegProbabilities(new(vShort, shor, med, lon));
                }
                
                Console.Clear();
            }


            Console.Write("Enter the number of courses to generate: ");
            courseCount = int.Parse(Console.ReadLine() ?? "1");

            Console.Write("Enter the number of threads to use: ");
            threadCount = int.Parse(Console.ReadLine() ?? "2");
        }
        
        static void Save()
        {
            Console.ReadLine();
            Console.Clear();

            Console.Write("Enter the output file path: ");
            string path = Console.ReadLine() ?? "save.ppen";

            Console.WriteLine(store.SaveCourses(courses.ToList(), path));
        }
    }
}
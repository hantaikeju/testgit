using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareTest
{
    public class Test : IComparable<Test>
    {
        public int x { get; set; }

        public Test() { }
        public Test(int x)
        {
            this.x = x;
        }

        public int CompareTo(object obj)
        {
            Test test = obj as Test;
            if (test != null)
                return CompareTo(test);
            throw
                new ArgumentException(
                    "Both objects being copared must be of type Test.");

        }


        public int CompareTo(Test other)
        {

            if (x == other.x)
                return 0;
            else if (x > other.x)
                return 1;
            else if (x < other.x)
                return -1;
            else
                return -1;
        }
    }

    public class Test2 : IComparer<Test>
    {
        public int Compare(object firstTest, object secondTest)
        {
            Test test1 = firstTest as Test;
            Test test2 = secondTest as Test;
            if (test1 == null || test2 == null)
                throw (new ArgumentException("Both parameters must be of type Test"));
            else
                return Compare(firstTest, secondTest);

        }

        public int Compare(Test a, Test b)
        {
            if (a.x == b.x)
                return 0;
            else if (a.x > b.x)
                return 1;
            else if (a.x < b.x)
                return -1;
            else
                return -1;
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            List<Test> listOfTest = new List<Test>() {

                new Test(2),
            new Test(1),
            new Test(3)
            };

            foreach (Test test in listOfTest)
            {
                Console.Write(test.ToString());
            }

            IComparer<Test> test2Compare = new Test2();
            listOfTest.Sort(test2Compare);
            foreach (Test test in listOfTest)
            { Console.WriteLine(test.ToString()); }

            listOfTest.Sort();

            var sortedListOfTest = new SortedList<int, Test>() {

                {0,new Test(2) },
                {1,new Test(3) }


            };
            foreach (KeyValuePair<int, Test> kvp in sortedListOfTest)
            {
                Console.WriteLine($"{kvp.Key}:{kvp.Value}");
            }
        }
    }
}

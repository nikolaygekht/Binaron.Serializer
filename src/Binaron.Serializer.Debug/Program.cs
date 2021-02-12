using Binaron.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinSerializerTest
{
    class Program
    {
        class TestObject
        {
            public int A { get; set; }
            public TestObject()
            {

            }

            public TestObject(int a)
            {
                A = a; 
            }
        }

        class TestObject1
        {
            public int? A { get; set; }
            
            public TestObject1()
            {

            }

            public TestObject1(int? a)
            {
                A = a;
            }
        }

        enum E1
        {
            E11,
            E12,
            E13
        };

        class Collector<T> : IEnumerable<T>
        {
            private List<T> mData = new List<T>();

            public T this[int index] => mData[index];
            public int Count => mData.Count;

            public void Add(T element) => mData.Add(element);

            public IEnumerator<T> GetEnumerator() => mData.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => mData.GetEnumerator();
        }

        public static void Main(string[] args)
        {
            if (false)
                Benchmark();

            if (false)
            {
                Collector<TestObject> c = new Collector<TestObject>() { new TestObject(1), new TestObject(2) };
                using (var ms1 = new MemoryStream())
                {
                    BinaronConvert.Serialize(c, ms1);
                    using (var ms2 = new MemoryStream(ms1.ToArray()))
                    {
                        var cr = BinaronConvert.Deserialize<Collector<TestObject>>(ms2);
                        ;
                    }
                }
            }

            if (true)
            {
                Collector<TestObject1> c = new Collector<TestObject1>() { new TestObject1(1), null, new TestObject1(null), new TestObject1(2) };
                using (var ms1 = new MemoryStream())
                {
                    BinaronConvert.Serialize(c, ms1);
                    using (var ms2 = new MemoryStream(ms1.ToArray()))
                    {
                        var cr = BinaronConvert.Deserialize<Collector<TestObject1>>(ms2);
                        ;
                    }
                }
            }

            if (false)
            {
                Collector<E1> c1 = new Collector<E1>() { E1.E11, E1.E12, E1.E13 };
                using (var ms1 = new MemoryStream())
                {
                    BinaronConvert.Serialize(c1, ms1);
                    using (var ms2 = new MemoryStream(ms1.ToArray()))
                    {
                        var cr1 = BinaronConvert.Deserialize<Collector<E1>>(ms2);
                        ;
                    }
                }
            }

            if (false)
            {
                List<TestObject> l = new List<TestObject>() { new TestObject(1), null, new TestObject(2) };
                using (var ms1 = new MemoryStream())
                {
                    BinaronConvert.Serialize(l, ms1);
                    using (var ms2 = new MemoryStream(ms1.ToArray()))
                    {
                        var l1 = BinaronConvert.Deserialize<List<TestObject>>(ms2);
                        ;
                    }
                }
            }

            if (false)
            {
                List<int?> c2 = new List<int?>() { 1, null, 2 };
                using (var ms1 = new MemoryStream())
                {
                    BinaronConvert.Serialize(c2, ms1);
                    using (var ms2 = new MemoryStream(ms1.ToArray()))
                    {
                        var cr2 = BinaronConvert.Deserialize<List<int?>>(ms2);
                        ;
                    }
                }
            }

            if (true)
            {
                Collector<int?> c2 = new Collector<int?>() { 1, null, 2 };
                using (var ms1 = new MemoryStream())
                {
                    BinaronConvert.Serialize(c2, ms1);
                    using (var ms2 = new MemoryStream(ms1.ToArray()))
                    {
                        var cr2 = BinaronConvert.Deserialize<Collector<int?>>(ms2);
                        ;
                    }

                    using (var ms2 = new MemoryStream(ms1.ToArray()))
                    {
                        var cr2 = BinaronConvert.Deserialize(ms2);
                        ;
                    }
                }
            }

            if (false)
            {
                using (var ms1 = new MemoryStream())
                {
                    BinaronConvert.Serialize<int?>(1, ms1);
                    using (var ms2 = new MemoryStream(ms1.ToArray()))
                    {
                        var cr2 = BinaronConvert.Deserialize<int?>(ms2);
                        ;
                    }
                }

                using (var ms1 = new MemoryStream())
                {
                    BinaronConvert.Serialize<int?>(null, ms1);
                    using (var ms2 = new MemoryStream(ms1.ToArray()))
                    {
                        var cr2 = BinaronConvert.Deserialize<int?>(ms2);
                        ;
                    }
                }
            }




        }

        private static void Benchmark()
        {
            Stopwatch sw = new Stopwatch();

            {
                sw.Reset();
                Collector<int> c = new Collector<int>();
                for (int i = 0; i < 1000; i++)
                    c.Add(i);
                sw.Start();
                for (int i = 0; i < 1000; i++)
                {
                    using (var ms1 = new MemoryStream())
                    {
                        BinaronConvert.Serialize(c, ms1);
                        using (var ms2 = new MemoryStream(ms1.ToArray()))
                        {
                            var c2 = BinaronConvert.Deserialize<Collector<int>>(ms2);
                        }
                    }
                }
                sw.Stop();
                Console.WriteLine("IEnumerable/invoke {0}", sw.ElapsedMilliseconds);
            }

            {
                sw.Reset();
                Collector<short> c = new Collector<short>();
                for (int i = 0; i < 1000; i++)
                    c.Add((short)i);
                sw.Start();
                for (int i = 0; i < 1000; i++)
                {
                    using (var ms1 = new MemoryStream())
                    {
                        BinaronConvert.Serialize(c, ms1);
                        using (var ms2 = new MemoryStream(ms1.ToArray()))
                        {
                            var c2 = BinaronConvert.Deserialize<Collector<short>>(ms2);
                        }
                    }
                }
                sw.Stop();
                Console.WriteLine("IEnumerable/reflection {0}", sw.ElapsedMilliseconds);
            }

            {
                sw.Reset();
                List<int> c = new List<int>();
                for (int i = 0; i < 1000; i++)
                    c.Add(i);
                sw.Start();
                for (int i = 0; i < 1000; i++)
                {
                    using (var ms1 = new MemoryStream())
                    {
                        BinaronConvert.Serialize(c, ms1);
                        using (var ms2 = new MemoryStream(ms1.ToArray()))
                        {
                            var c2 = BinaronConvert.Deserialize<List<int>>(ms2);
                        }
                    }
                }
                sw.Stop();
                Console.WriteLine("List {0}", sw.ElapsedMilliseconds);
            }




        }
    }
}

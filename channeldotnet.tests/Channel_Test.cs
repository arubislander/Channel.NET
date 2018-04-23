using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ArubIslander.Collections.Generic.Tests
{
    [TestFixture]
    public class Channel_Test
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(10)]
        public void Test_Retention(int capacity)
        {
            Channel<int> ch = new Channel<int>(capacity);
            for (int i=0; i<capacity; i++) {
                ch.Put(i);
            }
            Assert.That( ch.Count == capacity );
        }

        [TestCase(10)]
        public void Test_Iteration(int capacity)
        {
            Channel<int> ch = new Channel<int>(capacity);
            for (int i=0; i<capacity; i++) {
                ch.Put(i);
            }
            ch.Close();

            int counter=0;
            foreach (int a in ch) {
                Assert.That(a == counter);
                counter++;
            }
        }

        [TestCase(50, 5)]
        public void Test_Concurency(int maxitems, int taskcount)
        {
            int capacity = maxitems / taskcount;
            Channel<int> ch = new Channel<int>(capacity);
            List<Task> tasks = new List<Task>(taskcount);

            // Setup Consuming tasks
            for (int i=0; i<taskcount; i++) {
                tasks.Add(Task.Run( () => {
                    foreach (int a in ch) {
                        System.Diagnostics.Debug.WriteLine(a);
                    }
                }));
            }
            
            // run producing loop
            for (int i=0; i<maxitems; i++) {
                ch.Put(i);
            }
            ch.Close();

            Assert.That(Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(taskcount)));
        }
    }
}
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalGo.Accessibilities;
using System.Threading;
using System.Diagnostics;

namespace SignalGoTest.Utilities
{
    [TestClass]
    public class FactoryTest
    {
        [TestMethod]
        public void TestDataFactorySingleToneByThread()
        {
            var data = new Random();
            if (!DataFactory.SetSingleToneByThread(data))
                Assert.Fail("set singletone not work");
            var otherdata = new Random();
            var takeData = DataFactory.GetSingleToneByThread<Random>();
            Assert.IsTrue(takeData == data && takeData != otherdata);
        }

        [TestMethod]
        public void TestDataFactorySingleToneByThreadMultiThreading()
        {
            int finished = 0;
            bool isOk = true;
            for (int i = 0; i < 100; i++)
            {
                string name = "thread" + i;
                Thread thread1 = new Thread(() =>
                {
                    Thread.Sleep(1000);
                    var data = new Tuple<string>(name);
                    if (!DataFactory.SetSingleToneByThread(data))
                        Assert.Fail("set singletone not work");
                    var takeData = DataFactory.GetSingleToneByThread<Tuple<string>>();
                    if (takeData.Item1 != name)
                        isOk = false;
                    Debug.WriteLine(takeData.Item1);
                    finished++;
                });
                thread1.Start();
            }

            while (finished != 99)
            {

            }

            Assert.IsTrue(isOk);
        }

        [TestMethod]
        public void TestConstructorFactorySingleToneByThread()
        {
            if (!ConstructorFactory.SetSingleToneByThread<Tuple<string>>(new object[] { "hello factory" }))
                Assert.Fail("set singletone constructor not work");

            if (!DataFactory.SetSingleToneByThread<Tuple<string>>())
                Assert.Fail("set singletone not work");
            var takeData = DataFactory.GetSingleToneByThread<Tuple<string>>();
            Assert.IsTrue(takeData.Item1 == "hello factory");
        }
    }
}

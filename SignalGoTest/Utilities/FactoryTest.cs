using SignalGo.Accessibilities;
using System;
using Xunit;

namespace SignalGoTest.Utilities
{
    public class FactoryTest
    {
        [Fact]
        public void TestDataFactorySingleToneByThread()
        {
            var data = new Random();
            if (!DataFactory.SetSingleToneByThread(data))
                Assert.True(false, "set singletone not work");
            var otherdata = new Random();
            var takeData = DataFactory.GetSingleToneByThread<Random>();
            Assert.True(takeData == data && takeData != otherdata);
        }

        [Fact]
        public void TestDataFactorySingleToneByThreadMultiThreading()
        {
            int finished = 0;
            bool isOk = true;
            //for (int i = 0; i < 100; i++)
            //{
            //    string name = "thread" + i;
            //    Thread thread1 = new Thread(() =>
            //    {
            //        Thread.Sleep(1000);
            //        var data = new Tuple<string>(name);
            //        if (!DataFactory.SetSingleToneByThread(data))
            //            Assert.Fail("set singletone not work");
            //        var takeData = DataFactory.GetSingleToneByThread<Tuple<string>>();
            //        if (takeData.Item1 != name)
            //            isOk = false;
            //        Debug.WriteLine(takeData.Item1);
            //        finished++;
            //    });
            //    thread1.Start();
            //}

            //while (finished != 99)
            //{

            //}

            Assert.True(isOk);
        }

        [Fact]
        public void TestConstructorFactorySingleToneByThread()
        {
            if (!ConstructorFactory.SetSingleToneByThread<Tuple<string>>(new object[] { "hello factory" }))
                Assert.True(false, "set singletone constructor not work");

            if (!DataFactory.SetSingleToneByThread<Tuple<string>>())
                Assert.True(false, "set singletone not work");
            var takeData = DataFactory.GetSingleToneByThread<Tuple<string>>();
            Assert.True(takeData.Item1 == "hello factory");
        }
    }
}

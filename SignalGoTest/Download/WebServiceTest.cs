using Xunit;

namespace SignalGoTest2.Download
{
    /// <summary>
    /// Summary description for WebServiceTest
    /// </summary>
    public class WebServiceTest
    {
        public WebServiceTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [Fact]
        public void TestMethod1()
        {
            //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://www.zarinpal.com/pg/services/WebGate/wsdl");
            //var response = webRequest.GetResponse();
            //var stream = new StreamReader(response.GetResponseStream());
            //var data = stream.ReadToEnd();
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalGoTest2.Models;
using SignalGoTest2Services.ServerServices;

namespace SignalGoTest.Validations
{
    [TestClass]
    public class ValidationTest
    {
        [TestMethod]
        public void TestValidationsRule()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            ITestServerModel service = client.RegisterServerServiceInterfaceWrapper<ITestServerModel>();
            ArticleInfo result = service.AddArticle(new ArticleInfo() { Name = "ali", Detail = "rezxa" });
            Assert.IsTrue(result.CreatedDateTime.HasValue);
            MessageContract<ArticleInfo> resultMessage = service.AddArticleMessage(new ArticleInfo());
            Assert.IsTrue(resultMessage.Errors.Count == 2);
            Assert.IsFalse(resultMessage.IsSuccess);
        }
    }
}

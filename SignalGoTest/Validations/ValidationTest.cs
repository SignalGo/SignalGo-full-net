using SignalGoTest2.Models;
using SignalGoTest2Services.Interfaces;
using Xunit;

namespace SignalGoTest.Validations
{
    public class ValidationTest
    {
        [Fact]
        public void TestValidationsRule()
        {
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            ITestServerModel service = client.RegisterServerServiceInterfaceWrapper<ITestServerModel>();
            ArticleInfo result = service.AddArticle(new ArticleInfo() { Name = "ali", Detail = "rezxa" });
            Assert.True(result.CreatedDateTime.HasValue);
            MessageContract<ArticleInfo> resultMessage = service.AddArticleMessage(new ArticleInfo());
            Assert.True(resultMessage.Errors.Count == 2);
            Assert.False(resultMessage.IsSuccess);
        }
    }
}

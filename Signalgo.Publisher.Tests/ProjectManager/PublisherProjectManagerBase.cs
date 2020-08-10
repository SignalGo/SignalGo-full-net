using SignalGo.Publisher.Core.Engines.Interfaces.ProjectManager;
using SignalGo.Publisher.Core.Engines.ProjectManager;

namespace Signalgo.Publisher.Tests.ProjectManager
{
    public abstract class PublisherProjectManagerBase : TestBase
    {
        protected IProjectManager _projectManager;
        protected ICategoryManager _categoryManager;

        protected PublisherProjectManagerBase() : base()
        {
            _categoryManager = new CategoryManagerModule();
            _projectManager = new ProjectManagerModule();

        }

    }
}

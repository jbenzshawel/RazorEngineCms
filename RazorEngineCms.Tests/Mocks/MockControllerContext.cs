using System.Web.Mvc;
using System.Web.Routing;
using Moq;

namespace RazorEngineCms.Tests.Mocks
{
    public class MockControllerContext
    {
        public Mock<ControllerContext> ControllerContext { get; set; }

        public MockControllerContext(string controllerName)
        {
            var mockControllerContext = new Mock<ControllerContext>();
            const string userName = "user";
            mockControllerContext.SetupGet(p => p.HttpContext.User.Identity.Name).Returns(userName);
            mockControllerContext.SetupGet(p => p.HttpContext.Request.IsAuthenticated).Returns(true);
            mockControllerContext.SetupGet(p => p.HttpContext.User.Identity.IsAuthenticated).Returns(true);

            var routeData = new RouteData();
            routeData.Values.Add("controller", controllerName);
            mockControllerContext.SetupGet(m => m.RouteData).Returns(routeData);

            var view = new Mock<IView>();
            var engine = new Mock<IViewEngine>();
            var viewEngineResult = new ViewEngineResult(view.Object, engine.Object);
            engine.Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(viewEngineResult);
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(engine.Object);

            this.ControllerContext = mockControllerContext;
        }

    }
}

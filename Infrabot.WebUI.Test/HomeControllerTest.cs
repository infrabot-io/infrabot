using infrabot.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Infrabot.WebUI.Test
{
    public class HomeControllerTest
    {
        private readonly ILogger<HomeController> _logger = Mock.Of<ILogger<HomeController>>();

        [Test]
        public void Index_ReturnsView()
        {
            // Arrange: create a new context instance for testing
            var controller = new HomeController(_logger);

            // Act: call the asynchronous Index action (default page = 0)
            var result = controller.Index();

            // Assert: check that the result is a ViewResult using the constraint model
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
        }
    }
}

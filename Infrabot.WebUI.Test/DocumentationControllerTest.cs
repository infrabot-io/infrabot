using Infrabot.WebUI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Infrabot.WebUI.Test
{
    public class DocumentationControllerTest
    {
        private readonly ILogger<DocumentationController> _logger = Mock.Of<ILogger<DocumentationController>>();

        [Test]
        public void Introduction_ReturnsView()
        {
            // Arrange: create a new context instance for testing
            var controller = new DocumentationController(_logger);

            // Act: call the asynchronous Index action (default page = 0)
            var result = controller.Introduction();

            // Assert: check that the result is a ViewResult using the constraint model
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
        }

        [Test]
        public void Contents_ReturnsView()
        {
            // Arrange: create a new context instance for testing
            var controller = new DocumentationController(_logger);

            // Act: call the asynchronous Index action (default page = 0)
            var result = controller.Contents();

            // Assert: check that the result is a ViewResult using the constraint model
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
        }

        [Test]
        public void GettingStarted_ReturnsView()
        {
            // Arrange: create a new context instance for testing
            var controller = new DocumentationController(_logger);

            // Act: call the asynchronous Index action (default page = 0)
            var result = controller.GettingStarted();

            // Assert: check that the result is a ViewResult using the constraint model
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
        }

        [Test]
        public void Examples_ReturnsView()
        {
            // Arrange: create a new context instance for testing
            var controller = new DocumentationController(_logger);

            // Act: call the asynchronous Index action (default page = 0)
            var result = controller.Examples();

            // Assert: check that the result is a ViewResult using the constraint model
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
        }
    }
}

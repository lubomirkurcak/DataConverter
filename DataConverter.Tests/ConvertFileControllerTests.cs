using DataConverter.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DataConverter.Tests
{
    public class ConvertFileControllerTests
    {
        ConvertFileController controller;
        Mock<IMailer> mockMailer;

        byte[] jsonSampleFileContents;
        byte[] xmlSampleFileContents;
        byte[] jsonFromXmlSampleFileContents;

        // @todo: Don't rely on third-party API for verification!
        readonly string JsonUrl = "https://restcountries.com/v3.1/alpha/sk";

        FormFile EmptyFile
        {
            get
            {
                byte[] bytes = Array.Empty<byte>();
                var file = new FormFile(new MemoryStream(bytes), 0, bytes.LongLength, "file", "empty.json")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/json"
                };
                return file;
            }
        }

        FormFile JsonFile
        {
            get
            {
                var file = new FormFile(new MemoryStream(jsonSampleFileContents), 0, jsonSampleFileContents.LongLength, "file", "file.json")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/json"
                };
                return file;
            }
        }

        FormFile XmlFile
        {
            get
            {
                var file = new FormFile(new MemoryStream(xmlSampleFileContents), 0, xmlSampleFileContents.LongLength, "file", "file.xml")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/xml"
                };
                return file;
            }
        }

        [SetUp]
        public void Setup()
        {
            mockMailer = new Mock<IMailer>(); ;
            controller = new ConvertFileController();
            controller.Mailer = mockMailer.Object;

            var projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            jsonSampleFileContents = File.ReadAllBytes($"{projectDir}/Data/sk.json");
            xmlSampleFileContents = File.ReadAllBytes($"{projectDir}/Data/sk.xml");
            jsonFromXmlSampleFileContents = File.ReadAllBytes($"{projectDir}/Data/sk_from_xml.json");
        }

        [Test]
        public void FromFile_InvalidOutputContentType_ShouldReturnBadRequest()
        {
            var actionResult = controller.FromFile("unknown-content-type", JsonFile, null);

            Assert.That(actionResult, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public void FromFile_EmptyFile_ShouldReturnNoContent()
        {
            var actionResult = controller.FromFile("xml", EmptyFile, null);

            Assert.That(actionResult, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public void FromFile_ConvertJsonToXml_ShouldReturnCorrectFileResult()
        {
            var actionResult = controller.FromFile("xml", JsonFile, null);
            Assert.That(actionResult, Is.InstanceOf<FileContentResult>());

            var result = actionResult as FileContentResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.FileContents, Is.EqualTo(xmlSampleFileContents));
        }

        [Test]
        public void FromFile_ConvertXmlToJson_ShouldReturnCorrectFileResult()
        {
            var actionResult = controller.FromFile("json", XmlFile, null);
            Assert.That(actionResult, Is.InstanceOf<FileContentResult>());

            var result = actionResult as FileContentResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.FileContents, Is.EqualTo(jsonFromXmlSampleFileContents));
        }

        [Test]
        public void FromFile_ConvertWithEmailParameter_ShouldReturnOKAndSendMail()
        {
            var actionResult = controller.FromFile("json", XmlFile, "client@domain.com");
            Assert.That(actionResult, Is.InstanceOf<OkResult>());

            mockMailer.Verify(x => x.EmailFileTo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task FromURL_InvalidInputContentType_ShouldReturnBadRequest()
        {
            var actionResult = await controller.FromUrlAsync("unknown-content-type", "xml", JsonUrl, null);

            Assert.That(actionResult, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task FromURL_InvalidOutputContentType_ShouldReturnBadRequest()
        {
            var actionResult = await controller.FromUrlAsync("json", "unknown-content-type", JsonUrl, null);

            Assert.That(actionResult, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task FromURL_CorrectRequest_ShouldReturnCorrectFileResult()
        {
            var actionResult = await controller.FromUrlAsync("json", "xml", JsonUrl, null);

            Assert.That(actionResult, Is.InstanceOf<FileContentResult>());

            var result = actionResult as FileContentResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.FileContents, Is.EqualTo(xmlSampleFileContents));
        }

        [Test]
        public async Task FromURL_ConvertWithEmailParameter_ShouldReturnOKAndSendMail()
        {
            var actionResult = await controller.FromUrlAsync("json", "xml", JsonUrl, "client@domain.com");
            Assert.That(actionResult, Is.InstanceOf<OkResult>());

            mockMailer.Verify(x => x.EmailFileTo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
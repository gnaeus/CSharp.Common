using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Common.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.Mail
{
    [TestClass]
    public class MailMessageBinarySerializerAlternateViewsTest
    {
        #region Data
        private const string ImageData = "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";
        #endregion
        
        [TestMethod]
        public void TestAlternateViewSerialization()
        {
            var htmlView = AlternateView.CreateAlternateViewFromString("<p>html body</p>");
            htmlView.ContentType = new ContentType("text/html");
            htmlView.BaseUri = new Uri("https://microsoft.com");

            var imgResource = new LinkedResource(new MemoryStream(Convert.FromBase64String(ImageData)), "image/png") {
                ContentId = null,
                ContentType = new ContentType("image/png"),
            };

            htmlView.LinkedResources.Add(imgResource);

            var msg = new MailMessage("from@from", "to@to", "subject", "plain text body");
            msg.AlternateViews.Add(htmlView);

            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString("<div>html body 2</div>"));

            Debug.WriteLine(TestHelper.GetStreamString(htmlView.ContentStream, Encoding.UTF8));
            Debug.WriteLine(htmlView.BaseUri.ToString());

            byte[] serialized = MailMessageBinarySerializer.Serialize(msg);


            using (var msg2 = MailMessageBinarySerializer.Deserialize(serialized)) {
                Assert.IsNotNull(msg2);
                Assert.AreEqual(2, msg2.AlternateViews.Count);

                var alternateView = msg2.AlternateViews[0];
                Assert.IsNotNull(alternateView);
                Assert.AreNotSame(htmlView, alternateView);

                string data = Encoding.UTF8.GetString(((MemoryStream)alternateView.ContentStream).ToArray());
                Assert.AreEqual("<p>html body</p>", data);

                Assert.AreNotSame(htmlView.BaseUri, alternateView.BaseUri);
                Assert.AreEqual(htmlView.BaseUri, alternateView.BaseUri);

                Debug.WriteLine(TestHelper.GetStreamString(alternateView.ContentStream, Encoding.UTF8));
                Debug.WriteLine(alternateView.BaseUri.ToString());
            }
        }

        [Ignore]
        [TestMethod, TestCategory("Integration")]
        public void TestSendingMailWithAlternateView()
        {
            var address = new MailAddress("dspan@yandex.ru", "Test User", Encoding.UTF8);
            var msg = new MailMessage(address, address) {
                Subject = "text subject",
                Body = "plain text body"
            };

            var htmlView = AlternateView.CreateAlternateViewFromString(
                "<h1><img src=\"/wp-content/themes/ES/images/logo-ES.png\" /><img src=\"cid:companyLogo\" />html body</h1>");

            htmlView.ContentType = new ContentType("text/html");
            htmlView.BaseUri = new Uri("https://microsoft.com");

            var imgResource = new LinkedResource(new MemoryStream(Convert.FromBase64String(ImageData)), "image/png") {
                ContentId = "companyLogo",
                ContentType = new ContentType("image/png"),
            };

            htmlView.LinkedResources.Add(imgResource);

            msg.AlternateViews.Add(htmlView);

            byte[] serialized = MailMessageBinarySerializer.Serialize(msg);

            using (var client = new SmtpClient("10.10.104.138", 25))
            using (var msg2 = MailMessageBinarySerializer.Deserialize(serialized)) {
                client.Send(msg2);
            }
        }
    }
}

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
    public class MailMessageBinarySerializerAttachmentsTest
    {
        private class DisposableStream : MemoryStream
        {
            public bool Disposed { get; private set; }
            
            public DisposableStream(byte[] buffer, bool writeable = true)
                : base(buffer, writeable) {}
            
            protected override void Dispose(bool disposing)
            {
                Disposed = true;
                base.Dispose(disposing);
            }
        }

        private class NonSeekableStream : DisposableStream
        {
            public NonSeekableStream(byte[] buffer, bool writeable = true)
                : base(buffer, writeable) {}

            public override bool CanSeek
            {
                get { return false; }
            }
        }

        [TestMethod]
        public void TestEmailDispose()
        {
            var ts = new DisposableStream(Encoding.UTF8.GetBytes("test test test"));

            using (var msg = new MailMessage()) {
                msg.Attachments.Add(new Attachment(ts, new ContentType("text/plain")));
                Assert.IsFalse(ts.Disposed);
            }
            Assert.IsTrue(ts.Disposed);
        }

        [TestMethod]
        public void TestSeekableStreamSerialization()
        {
            byte[] srcArray = Encoding.UTF8.GetBytes("test test test");
            var src = new DisposableStream(srcArray, false) {
                Position = 5
            };

            byte[] destArray;
            MemoryStream dest;

            byte[] serialized;
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms)) {
                w.Write(src);
                serialized = ms.ToArray();
            }

            Assert.IsTrue(src.Disposed);

            using (var ms = new MemoryStream(serialized, false))
            using (var r = new BinaryReader(ms))
            using (dest = r.ReadStream()) {
                destArray = dest.ToArray();
            }

            Assert.AreNotSame(src, dest);
            Assert.AreEqual("test test test", Encoding.UTF8.GetString(destArray));
        }

        [TestMethod]
        public void TestNonSeekableStreamSerialization()
        {
            byte[] srcArray = Encoding.UTF8.GetBytes("test test test");
            var src = new NonSeekableStream(srcArray, false) {
                Position = 5
            };

            byte[] destArray;
            MemoryStream dest;

            byte[] serialized;
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms)) {
                w.Write(src);
                serialized = ms.ToArray();
            }

            Assert.IsTrue(src.Disposed);

            using (var ms = new MemoryStream(serialized, false))
            using (var r = new BinaryReader(ms))
            using (dest = r.ReadStream()) {
                destArray = dest.ToArray();
            }

            Assert.AreNotSame(src, dest);
            Assert.AreEqual("test test", Encoding.UTF8.GetString(destArray));
        }

        [TestMethod]
        public void TestAttachmentSerialization()
        {
            var stream = new DisposableStream(Encoding.UTF8.GetBytes("test test test"), false);
            
            var attachment = new Attachment(stream, "file.txt", "text/plain");
            attachment.ContentDisposition.FileName = "file.txt";
            attachment.Name = null;

            var msg = new MailMessage();
            msg.Attachments.Add(attachment);

            Debug.WriteLine(TestHelper.GetStreamString(attachment.ContentStream, Encoding.UTF8));
            Debug.WriteLine(attachment.ContentType.ToString());
            Debug.WriteLine(attachment.ContentDisposition.ToString());

            Assert.IsFalse(stream.Disposed);

            byte[] serialized = MailMessageBinarySerializer.Serialize(msg);

            Assert.IsTrue(stream.Disposed);

            using (var msg2 = MailMessageBinarySerializer.Deserialize(serialized)) {
                Assert.IsNotNull(msg2);
                Assert.AreEqual(1, msg2.Attachments.Count);

                var attachment2 = msg2.Attachments[0];
                Assert.IsNotNull(attachment2);
                Assert.AreNotSame(attachment, attachment2);

                string data = Encoding.UTF8.GetString(((MemoryStream) attachment2.ContentStream).ToArray());
                Assert.AreEqual("test test test", data);

                Assert.AreNotSame(attachment.ContentType, attachment2.ContentType);
                Assert.AreEqual(attachment.ContentType.MediaType, attachment2.ContentType.MediaType);
                Assert.IsTrue(TestHelper.DictionariesAreEqual(
                    attachment.ContentType.Parameters, attachment2.ContentType.Parameters));

                Assert.AreNotSame(attachment.ContentDisposition, attachment2.ContentDisposition);
                Assert.AreEqual(attachment.ContentDisposition, attachment2.ContentDisposition);
                
                Debug.WriteLine(TestHelper.GetStreamString(attachment2.ContentStream, Encoding.UTF8));
                Debug.WriteLine(attachment2.ContentType.ToString());
                Debug.WriteLine(attachment2.ContentDisposition.ToString());
            }
        }

        [Ignore]
        [TestMethod, TestCategory("Integration")]
        public void TestSendingMailWithAttachment()
        {
            var address = new MailAddress("dspan@yandex.ru", "Test User", Encoding.UTF8);
            var msg = new MailMessage(address, address) { IsBodyHtml = true };
            var attachment = Attachment.CreateAttachmentFromString("test test test", "file.txt");

            attachment.ContentDisposition.FileName = "file.txt";

            msg.Body = "body body body";

            msg.Attachments.Add(attachment);

            byte[] serialized = MailMessageBinarySerializer.Serialize(msg);

            using (var client = new SmtpClient("10.10.104.138", 25))
            using (var msg2 = MailMessageBinarySerializer.Deserialize(serialized)) {
                client.Send(msg2);
            }
        }
    }
}

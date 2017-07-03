using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Common.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.Mail
{
    [TestClass]
    public class MailMessageBinarySerializerTest
    {
        [TestMethod]
        public void TestConcreteMessageDeserialization()
        {
            const string data = "AD534B6FD340107604A74AF90BC8CD193B8F0605A21081102DA84A704250738BF2589434260E7EA48453DA222AD40810EA8D03D07245B242DD3A0FBB7F61F62F70E347A032BB4EA422B53776B5DA9D996F7666BE9DBD169763028E361C8243B7E90EF830A2FB3016E10B1EA77408A73745F80436CC4438A30370E0187C8EB4610453943D71A1A2BB7026C20481280F38644A77C5087CC3790847327C85EF91F3EBE1253842671B4E7039300B025E9DC172A6A667D1E9BFA621CF2FFD88E17618105C38059BBEC5DD070F3C4CC26177DA0B213DF7F88CF69F88F2444549C6A4D59814974AC430B55EBFA5CC310718F203CF80C5A67B2C417003958DB1FC85DDE1C58C318E8B6938DC45A4DBBCA463F47C4FDF313BAA70EEA37A82A05F8383F052A62A3675F2FC6EA4699ADD7434AA6AF5AADAD40C337D6B25753B152D1283984AD530B634BD117DA4BF2C561EE6DAB9D5427F4D7D5A8A6FA89B957CAF55AB6FDE29D69EBC262BA9C72F8A85C45A522A747B6D3545483DB7B55ED69BC57C57274AF981954FD43B25E9FE8644D6CBC9C2B35C3E5E4844B2176AC40AE8F06285C34CB49A0D1899F3F28371CD202E963384192F96BEE1FCCF50C9FC469C7244D9C1D3A0C1C6DD652C22331370E5A0394E82978571D008D8A81E63D0476184D03D348F03523DB0B911DFCD9743A165C330AD4A83F42A9CB47BE45597E8A6A1A996D9D23A86AC5BE17F9A4508E11FB9D1E8563B7DCB68B65B9D4B3C84CBC6398EDF7F04812D765EE8FF02";
            MailMessage msg = MailMessageBinarySerializer.Deserialize(TestHelper.FromHex(data));

            Debug.Write(msg.Body);
            Debug.WriteLine("");

            string actual = TestHelper.ToHex(MailMessageBinarySerializer.Serialize(msg));

            Assert.AreEqual(data, actual);
        }

        [TestMethod]
        public void TestMailMessage() 
        {
            var address = new MailAddress("test.user@example.com", "Test User", Encoding.UTF8);

            using (var ms = new MemoryStream()) {
                var sw = new BinaryWriter(ms);
                sw.Write(Encoding.UTF8.GetBytes("test test test"));
                sw.Flush();
                ms.Position = 0;

                var msg = new MailMessage(address, address) {
                    Body = "body body body"
                };
                msg.Attachments.Add(new Attachment(ms, new ContentType("text/plain")));

                var client = new SmtpClient("mysmtphost") {
                    DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                    PickupDirectoryLocation = Path.GetTempPath()
                };

                client.Send(msg);
            }
        }

        [TestMethod]
        public void TestSerialization()
        {
            var msg = new MailMessage(
                new MailAddress("test1@mail.com", "Address1"),
                new MailAddress("test2@mail.com", "Address2")) {
                    Subject = "subject sucbejct",
                    Body = "Message Body",
                    IsBodyHtml = false,
                    Priority = MailPriority.High,
                    DeliveryNotificationOptions =
                        DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.Delay
                };
            msg.CC.Add(new MailAddress("test3@mail.com", "Address3"));
            msg.Bcc.Add(new MailAddress("test4@mail.com"));
            msg.ReplyToList.Add("test5@mail.com");
            msg.ReplyToList.Add("test6@mail.com");
            msg.Headers.Add("foo", "bar");
            msg.Headers.Add("foo", "baz");
            msg.HeadersEncoding = Encoding.UTF8;

            byte[] serialized = MailMessageBinarySerializer.Serialize(msg);

            Debug.WriteLine(serialized.Length);
            Debug.WriteLine(TestHelper.ToHex(serialized));
            Debug.WriteLine(Encoding.UTF8.GetString(serialized));

            MailMessage msg2 = MailMessageBinarySerializer.Deserialize(serialized);

            Assert.AreEqual(msg.Subject, msg2.Subject);
            Assert.AreEqual(msg.SubjectEncoding, msg2.SubjectEncoding);

            Assert.AreEqual(msg.Body, msg2.Body);
            Assert.AreEqual(msg.IsBodyHtml, msg2.IsBodyHtml);
            Assert.AreEqual(msg.BodyEncoding, msg2.BodyEncoding);
            Assert.AreEqual(msg.BodyTransferEncoding, msg2.BodyTransferEncoding);


            Assert.AreEqual(msg.Priority, msg2.Priority);
            Assert.AreEqual(msg.DeliveryNotificationOptions, msg2.DeliveryNotificationOptions);
            Assert.IsTrue(msg.Headers.AllKeys.SequenceEqual(msg2.Headers.AllKeys));
            Assert.IsTrue(msg.Headers.AllKeys.SelectMany(msg.Headers.GetValues)
                .SequenceEqual(msg2.Headers.AllKeys.SelectMany(msg2.Headers.GetValues)));
            Assert.AreEqual(msg.HeadersEncoding, msg2.HeadersEncoding);

            Assert.AreEqual(msg.From, msg2.From);
            Assert.AreEqual(msg.Sender, msg2.Sender);
            Assert.IsTrue(msg.To.SequenceEqual(msg2.To));
            Assert.IsTrue(msg.CC.SequenceEqual(msg2.CC));
            Assert.IsTrue(msg.Bcc.SequenceEqual(msg2.Bcc));
            Assert.IsTrue(msg.ReplyToList.SequenceEqual(msg2.ReplyToList));
        }

        [TestMethod]
        public void TestEmptySerialization()
        {
            var msg = new MailMessage();

            byte[] serialized = MailMessageBinarySerializer.Serialize(msg);

            Debug.WriteLine(serialized.Length);
            Debug.WriteLine(TestHelper.ToHex(serialized));
            Debug.WriteLine(Encoding.UTF8.GetString(serialized));

            using (var msg2 = MailMessageBinarySerializer.Deserialize(serialized)) {
                Assert.IsNotNull(msg2);
            }
        }
    }
}

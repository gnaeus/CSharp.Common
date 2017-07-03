using System.IO;
using System.Net.Mail;
using Common.Mail;

partial class _Examples
{
    class DelayedMailSender
    {
        public void StoreMessage(string filePath)
        {
            MailMessage msg = new MailMessage(
                new MailAddress("test1@mail.com", "Address1"),
                new MailAddress("test2@mail.com", "Address2"))
            {
                Subject = "subject sucbejct",
                Body = "Message Body",
                IsBodyHtml = false,
                Priority = MailPriority.High,
            };
            msg.CC.Add(new MailAddress("test3@mail.com", "Address3"));
            msg.Bcc.Add(new MailAddress("test4@mail.com"));
            msg.ReplyToList.Add("test5@mail.com");

            byte[] serializedMsg = MailMessageBinarySerializer.Serialize(msg);

            File.WriteAllBytes(filePath, serializedMsg);
        }

        public void SendStoredMessage(string filePath)
        {
            byte[] serializedMsg = File.ReadAllBytes(filePath);

            MailMessage msg = MailMessageBinarySerializer.Deserialize(serializedMsg);

            using (var client = new SmtpClient("mysmtphost")
            {
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = Path.GetTempPath(),
            })
            {
                client.Send(msg);
            }
        }
    }
}

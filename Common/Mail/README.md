### MailMessageBinarySerializer
Utility for de(serialiaing) `MailMessage` to byte array. Supports .NET 4.0, 4.5.

__`static byte[] Serialize(MailMessage msg)`__  

__`static MailMessage Deserialize(byte[] binary)`__  

__`static MailMessage ReadMailMessage(this BinaryReader r)`__  

__`static void Write(this BinaryWriter w, MailMessage msg)`__  

```cs
using Common.Mail;

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
```

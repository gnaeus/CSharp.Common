using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace Common.Mail
{
    /// <summary>
    /// Utility for de(serialiaing) <see cref="MailMessage"/> to binary. Supports .NET 4.0, 4.5.
    /// </summary>
    /// <remarks>
    /// <para>If some properties of <see cref="MailMessage"/> will be added in future version
    /// of .NET Framework, this utility will not be able to serialize them.</para>
    /// <para>MailAddress constructor <see cref="MailAddress(string, string, Encoding)"/> is not supported.</para>
    /// <para><see cref="ContentType"/>, <see cref="ContentDisposition"/> classes are serialized through
    /// <see cref="ContentType.ToString()"/>, <see cref="ContentDisposition.ToString()"/> methods and deserialized
    /// through <see cref="ContentType(string)"/>, <see cref="ContentDisposition(string)"/> constructors.</para>
    /// </remarks>
    public static class MailMessageBinarySerializer
    {
        /// <summary>
        /// Format version for backward compatibility
        /// </summary>
        public const string FormatVersion = "1.0";

        public static byte[] Serialize(MailMessage msg)
        {
            using (var stream = new MemoryStream()) {
                using (var compressor = new DeflateStream(stream, CompressionMode.Compress))
                using (var writer = new BinaryWriter(compressor)) {
                    writer.Write(msg);
                }
                return stream.ToArray();
            }
        }

        public static MailMessage Deserialize(byte[] binary)
        {
            using (var stream = new MemoryStream(binary))
            using (var decompressor = new DeflateStream(stream, CompressionMode.Decompress))
            using (var reader = new BinaryReader(decompressor)) {
                return reader.ReadMailMessage();
            }
        }

        public static MailMessage ReadMailMessage(this BinaryReader r)
        {
            var msg = new MailMessage();

            // format version for backward compatibility
            string formatVersion = r.ReadString();

            msg.Headers.Clear();
            msg.Headers.Add(r.ReadNameValueCollection());

            msg.Subject = r.ReadString();

            msg.Body = r.ReadString();
            msg.IsBodyHtml = r.ReadBoolean();

            // addresses
            if (r.ReadBoolean()) {
                msg.From = r.ReadMailAddress();
            }
            if (r.ReadBoolean()) {
                msg.Sender = r.ReadMailAddress();
            }
            ReadCollection(r, msg.To, ReadMailAddress);
            ReadCollection(r, msg.CC, ReadMailAddress);
            ReadCollection(r, msg.Bcc, ReadMailAddress);
            ReadCollection(r, msg.ReplyToList, ReadMailAddress);
            
            // options
            msg.Priority = (MailPriority)r.ReadInt32();
            msg.DeliveryNotificationOptions = (DeliveryNotificationOptions)r.ReadInt32();

            // encodings
            int codePage;
            if ((codePage = r.ReadInt32()) != -1) {
                msg.HeadersEncoding = Encoding.GetEncoding(codePage);
            }
            if ((codePage = r.ReadInt32()) != -1) {
                msg.SubjectEncoding = Encoding.GetEncoding(codePage);
            }
            if ((codePage = r.ReadInt32()) != -1) {
                msg.BodyEncoding = Encoding.GetEncoding(codePage);
            }
#if !NET_40
            msg.BodyTransferEncoding = (TransferEncoding)r.ReadInt32();
#else
            r.ReadInt32();
#endif
            ReadCollection(r, msg.Attachments, ReadAttachment);
            ReadCollection(r, msg.AlternateViews, ReadAlternateView);

            return msg;
        }

        public static void Write(this BinaryWriter w, MailMessage msg)
        {
            // format version for backward compatibility
            w.Write(FormatVersion);

            w.Write(msg.Headers);

            w.Write(msg.Subject);

            w.Write(msg.Body);
            w.Write(msg.IsBodyHtml);

            // addresses
            w.Write(msg.From != null);
            if (msg.From != null) {
                w.Write(msg.From);
            }
            w.Write(msg.Sender != null);
            if (msg.Sender != null) {
                w.Write(msg.Sender);
            }
            WriteCollection(w, msg.To, Write);
            WriteCollection(w, msg.CC, Write);
            WriteCollection(w, msg.Bcc, Write);
            WriteCollection(w, msg.ReplyToList, Write);

            // options
            w.Write((int)msg.Priority);
            w.Write((int)msg.DeliveryNotificationOptions);

            // encodings
            w.Write(msg.HeadersEncoding != null ? msg.HeadersEncoding.CodePage : -1);
            w.Write(msg.SubjectEncoding != null ? msg.SubjectEncoding.CodePage : -1);
            w.Write(msg.BodyEncoding != null ? msg.BodyEncoding.CodePage : -1);
#if !NET_40
            w.Write((int)msg.BodyTransferEncoding);
#else
            w.Write(0);
#endif
            WriteCollection(w, msg.Attachments, Write);
            WriteCollection(w, msg.AlternateViews, Write);
        }

        private static void ReadCollection<T>(
            BinaryReader r, Collection<T> collection,
            Func<BinaryReader, T> readElement)
        {
            collection.Clear();
            int count = r.ReadInt32();
            for (int i = 0; i < count; ++i) {
                collection.Add(readElement(r));
            }
        }

        private static void WriteCollection<T>(
            BinaryWriter w, Collection<T> collection,
            Action<BinaryWriter, T> writeElement)
        {
            w.Write(collection.Count);
            foreach (T element in collection) {
                writeElement(w, element);
            }
        }

        private static NameValueCollection ReadNameValueCollection(this BinaryReader r)
        {
            var collection = new NameValueCollection();

            int pairsCount = r.ReadInt32();

            for (int i = 0; i < pairsCount; ++i) {
                collection.Add(r.ReadString(), r.ReadString());
            }
            return collection;
        }

        private static void Write(this BinaryWriter w, NameValueCollection collection)
        {
            int pairsCount = collection.Keys
                .Cast<string>()
                .Select(collection.GetValues)
                .Where(v => v != null)
                .Sum(v => v.Length);

            w.Write(pairsCount);

            foreach (string key in collection.Keys) {
                string[] values = collection.GetValues(key);
                if (values != null) {
                    foreach (string value in values) {
                        w.Write(key);
                        w.Write(value);
                    }
                }
            }
        }

        private static MailAddress ReadMailAddress(this BinaryReader r)
        {
            return new MailAddress(r.ReadString(), r.ReadString());
        }

        private static void Write(this BinaryWriter w, MailAddress addres)
        {
            w.Write(addres.Address);
            w.Write(addres.DisplayName);
        }

        #region Attachments

        private static T ReadAttachmentBase<T>(this BinaryReader r)
            where T : AttachmentBase
        {
            var attachment = (T)Activator.CreateInstance(typeof(T), new object[] {
                // ContentType is deserialized from string
                // through it's `ContentType(string contentType)` constructor
                r.ReadStream(), new ContentType(r.ReadString())
            });

            if (r.ReadBoolean()) {
                attachment.ContentId = r.ReadString();
            }
            attachment.TransferEncoding = (TransferEncoding)r.ReadInt32();

            return attachment;
        }

        private static void WriteAttachmentBase(this BinaryWriter w, AttachmentBase attachment)
        {
            w.Write(attachment.ContentStream);

            // ContentType is completely serialized through it's `ToString()` method
            w.Write(attachment.ContentType.ToString());

            w.Write(attachment.ContentId != null);
            if (attachment.ContentId != null) {
                w.Write(attachment.ContentId);
            }
            w.Write((int)attachment.TransferEncoding);
        }

        private static Attachment ReadAttachment(this BinaryReader r)
        {
            var attachment = r.ReadAttachmentBase<Attachment>();
            // ContentDisposition is deserialized from string
            // through it's `ContentDisposition(string disposition)` constructor
            MapContentDisposition(new ContentDisposition(r.ReadString()), attachment.ContentDisposition);
            
            if (r.ReadBoolean()) {
                attachment.Name = r.ReadString();
            }
            int codePage;
            if ((codePage = r.ReadInt32()) != -1) {
                attachment.NameEncoding = Encoding.GetEncoding(codePage);
            }
            return attachment;
        }

        private static void Write(this BinaryWriter w, Attachment attachment)
        {
            w.WriteAttachmentBase(attachment);
            // ContentDisposition is completely serialized through it's `ToString()` method
            w.Write(attachment.ContentDisposition.ToString());

            w.Write(attachment.Name != null);
            if (attachment.Name != null) {
                w.Write(attachment.Name);
            }
            w.Write(attachment.NameEncoding != null ? attachment.NameEncoding.CodePage : -1);
        }

        // Attachment.ContentDisposition has no setter so we project it's properties one by one
        private static void MapContentDisposition(ContentDisposition from, ContentDisposition to)
        {
            to.CreationDate = from.CreationDate;
            to.DispositionType = from.DispositionType;
            to.FileName = from.FileName;
            to.Inline = from.Inline;
            to.ModificationDate = from.ModificationDate;
            to.ReadDate = from.ReadDate;
            to.Size = from.Size;

            to.Parameters.Clear();
            foreach (string key in from.Parameters.Keys) {
                to.Parameters.Add(key, from.Parameters[key]);
            }
        }

        #endregion

        #region AlternateViews

        private static AlternateView ReadAlternateView(this BinaryReader r)
        {
            var view = r.ReadAttachmentBase<AlternateView>();
            if (r.ReadBoolean()) {
                view.BaseUri = r.ReadUri();
            }
            ReadCollection(r, view.LinkedResources, ReadLinkedResource);
            return view;
        }

        private static void Write(this BinaryWriter w, AlternateView view)
        {
            w.WriteAttachmentBase(view);
            w.Write(view.BaseUri != null);
            if (view.BaseUri != null) {
                w.Write(view.BaseUri);
            }
            WriteCollection(w, view.LinkedResources, Write);
        }

        private static LinkedResource ReadLinkedResource(this BinaryReader r)
        {
            var resource = r.ReadAttachmentBase<LinkedResource>();
            if (r.ReadBoolean()) {
                resource.ContentLink = r.ReadUri();
            }
            return resource;
        }

        private static void Write(this BinaryWriter w, LinkedResource resource)
        {
            w.WriteAttachmentBase(resource);
            w.Write(resource.ContentLink != null);
            if (resource.ContentLink != null) {
                w.Write(resource.ContentLink);
            }
        }

        #endregion

        internal static MemoryStream ReadStream(this BinaryReader r)
        {
            int length = r.ReadInt32();
            return new MemoryStream(r.ReadBytes(length), false);
        }

        /// <exception cref="NotSupportedException" />
        internal static void Write(this BinaryWriter w, Stream stream)
        {
            if (!stream.CanSeek) {
                var ms = new MemoryStream();
                using (stream) {
                    stream.CopyTo(ms);
                }
                stream = ms;
            }
            stream.Position = 0;
            if (stream.Length > Int32.MaxValue) {
                throw new NotSupportedException("Streams with length > Int32.MaxValue are not supported");
            }
            w.Write((int)stream.Length);
            using (stream) {
                stream.CopyTo(w.BaseStream);    
            }
        }

        internal static Uri ReadUri(this BinaryReader r)
        {
            return new Uri(r.ReadString(), r.ReadBoolean() ? UriKind.Absolute : UriKind.Relative);
        }

        internal static void Write(this BinaryWriter w, Uri uri)
        {
            w.Write(uri.ToString());
            w.Write(uri.IsAbsoluteUri);
        }
    }
}

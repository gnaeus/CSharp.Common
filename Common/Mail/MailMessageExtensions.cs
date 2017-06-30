using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;

namespace Common.Mail
{
    public struct MailValidationError
    {
        public readonly string FieldName;
        public readonly string Value;

        internal MailValidationError(string fieldName, string value)
        {
            FieldName = fieldName;
            Value = value;
        }

        public override string ToString()
        {
            return FieldName + ": " + Value;
        }
    }

    public static class MailMessageExtensions
    {
        public static IEnumerable<MailValidationError> ValidateAddresses(this MailMessage message)
        {
            if (!AddressIsValid(message.From.Address)) {
                yield return new MailValidationError("From", message.From.Address);
            }
            foreach (MailAddress address in message.To) {
                if (!AddressIsValid(address.Address)) {
                    yield return new MailValidationError("To", address.Address);
                }
            }
            foreach (MailAddress address in message.CC) {
                if (!AddressIsValid(address.Address)) {
                    yield return new MailValidationError("CC", address.Address);
                }
            }
            foreach (MailAddress address in message.Bcc) {
                if (!AddressIsValid(address.Address)) {
                    yield return new MailValidationError("Bcc", address.Address);
                }
            }
            foreach (MailAddress address in message.ReplyToList) {
                if (!AddressIsValid(address.Address)) {
                    yield return new MailValidationError("ReplyTo", address.Address);
                }
            }
            if (message.Sender != null && !AddressIsValid(message.Sender.Address)) {
                yield return new MailValidationError("Sender", message.Sender.Address);
            }
        }

        private static bool AddressIsValid(string address)
        {
            const ushort maxAsciiCode = 127;

            return new EmailAddressAttribute().IsValid(address)
                && address.All(c => c <= maxAsciiCode);
        }
    }
}

using System.Net.Mail;

namespace DataConverter
{
    public interface IMailer
    {
        public void EmailFileTo(string receiver, string attachmentName, string attachmentContent, string attachmentContentType);
    }

    public class Mailer: IMailer
    {
        const string clientAddress = "noreply@domain.com";
        static readonly SmtpClient client;

        static Mailer()
        {
            client = new SmtpClient
            {
                Credentials = new System.Net.NetworkCredential(clientAddress, "password"),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
            };
        }

        public void EmailFileTo(string receiver, string attachmentName, string attachmentContent, string attachmentContentType)
        {
            var attachment = Attachment.CreateAttachmentFromString(attachmentContent, attachmentContentType);
            attachment.Name = attachmentName;

            var mail = new MailMessage(clientAddress, receiver, "New Converted File", "See attached file.");
            mail.Attachments.Add(attachment);

            client.Send(mail);
        }
    }
}

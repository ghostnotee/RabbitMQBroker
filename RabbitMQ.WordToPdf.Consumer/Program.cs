// See https://aka.ms/new-console-template for more information

using System.Net.Mail;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.WordToPdf.Consumer;
using Spire.Doc;

var connectionFactory = new ConnectionFactory
{
    Uri = new Uri("amqp://guest:guest@localhost:5672")
};

var result = false;

using (var connection = connectionFactory.CreateConnection())
{
    using (var channel = connection.CreateModel())
    {
        channel.ExchangeDeclare("convert-exchange", ExchangeType.Direct, true, false, null);
        channel.QueueBind(queue: "queue-pdf-file", exchange: "DirectExchangePdf", "route-pdf-file");
        channel.BasicQos(0, 1, false);
        var consumer = new EventingBasicConsumer(channel);

        channel.BasicConsume(queue: "queue-pdf-file", autoAck: false, consumer: consumer);
        consumer.Received += ((sender, eventArgs) =>
        {
            try
            {
                Console.WriteLine("A message has been received from the queue and is being processed.");
                Document document = new();

                string desializeString = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                MessageWordToPdf? messageWordToPdf = JsonSerializer.Deserialize<MessageWordToPdf>(desializeString);
                document.LoadFromStream(new MemoryStream(messageWordToPdf.WordByte), FileFormat.Docx2013);

                using (MemoryStream ms = new MemoryStream())
                {
                    document.SaveToStream(ms, FileFormat.PDF); 
                    result = EmailSend(messageWordToPdf.Email, ms, messageWordToPdf.FileName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Someting went wrong. " + e);
            }

            if (result)
            {
                Console.WriteLine("Message processed successfully.");
                channel.BasicAck(eventArgs.DeliveryTag, false);
            }
        });
        Console.ReadLine();
    }
}

bool EmailSend(string email, MemoryStream memoryStream, string fileName)
{
    try
    {
        memoryStream.Position = 0;
        System.Net.Mime.ContentType
            ct = new System.Net.Mime.ContentType(System.Net.Mime.MediaTypeNames.Application.Pdf);
        Attachment attachment = new(memoryStream, ct);
        attachment.ContentDisposition.FileName = $"{fileName}.pdf";

        MailMessage mailMessage = new();
        SmtpClient smtpClient = new();

        mailMessage.From = new MailAddress("selbilgen@gmail.com");
        mailMessage.To.Add(email);
        mailMessage.Subject = "Pdf Dosyası | uzaylimustafa.com";
        mailMessage.Body = "PDF dosyanız ektedir.";
        mailMessage.IsBodyHtml = true;
        mailMessage.Attachments.Add(attachment);

        smtpClient.Host = "smtp.mailtrap.io";
        smtpClient.Port = 587;
        smtpClient.Credentials = new System.Net.NetworkCredential("bad8253f7231fa", "12315f89622f81");
        smtpClient.Send(mailMessage);
        
        Console.WriteLine($"{email} adresine dosya gönderilmiştir.");
        memoryStream.Close();
        memoryStream.Dispose();
        return true;
    }
    catch (Exception e)
    {
        Console.WriteLine($"Bir hata meydana geldi: {e}");
        return false;
    }
}
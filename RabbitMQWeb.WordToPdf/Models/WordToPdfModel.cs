namespace RabbitMQWeb.WordToPdf.Models;

public class WordToPdfModel  {
    public string Email { get; set; }
    public IFormFile File { get; set; }
}
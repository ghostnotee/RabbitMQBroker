# RabbitMQBroker
RabbitMQ HelloWorld
![Untitled](https://s3-us-west-2.amazonaws.com/secure.notion-static.com/efc259b3-6c30-4103-b262-51cb9ac33fc2/Untitled.png)

[https://www.cloudamqp.com/](https://www.cloudamqp.com/) (Online RabbitMQ servisi.)

```docker
docker run -d --hostname my-rabbit --name rabbitmqcontainer -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

![Untitled](https://s3-us-west-2.amazonaws.com/secure.notion-static.com/e342b0f9-8323-4db6-8ee1-b3941447afa7/Untitled.png)

RabbitMQ bir mesaj kuyruğu sistemidir. Benzerleri Apache Kafka, Msmq, Microsoft Azure Service Bus, Kestrel, ActiveMQ olarak sıralanabilir. Amacı herhangi bir kaynaktan alınan bir mesajın, bir başka kaynağa sırası geldiği anda iletilmesidir. Mantık olarak Redis Pub/Sub’a benzemektedir. Ama burada yapılacak işler bir sıraya alınmaktadır. Yani iletimin yapılacağı kaynak ayağa kalkana kadar, tüm işlemler bir quee’de sıralanabilir. Fakat aynı durum Redis Pub’Sub için geçerli değildir. RabbitMQ çoklu işletim sistemine destek vermesi ve açık kaynak kodlu olması da en büyük tercih sebeplerinden birisidir.

- **Producer / Publisher**: Mesajı atan kaynak yani uygulamadır. Redis’deki Pub/Sub düşünüldüğünde Publisher tarafıdır.
- **Exchange :** Hiç bir kuyruk yoksa excange gelen mesaj havada kalır. kuyruk must.
- **Queue** : Gönderilen mesajlar alıcaya ulaştırılmadan önce bir sıraya konur. Gelen yoğunluğa göre veya alıcıya erişilemediği durumlarda, gelen tüm mesajlar Queue’de yani memory’de saklanır. Eğer bu süreç uzun sürer ise memory şişebilir. Ayrıca server’ın restart edilmesi durumunda ilgili mesajlar kaybolabilir.
- **Consumer / Subscriber**: Gönderilen mesajı karşılayan sunucudur. Yani Redis Pub/Sub’daki Subscribe’dır. Kısaca ilgili kuyruğu(Queue)’yu dinleyen taraftır.
- **Fifo**: RabbitMQ’da giden mesajların işlem sırası first in first out yani ilk giren ilk çıkar şeklindedir.

Mesajları emir kipi, event’leri ise geçmiş zaman kipi ile kullanabiliriz.

### .Net ile Kullanmaya Başlamak. 👀

RabbitMq’ya bağlanmak için öncelikle bir **Connection Factory** isminde bir class oluşturmamız gerekiyor.

```csharp
var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
```

Sonra bu factory üzerinden bir bağlantı açılıyor.

```csharp
using var connection = factory.CreateConnection();
```

Connection açıldıktan sonra bir kanal açılıyor. Bu kanal üzerinden RabbitMq’ya bağlanacağız.

```csharp
var channel = connection.CreateModel();
```

RabbitMQ’ya bir mesaj iletmek için bir kuyruk olmalı. Kuyruk olmazsa mesajlar boşa gider. Kuyruk oluşturma.

```csharp
channel.QueueDeclare("hello-queue",true,false,false);
```

string queue nun ismi, durable : mesajlarlar memory demi fiziksel olarakmı tutulacak, exclusive: true değeri verilirse sadece bu class da oluşturduğumuz kanal üzerinden bağlanılabilir; false: değeri verildiği taktirde farklı bir subscriber da oluşturulan bir kanaldan da bağlanılabilir. autoDelete: Subscriber lar down olduğunda kuyruk silinir. false değeri verildiğinde silinmez.

Mesaj oluşturma; mesajlarımızı RabbitMQ’ya byte dizisi olarak gönderiyoruz.

```csharp
string message = "Hello-RabbitMQ";
var messageBody = Encoding.UTF8.GetBytes(message);
```

Mesajın kuyruğa gönderilmesi;

```csharp
channel.BasicPublish(String.Empty,"hello-queue",null,messageBody);
```

!! Kuyruk oluşturma işlemini Subscriber a bırakabiliriz çünkü Subscriber ayağa kalktığında bir kuyruk yoksa hata alınır. Kuyruk her iki tarafta da oluşturulduysa; aynı parametrelerle oluşturulduğundan emin olunmalı.

Consumer (Subscriber) tarafının oluşturulması:

```csharp
var consumer = new EventingBasicConsumer(channel);
```

Consumer hangi channel i dinleyecek.

parametreler: dinlenecek kuyruğun ismi, autoAck: true ise mesaj doğruda işlense yanlış da işlense kuyruktan siler, consumer:

```csharp
channel.BasicConsume("hello-queue", true, consumer);
```

Subscriber lar mesajı başarılı bir şekilde işlediğini haber verebilir. BasicConsume ikinci parametresi autoAck’i  false yapmamız gerek.

Ve daha sonra mesaj işlendikten sonra; ikinci multiple parametresi kuyrukta başka rabbit mq ya gitmemiş mesajlar varsa onların bilgilerini de döner.

```csharp
channel.BasicAck(eventArgs.DeliveryTag,false);
```

**Exchange Türleri:**

**Fanout exchange:** Burada routing key’in bir önemi yoktur. Daha çok broadcast yayınlar için uygundur. Özellikle (MMO) oyunlarda top10 güncellemeleri ve global duyurular için kullanılır. Yine real-time spor haberleri gibi yayınlarda fanout exchange kullanılır.

![http://www.borakasmer.com/wp-content/uploads/2016/12/exchange-fanout.png](http://www.borakasmer.com/wp-content/uploads/2016/12/exchange-fanout.png)

**Direct exchange:** Yapılacak işlere göre bir routing key belirlenir ve buna göre ilgili direct exchange ile amaca en uygun queue’ya gidilir.

![https://www.borakasmer.com/wp-content/uploads/2016/12/exchange-direct.png](https://www.borakasmer.com/wp-content/uploads/2016/12/exchange-direct.png)

**Topic Exchange:** Consumer nasıl bir data istiyorsa bunu routeKey'ine yazsın ve routeKeyine bind ettiği kuyruğunu oluştursun. Detaylı routlama yapmak istediğimizde kullanacağımız bir exchange tipi.

**Headers Exchange:** Yine bu exchange de routing key’i kullanmaz ve message headers’daki birkaç özellik ve tanımlama ile doğru queue’ye iletim yapar. Header üzerindeki attributeler ile queue üzerindeki attributelerin, tamamının değerlerinin birbirini tutması gerekmektedir. Bir tanımlamada Header Exchange’in Direct Exchange’in Steroidli hali dendiğini gördüm :) x-match=any herhangi bir header uysa yeter. x-match=all dersem herbir header eşleşmeli demiş oluyorum.

### Mesajları Direk Kuyruklar Yerine Exchange’lere göndermek

**Fanout Exchange:** Örnek senaryoda yayın dinleyecek consumerlar kendi kuyruklarını kendileri oluşturur. Exchange oluşturmak;

```csharp
channel.ExchangeDeclare("logs-fanout",durable:true,type:ExchangeType.Fanout);
```

Random Queue Name üretmek ve ardından kuyruk oluşturma (bind)

```csharp
var randomQueueName = channel.QueueDeclare().QueueName;
channel.QueueBind(randomQueueName,"logs-fanout",null);
```

Kuyruğu kalıcı hale getirme;

```csharp
var randomQueueName = "log-database-save-queue";
channel.QueueDeclare(randomQueueName, true, false, false);
```

**Direct Exchange:** Exchange mesajı direk ilgili kuyruğa bir route key ile route’lama yapıyor. Hem publisher hem subscriber bu keyi bildiğinden doğru kuyruğu dinliyor.

Publisher;

```csharp
var routeKey = $"route-{x}";
var queueName = $"direct-queue-{x}";
channel.QueueDeclare(queueName, true, false, false, null);
channel.QueueBind(queueName, "logs-direct", routeKey, null);
```

Subscriber;

```csharp
var queueName = "direct-queue-Error";
channel.BasicConsume(queueName, false, consumer);
```
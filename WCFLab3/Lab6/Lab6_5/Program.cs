using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.IO;
using System.Xml;
using System.ServiceModel.Description;

namespace Lab6_5
{
    [ServiceContract]
    public interface IZadanie3
    {
        [OperationContract, WebGet(UriTemplate = "index.html"), XmlSerializerFormat]
        XmlDocument Serwuj();

        [OperationContract, WebInvoke(UriTemplate = "Dodaj/{a}/{b}")]
        int Dodaj(string a, string b);

        [OperationContract, WebGet(UriTemplate = "scripts.js")]
        Stream GetStream();
    }
    class Program
    {
        static void Main(string[] args)
        {
            var fabryka = new ChannelFactory<IZadanie3>(
                new WebHttpBinding(),
                new EndpointAddress("http://localhost:58893/Service1.svc/Lab6_3_4/"));
            fabryka.Endpoint.Behaviors.Add(new WebHttpBehavior());
            var klient = fabryka.CreateChannel();

            string a = "5";
            string b = "18";
            Console.WriteLine("Wynik " + a + " + " + b + ": " + klient.Dodaj(a, b));
            ((IDisposable)klient).Dispose();
            fabryka.Close();
            Console.ReadKey();
        }
    }
}
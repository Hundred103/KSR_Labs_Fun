using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Lab6_client
{
    [ServiceContract]
    public interface IZadanie6
    {
        [OperationContract]
        int Dodaj(int a, int b);
    }
    class Program
    {
        static void Main(string[] args)
        {
            var fabryka = new ChannelFactory<IZadanie6>(
                new NetNamedPipeBinding(),
                new EndpointAddress("net.pipe://localhost/router"));
            var klient = fabryka.CreateChannel();
            Console.WriteLine(klient.Dodaj(53, 98));
            Console.ReadKey();
            ((IDisposable)klient).Dispose();
            fabryka.Close();
        }
    }
}
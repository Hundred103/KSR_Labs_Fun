using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Collections.ObjectModel;

namespace Lab6_2
{
    [ServiceContract]
    public interface IZadanie1
    {
        [OperationContract]
        string ScalNapisy(string a, string b);
    }
    class Program
    {
        static void Main(string[] args)
        {
            var klientDisc = new DiscoveryClient(
                new UdpDiscoveryEndpoint("soap.udp://localhost:30703"));
            var list = klientDisc.Find(new FindCriteria(typeof(IZadanie1))).Endpoints;
            klientDisc.Close();

            if (list.Count > 0)
            {
                var adres = list[0].Address;
                Console.WriteLine(adres.ToString());
                var klient = ChannelFactory<IZadanie1>
                    .CreateChannel(new NetNamedPipeBinding(), adres);
                Console.WriteLine(klient.ScalNapisy("Klient Zadanie2", " dziala poprawnie..."));
                Console.ReadKey();
                ((IDisposable)klient).Dispose();
            }
        }
    }
}
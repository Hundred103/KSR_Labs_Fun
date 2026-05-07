using System;
using System.ServiceModel;
using KSR_WCF1;

namespace Lab4_Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Lab4_1

            var fact = new ChannelFactory<IZadanie1>(new NetNamedPipeBinding(), 
                new EndpointAddress("net.pipe://localhost/ksr-wcf1-test"));
            var channel = fact.CreateChannel();

            Console.WriteLine("wynik wyslania testu = {0}", channel.Test("TEST"));

            //Lab4_3

            var channel3 = new ServiceReferenceLab4_3.Zadanie2Client();
            Console.WriteLine(channel3.Test("xyz"));
            ((IDisposable)channel3).Dispose();

            //Lab4_5

            try
            {
                channel.RzucWyjatek(true);
            }
            catch (FaultException<Wyjatek> ex) 
            {
                Console.WriteLine(channel.OtoMagia(ex.Detail.magia));
            }

            //Lab4_7
            var channel7 = new ServiceReferenceLab4_7.Zadanie7Client();
            try
            {
                channel7.RzucWyjatek7("HELLO WORLD", 9);
            }
            catch (FaultException<ServiceReferenceLab4_7.Wyjatek7> ex)
            {
                Console.WriteLine("Zlapano wyjatek: " + ex);
            }

            ((IDisposable)(channel7)).Dispose();
            ((IDisposable)channel).Dispose();
            fact.Close();
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }
    }
}

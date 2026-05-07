using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using KSR_WCF1;

namespace Lab4_Server
{
    [ServiceContract]
    public interface IZadanie2
    {
        [OperationContract] string Test(string arg);
    }
    public class Server : IZadanie2
    {
        public string Test(string arg)
        {
            return $"test arg: {arg}";
        }
    }

    [DataContract]
    public class Wyjatek7
    {
        [DataMember] string opis;
        [DataMember] string a;
        [DataMember] int b;
    }

    [ServiceContract]
    public interface IZadanie7
    {
        [OperationContract]
        [FaultContract(typeof(Wyjatek7))]
        void RzucWyjatek7(string a, int b);
    }

    public class Server7 : IZadanie7
    {
        public void RzucWyjatek7(string a, int b)
        {
            throw new FaultException<Wyjatek7>(new Wyjatek7(),
                new FaultReason("Reason: " + a + " " + b));
        }
    }

    internal class Program
    {

        static void Main(string[] args)
        {
            //Lab4_2

            var host = new ServiceHost(typeof(Server));
            host.AddServiceEndpoint(typeof(IZadanie2), 
                new NetNamedPipeBinding(), 
                "net.pipe://localhost/ksr-wcf1-zad2");

            //Lab4_3

            var behav = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (behav == null) behav = new ServiceMetadataBehavior();
            host.Description.Behaviors.Add(behav);

            host.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, 
                MetadataExchangeBindings.CreateMexNamedPipeBinding(), 
                "net.pipe://localhost/metadata");

            //Lab4_4

            host.AddServiceEndpoint(typeof(IZadanie2), 
                new NetTcpBinding(), 
                "net.tcp://127.0.0.1:55765");

            //Lab4_7 

            var host7 = new ServiceHost(typeof(Server7));
            host7.AddServiceEndpoint(typeof(IZadanie7), 
                new NetNamedPipeBinding(), 
                "net.pipe://localhost/ksr-wcf1-zad7");

            var behav7 = host7.Description.Behaviors. Find<ServiceMetadataBehavior>();
            if (behav7 == null) behav7 = new ServiceMetadataBehavior();
            host7.Description.Behaviors.Add(behav7);

            host7.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                MetadataExchangeBindings.CreateMexNamedPipeBinding(),
                "net.pipe://localhost/metadata7");

            host.Open();
            host7.Open();
            Console.WriteLine("Opened host and host7. Press any key to close...");
            Console.ReadKey();
        }
    }
}

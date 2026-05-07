using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Routing;
using System.ServiceModel.Dispatcher;

namespace Lab6_router
{
    class Program
    {
        static void Main(string[] args)
        {
            var routePath1 = "net.pipe://localhost/zad6_server1";
            var routePath2 = "net.pipe://localhost/zad6_server2";
            var routeAdres = "net.pipe://localhost/router";

            var host = new ServiceHost(typeof(RoutingService));
            host.AddServiceEndpoint(
                typeof(IRequestReplyRouter),
                new NetNamedPipeBinding(),
                routeAdres);


            var routeConfig = new RoutingConfiguration();
            var contract = ContractDescription.GetContract(typeof(IRequestReplyRouter));

            var server1 = new ServiceEndpoint(
                contract,
                new NetNamedPipeBinding(),
                new EndpointAddress(routePath1));

            var server2 = new ServiceEndpoint(
                contract,
                new NetNamedPipeBinding(),
                new EndpointAddress(routePath2));

            var list = new List<ServiceEndpoint>();
            list.Add(server1);
            list.Add(server2);

            routeConfig.FilterTable.Add(new MatchAllMessageFilter(), list);
            host.Description.Behaviors.Add(new RoutingBehavior(routeConfig));

            host.Open();
            Console.WriteLine("Router działa...");
            Console.ReadKey();
            host.Close();
        }
    }
}
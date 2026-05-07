using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using KSR_WCF2;

namespace Lab5_1_2
{
    public class Handler : Zadanie2Ref.IZadanie2Callback
    {
        public void Zadanie([MessageParameter(Name = "zadanie")] string zadanie1, int pkt, bool zaliczone)
        {
            Console.WriteLine($"{zadanie1} pkt: {pkt} zaliczone: {zaliczone}");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var client1 = new Zadanie1Ref.Zadanie1Client();
            IAsyncResult result = client1.BeginDlugieObliczenia(null, null);

            for (int x = 0; x < 21; x++)
            {
                Console.WriteLine(client1.Szybciej(x, 3 * x * x - 2 * x));
            }
            Console.WriteLine(client1.EndDlugieObliczenia(result));

            ((IDisposable)client1).Dispose();

            var client2 = new Zadanie2Ref.Zadanie2Client(new InstanceContext(new Handler()));
            client2.PodajZadania();
            Console.ReadKey();

            ((IDisposable)client2).Dispose();
        }
    }
}

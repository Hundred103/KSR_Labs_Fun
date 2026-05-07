using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using KSR_WCF2;

namespace Lab5_7
{
    public class HandlerZad6 : Zadanie5_6Ref.IZadanie6Callback
    {
        public void Wynik(int wyn)
        {
            Console.WriteLine($"Wynik zwrotny: {wyn}");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var client5 = new Zadanie5_6Ref.Zadanie5Client();
            Console.WriteLine(client5.ScalNapisy(client5.ScalNapisy("Start123", "456"), "789Koniec"));

            var client6 = new Zadanie5_6Ref.Zadanie6Client(new InstanceContext(new HandlerZad6()));
            client6.Dodaj(1, 1);
            Console.ReadKey();
            ((IDisposable)client5).Dispose();
            ((IDisposable)client6).Dispose();
        }
    }
}
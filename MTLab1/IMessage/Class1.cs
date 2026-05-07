using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMessage
{
    public interface IWiadomosc1
    {
        string message1 { get; set; }
    }

    public interface IWiadomosc2
    {
        string message2 { get; set; }
    }

    public interface IWiadomosc3 : IWiadomosc1, IWiadomosc2
    {
    }
}

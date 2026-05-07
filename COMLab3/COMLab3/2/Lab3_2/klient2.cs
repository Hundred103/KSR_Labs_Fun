using System;


namespace KSRKlient2
{
    class Program
    {
        public static void Main(string[] args)
        {
            string progId = "KSR20.COM3Klasa.1";
            string methodName = "Test";

            Type type = Type.GetTypeFromProgID(progId);
            if (type != null)
            {
                try
                {
                    object act = Activator.CreateInstance(type);
                    type.InvokeMember(methodName, System.Reflection.BindingFlags.InvokeMethod, null, act, new object[] { "Testowanie, zadanie 2 ok! Klasa dzia³a w c#!" });
                }
                catch
                {
                    Console.WriteLine("nie uda³o sie wywo³aæ testu");
                }
            }
            else
            {
                Console.WriteLine("nie pobrano typu");
            }
        }
    }
} // namespace KSRKlient2

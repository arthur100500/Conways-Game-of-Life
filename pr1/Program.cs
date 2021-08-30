using System;

namespace pr1
{
    public class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var g = new Window(800, 600, "");
            g.Run();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Console;

namespace spider
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            int suitCount = 1;
            for (int i = 0 ; i < args.Length ; ++i)
            switch (args[i]) 
            {
                case "-sc":
                    suitCount = int.Parse(args[i + 1]);
                    i += 1;
                    break;
                default:
                    WriteLine("unknown option: " + args[i]);
                    WriteLine("usage: [-sc <num>]");    
                    return;
            }
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(suitCount));
        }
    }
}

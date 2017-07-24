using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    class Program
    {
        static void Main(string[] args)
        {
            DeviceLayer dl = new DeviceLayer();
            dl.Intialize("COM3");

            Console.WriteLine("HP-77 Auto Titrator");
            //var str = Console.ReadLine();
            Task.Run(async () =>
                {
                   // while (true)
                    {
                        try
                        {
                            await dl.Rinse();
                            // int temp = await dl.ReadMv();
                            // Console.SetCursorPosition(2, 2);
                            // Console.WriteLine(temp + " mV                        ");
                            // continue;
                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine("Error");
                        }
                    }
                }
                ).GetAwaiter().GetResult();

            Console.WriteLine("Program ended. Thanks.");
            Console.Read();
        }

        private static async Task Titrate1(DeviceLayer dl)
        {
            Console.WriteLine("Press <Enter> key to end...");
            Task.WaitAny(dl.Titrate(DosageSpeed.SpeedSixteen), GetInputAsync());

            await dl.TitrateCancel();
        }

        private static async Task<string> GetInputAsync()
        {
            return Task.Run(() => Console.ReadLine()).Result;
        }
    }
}

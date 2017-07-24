using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    public class DeviceLayer
    {
        static SerialPort _serialPort = new SerialPort();
        int timeout = 750000;

        TaskCompletionSource<int> ReceiveElectrodeWaitTask;
        
        TaskCompletionSource<int> ReceiveMainWaitTask;

        public Dictionary<DosageSpeed, string> DosageSpeeds;

        public void Intialize(string comport)
        {
            _serialPort.PortName = comport;
            _serialPort.Open();
            _serialPort.DataReceived += _serialPort_DataReceived;

            DosageSpeeds = new Dictionary<DosageSpeed, string>();

            DosageSpeeds.Add(DosageSpeed.SpeedHalf, "d");
            DosageSpeeds.Add(DosageSpeed.SpeedOne, "e");
            DosageSpeeds.Add(DosageSpeed.SpeedTwo, "f");
            DosageSpeeds.Add(DosageSpeed.SpeedThree, "n");
            DosageSpeeds.Add(DosageSpeed.SpeedFour, "g");
            DosageSpeeds.Add(DosageSpeed.SpeedFive, "o");
            DosageSpeeds.Add(DosageSpeed.SpeedSix, "h");
            DosageSpeeds.Add(DosageSpeed.SpeedEight, "i");
            DosageSpeeds.Add(DosageSpeed.SpeedTen, "j");
            DosageSpeeds.Add(DosageSpeed.SpeedTwelve, "k");
            DosageSpeeds.Add(DosageSpeed.SpeedFourteen, "l");
            DosageSpeeds.Add(DosageSpeed.SpeedSixteen, "m");
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int byteRead = Convert.ToInt32(_serialPort.ReadByte());
            Console.WriteLine("Received: " + byteRead.ToString());


            if (ReceiveMainWaitTask != null && ReceiveMainWaitTask.Task.IsCompleted == false && ((int[])ReceiveMainWaitTask.Task.AsyncState).Any(x => x == byteRead))
                ReceiveMainWaitTask.SetResult(byteRead);

            if (ReceiveElectrodeWaitTask!=null && ReceiveElectrodeWaitTask.Task.IsCompleted == false)
                {
                    ReceiveElectrodeWaitTask.SetResult(byteRead);
                }
         
        }


        public void Send(String sendText)
        {
            Console.WriteLine("Sent: " + sendText);
            _serialPort.Write(sendText);
            
        }

        public async Task<Int32> ReadMv()
        {

            Send("v");

            int lowerbyte;
            try
            {
                lowerbyte = await WaitForElectrodeInput(timeout);
            }
            catch(DeviceNotRespondingException ex)
            {
                Send("q");
                throw ex;
            }


            Send("q");
            int higherbyte = await WaitForElectrodeInput(timeout);
            return ((higherbyte * 256) + lowerbyte) - 2048;
        }

        public async Task Dose(int timeout)
        {
            Send("b");
            Send("b");

            int output = await WaitForMainInput(new int[] { 255 }, timeout);

            Send("c");
            Send("c");
        }
        public async Task Fill(int timeout)
        {
            Send("a");
            Send("a");
            int output = await WaitForMainInput(new int[] { 255 }, timeout);

            output = await WaitForMainInput(new int[] { 255 }, timeout);

            Send("c");
            Send("c");
        }
        public async Task Wash()
        {
            Send("a");
            Send("a");
            int output = await WaitForMainInput(new int[] { 255 }, timeout);
            output = await WaitForMainInput(new int[] { 255 }, timeout);

            Send("c");
            Send("c");

            output = await WaitForMainInput(new int[] { 255 }, timeout);

            Send("b");
            Send("b");

            output = await WaitForMainInput(new int[] { 255 }, timeout);

            output = await WaitForMainInput(new int[] { 255 }, timeout);

            output = await WaitForMainInput(new int[] { 255 }, timeout);

            output = await WaitForMainInput(new int[] { 255 }, timeout);

            Send("c");
            Send("c");
        }
        public async Task Rinse()
        {
            Send("a");
            Send("a");

            int output = await WaitForMainInput(new int[] { 255}, timeout);

            output = await WaitForMainInput(new int[] { 255 }, timeout);

            Send("R");
            Send("R");

            output = await WaitForMainInput(new int[] { 255 }, timeout);

            output = await WaitForMainInput(new int[] { 255 }, timeout);

            output = await WaitForMainInput(new int[] { 255 }, timeout);

            Send("c");
            Send("c");

        }

        private async Task<int> WaitForMainInput(int[] array, int timeout)
        {
            ReceiveMainWaitTask = new TaskCompletionSource<int>(array);
            var result1 = await Task.WhenAny(ReceiveMainWaitTask.Task, Task.Delay(timeout));
            if (result1 != ReceiveMainWaitTask.Task) throw new DeviceNotRespondingException("The device did not respond with the given time.");
            var output = ReceiveMainWaitTask.Task.Result;
            return output;
        }

        private async Task<int> WaitForElectrodeInput(int timeout)
        {
            ReceiveElectrodeWaitTask = new TaskCompletionSource<int>();
            var result1 = await Task.WhenAny(ReceiveElectrodeWaitTask.Task, Task.Delay(timeout));
            if (result1 != ReceiveElectrodeWaitTask.Task) throw new DeviceNotRespondingException("The device did not respond with the given time.");
            var output = ReceiveElectrodeWaitTask.Task.Result;
            return output;
        }

        public async Task TitrateCancel()
        {
            Send("r");
            Send("r");

            int output = await WaitForMainInput(new int[] { 255 }, timeout);
        }



        public async Task Titrate(DosageSpeed speed)
        {
            Send(DosageSpeeds[speed]);
            Send(DosageSpeeds[speed]);

            int output = await WaitForMainInput(new int[] { 255 }, timeout);
        }

    }
}

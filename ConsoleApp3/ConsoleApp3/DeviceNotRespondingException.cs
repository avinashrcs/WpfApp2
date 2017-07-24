using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    class DeviceNotRespondingException : Exception
    { 

        public DeviceNotRespondingException(string message) : base(message)
        {
        }  
    }
}

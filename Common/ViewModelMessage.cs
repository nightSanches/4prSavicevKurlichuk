using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ViewModelMessage
    {
        public string Command { get; set; }
        public string Date { get; set; }
        public ViewModelMessage(string Command, string Data) {
            this.Command = Command;
            this.Date = Data;
        }
    }
}

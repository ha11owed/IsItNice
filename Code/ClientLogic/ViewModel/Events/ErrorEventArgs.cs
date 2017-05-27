using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLogic.ViewModel.Events
{
    public class ErrorEventArgs : EventArgs
    {
        public ErrorEventArgs(string errorType)
        {
            this.ErrorType = errorType;
        }

        public string ErrorType { get; private set; }
    }
}

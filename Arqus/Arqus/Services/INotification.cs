using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Services
{
    public interface INotification
    {
        void Show(string messageTitle, string message);
    }
}

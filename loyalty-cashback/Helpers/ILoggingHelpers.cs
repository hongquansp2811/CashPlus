using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;

namespace LOYALTY.Helpers
{
    public interface ILoggingHelpers
    {
        Task insertLogging(LoggingRequest loggingRequest);
    }
}

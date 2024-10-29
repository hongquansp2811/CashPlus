using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using Microsoft.Extensions.Configuration;
using LOYALTY.Models;
using LOYALTY.Data;

namespace LOYALTY.Helpers
{
    public class LoggingHelpers : ILoggingHelpers
    {
        private readonly IConfiguration _configuration;
        private readonly LOYALTYContext _context;
        public LoggingHelpers(IConfiguration configuration, LOYALTYContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        public Task insertLogging(LoggingRequest loggingRequest)
        {
            try
            {
                var logging = new Logging();
                logging.id = Guid.NewGuid();
                logging.user_type = loggingRequest.user_type;
                logging.api_name = loggingRequest.api_name;
                logging.application = loggingRequest.application;
                logging.functions = loggingRequest.functions;
                logging.actions = loggingRequest.actions;
                logging.IP = loggingRequest.IP;
                logging.content = loggingRequest.content;
                logging.result_logging = loggingRequest.result_logging;
                logging.is_login = loggingRequest.is_login;
                logging.is_call_api = loggingRequest.is_call_api;
                logging.user_created = loggingRequest.user_created;
                logging.date_created = DateTime.Now;

                _context.Loggings.Add(logging);
                _context.SaveChanges();
            } catch (Exception ex)
            {

            }

            return Task.CompletedTask;
          
        }
    }
}

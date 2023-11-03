using System;
using AlliedLogger;
using SAP.Middleware.Connector;
using SAPServer.Models;

namespace SAPServer
{
    public class SapSystemConnect : IDestinationConfiguration
    {
        private readonly string _sapServer;
        private readonly string _username;
        private readonly string _password;
        private readonly string _appServerHost;
        private readonly string _systemNumber;
        private readonly string _client;
        private readonly string _language;
        private readonly string _poolSize;
        private readonly string _peakConnectionsLimit;
        private readonly string _connectionIdleTimeout;
        private readonly IDynamicLogger _logger;


        event RfcDestinationManager.ConfigurationChangeHandler IDestinationConfiguration.ConfigurationChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public SapSystemConnect(SapVariables sapVariables, IDynamicLogger logger)
        {
            _sapServer = sapVariables.SapServer;
            _username = sapVariables.Username;
            _password = sapVariables.Password;
            _appServerHost = sapVariables.AppServerHost;
            _systemNumber = sapVariables.SystemNumber;
            _client = sapVariables.Client;
            _language = sapVariables.Language;
            _poolSize = sapVariables.PoolSize;
            _peakConnectionsLimit = sapVariables.PeakConnectionsLimit;
            _connectionIdleTimeout = sapVariables.ConnectionIdleTimeout;
            _logger = logger;
        }

        bool IDestinationConfiguration.ChangeEventsSupported()
        {
            //throw new NotImplementedException();
            return false;
        }

        public RfcConfigParameters GetParameters(string destinationName)
        {
            RfcConfigParameters parms = new RfcConfigParameters();
            if (_sapServer.Equals(destinationName))
            {
                parms.Add(RfcConfigParameters.AppServerHost, _appServerHost);
                parms.Add(RfcConfigParameters.SystemNumber, _systemNumber);
                parms.Add(RfcConfigParameters.User, _username);
                parms.Add(RfcConfigParameters.Password, _password);
                parms.Add(RfcConfigParameters.Client, _client);
                parms.Add(RfcConfigParameters.Language, _language);
                parms.Add(RfcConfigParameters.PoolSize, _poolSize);
                parms.Add(RfcConfigParameters.PeakConnectionsLimit, _peakConnectionsLimit);
                parms.Add(RfcConfigParameters.ConnectionIdleTimeout, _connectionIdleTimeout);

                _logger.Log($"parms.Add(RfcConfigParameters.AppServerHost, _appServerHost)");
                _logger.Log($"parms.Add(RfcConfigParameters.SystemNumber, _systemNumber)");
                _logger.Log($"parms.Add(RfcConfigParameters.User, _username)");
                _logger.Log($"parms.Add(RfcConfigParameters.Password, _password)");
                _logger.Log($"parms.Add(RfcConfigParameters.Client, _client)");
                _logger.Log($"parms.Add(RfcConfigParameters.Language, _language)");
                _logger.Log($"parms.Add(RfcConfigParameters.PoolSize, _poolSize)");
                _logger.Log($"parms.Add(RfcConfigParameters.PeakConnectionsLimit, _peakConnectionsLimit)");
                _logger.Log($"parms.Add(RfcConfigParameters.ConnectionIdleTimeout, _connectionIdleTimeout)");

            }
            return parms;
        }
    }
}

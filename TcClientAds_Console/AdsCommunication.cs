/*
 To run the application setup :
 _remoteNetId
 _remoteIp
 _remoteRouteName
 _local (ip addres of local computer + *.1.1)

Add route on remote device with TwinCAT to PC where this code is run

Make sure Windows Defender ( Firewall ) is not blocking the connection
 */

namespace TcClientAds_Console
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using TwinCAT;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TcpRouter;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem;

    internal class AdsCommunication
    {
        /*VARIABLES*/
        private bool bRouterIsConfigured = false;
        private bool blocal = false;
        private int _port = 851;
        private AmsNetId _remoteNetId;
        private IPAddress _remoteIp;
        private string _remoteRouteName;
        private AmsNetId _local;
        private AdsClient ads;
        ISymbolLoader loader;

        /*CONSTRUCTOR*/
        public AdsCommunication(int port)
        {
            _port = port;
            blocal = true;
        }
        public AdsCommunication(string remoteNetId, string remoteIp, int port)
        {
            _remoteNetId = new AmsNetId(remoteNetId);
            _remoteIp = IPAddress.Parse(remoteIp);
            _port = port;
        }
        public AdsCommunication(string remoteNetId, string remoteIp, string remoteRouteName, string localNetId, int port)
        {
            bRouterIsConfigured = false;
            _remoteNetId = new AmsNetId(remoteNetId);
            _remoteIp = IPAddress.Parse(remoteIp);
            _remoteRouteName = remoteRouteName;
            _local = new AmsNetId(localNetId);
            _port = port;
        }

        public bool Begin(bool printConsole = false)
        {
            ads = new AdsClient();

            if (bRouterIsConfigured)
            {
                AmsTcpIpRouter _router = new AmsTcpIpRouter(_local);
                CancellationToken cancel = new CancellationToken();
                try
                {
                    _router.AddRoute(new Route(_remoteRouteName, _remoteNetId, new IPAddress[] { _remoteIp }));
                    _router.StartAsync(cancel);
                }
                catch (Exception e)
                {
                    if (printConsole)
                        Console.WriteLine("Error in routing : \n" + e.ToString());
                    return false;
                }
                if (printConsole)
                {
                    Console.WriteLine("router is running " + _router.IsRunning);
                    Console.WriteLine("router is active " + _router.IsActive);
                }
            }

            try
            {
                if (blocal)
                    ads.Connect(_port);
                else
                    ads.Connect(_remoteNetId, _port);
                loader = SymbolLoaderFactory.Create(ads, SymbolLoaderSettings.Default);
                return ads.IsConnected;

            }
            catch (Exception e1)
            {
                if (printConsole)
                {
                    Console.WriteLine("error in ADS connection check AMS Net IP" + _remoteNetId.ToString());
                    Console.WriteLine(e1.ToString());
                }
                return false;
            }
            if (printConsole)
                Console.WriteLine("Ads client is connected " + ads.IsConnected);
        }

        public ISymbolCollection<ISymbol> getAllVariables(bool printConsole = false)
        {
            /*Get all symbols*/
            ISymbolLoader loader = SymbolLoaderFactory.Create(ads, SymbolLoaderSettings.Default);
            ISymbolCollection<ISymbol> allSymbols = loader.Symbols;
            Symbol symbol;
            foreach (ISymbol val in allSymbols)
            {
                if (val.Category != DataTypeCategory.Struct)
                {
                    symbol = (Symbol)loader.Symbols["." + val.InstanceName];
                    if (printConsole)
                        Console.WriteLine(val.InstanceName + " " + val.TypeName + " Value : " + symbol.ReadValue());
                }
                else
                {
                    if (printConsole)
                        Console.WriteLine(val.InstanceName);
                }
            }

            return allSymbols;
        }

        public Symbol GetVariableValue(Symbol symbol)
        {
            if (ads.IsConnected)
            {
                Symbol _symbol = (Symbol)loader.Symbols[symbol.InstanceName];
                return _symbol;
            }
            else
                return null;
        }
        public float GetVariableValue(string variableName)
        {
            if (ads.IsConnected)
            {
                Symbol _symbol = (Symbol)loader.Symbols[variableName];
                float value = (float)_symbol.ReadValue();
                return value;
            }
            else
                return float.NaN;
        }
        public void SetVariable(Symbol symbol)
        {
            if (ads.IsConnected)
            {
                Symbol _symbol = (Symbol)loader.Symbols[symbol.InstanceName];
                _symbol.WriteValue(symbol);
            }
        }
        public void SetVariable(string variableName, string value)
        {
            if (ads.IsConnected)
            {
                Symbol _symbol = (Symbol)loader.Symbols[variableName];
                _symbol.TryWriteValue(value, 100);
            }
        }
    }
}
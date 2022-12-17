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
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Net.Sockets;
    using TwinCAT;
    using TwinCAT.Ads;
    using TwinCAT.Ads.SumCommand;
    using TwinCAT.Ads.TcpRouter;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem;

    internal class Program 
    {
        static AmsNetId _remoteNetId = new AmsNetId("169.254.13.29.1.1");
        static IPAddress _remoteIp = IPAddress.Parse("192.168.58.129");
        static string _remoteRouteName = "VM";
        static AmsNetId _local = new AmsNetId("192.168.58.1.1.1");
        AdsClient ads = new AdsClient();

        static void Main(string[] args)
        {
            AmsTcpIpRouter _router = new AmsTcpIpRouter(_local);
            CancellationToken cancel = new CancellationToken();            
            var Prg = new Program();
            

            try
            {
                _router.AddRoute(new Route(_remoteRouteName, _remoteNetId, new IPAddress[] { _remoteIp }));
                _router.StartAsync(cancel);
            }catch (Exception e)
            {
                Console.WriteLine("Error in routing : \n" + e.ToString());
            }
            
            Console.WriteLine("router is running " + _router.IsRunning);
            Console.WriteLine("router is active " + _router.IsActive);
            try
            {
                Prg.ads.Connect(_remoteNetId, 851);
            }
            catch (Exception e1)
            {
                Console.WriteLine("error in ADS connection check AMS Net IP" + _remoteNetId.ToString());
                Console.WriteLine(e1.ToString());
            }

            Console.WriteLine("Ads client is connected " + Prg.ads.IsConnected);
            /*Get all symbols*/
            ISymbolLoader loader = SymbolLoaderFactory.Create(Prg.ads, SymbolLoaderSettings.Default);
            ISymbolCollection<ISymbol> allSymbols = loader.Symbols;
            Symbol symbol;
            foreach (ISymbol val in allSymbols)
            {
                if (val.Category != DataTypeCategory.Struct)
                {
                    symbol = (Symbol)loader.Symbols["." + val.InstanceName];
                    Console.WriteLine(val.InstanceName + " " + val.TypeName + " Value : " + symbol.ReadValue());
                }else
                {
                    Console.WriteLine(val.InstanceName);
                }
            }
            Console.ReadLine();
        }
    }
}
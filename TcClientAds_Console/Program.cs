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
    internal class Program 
    {
        static void Main(string[] args)
        {
            AdsCommunication plcTest = new AdsCommunication("10.0.10.202.1.1", "172.16.15.101", 851);
            plcTest.Begin(true);
            plcTest.getAllVariables(true);
        }
    }
}
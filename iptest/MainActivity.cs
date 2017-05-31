using Android.App;
using Android.Widget;
using Android.OS;
using Java.Net;
using Android.Runtime;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Net;

namespace iptest
{

    public class IPStuff
    {
        public string Name { get; set; }
        public bool IsUp { get; set; }
        public string IP { get; set;}
        public bool IsVirtual { get; set; }
        public bool IsPP { get; set; }
        public bool IsActive { get; set; }

       public override string ToString()
        {
            return string.Format("[IPStuff: Name={0}, IsUp={1}, IP={2}, IsVirtual={3}, IsPP={4}, IsActive={5}]", Name, IsUp, IP, IsVirtual, IsPP, IsActive);
        }

    }
    [Activity(Label = "iptest", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : ListActivity
    {
        int count = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
           
            var items = GetIPAddressQuick();

            ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, Android.Resource.Id.Text1, items.Select(item => item.ToString()).ToArray());

        }

        public static List<IPStuff> GetIPAddressQuick()
        {
            var stuff = new List<IPStuff>();
            var interfaces = NetworkInterface.NetworkInterfaces;
            IPAddress ipAddr;
            try
            {
                bool hasMore = true;
                int count = 0;
                while (hasMore)
                {
                    count++;
                    if (count > 500)
                        break;

                    var next = interfaces.NextElement().JavaCast<NetworkInterface>();
                    hasMore = interfaces.HasMoreElements;
                    try
                    {
                        if (next.IsLoopback)
                            continue;

						foreach (var address in next.InterfaceAddresses)
                        {
                            if (address.Address == null)
                                continue;

                            if (address.Address.IsLoopbackAddress)
                                continue;

                            var hostAddress = address.Address.HostAddress;

                            if (hostAddress == null)
                            {
                                hostAddress = address.Address.ToString();
                                var splitAddress = hostAddress.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                                //grab the ip as it is all it has, else the last entry
                                if (splitAddress.Length > 0)
                                    hostAddress = splitAddress[splitAddress.Length - 1];
                                //run regex for good measure
                                hostAddress = Regex.Replace(hostAddress, "[^0-9.]", string.Empty);
                            }

                            if (!IPAddress.TryParse(hostAddress, out ipAddr))
                                continue;

                            if (ipAddr.AddressFamily != AddressFamily.InterNetwork)
                                continue;

                            stuff.Add(new IPStuff
                            {
                                IP = hostAddress,
                                IsUp =  next.IsUp,
                                IsVirtual = next.IsVirtual,
                                Name = next.DisplayName,
                                IsPP = next.IsPointToPoint
                            });
                        }

                    }
                    catch
                    {
                    }
                    finally
                    {
                        next.Dispose();
                    }
                }

            }
            catch
            {
            }
            finally
            {
                interfaces.Dispose();
            }

            var cm = ConnectivityManager.FromContext(Application.Context);
            var link = cm.GetLinkProperties(cm.ActiveNetwork);


            var wlan = stuff.FirstOrDefault(ip => !string.IsNullOrWhiteSpace(ip.Name) && !string.IsNullOrWhiteSpace(link.InterfaceName) && ip.Name == link.InterfaceName.ToLower());

            if(wlan != null)
                wlan.IsActive = true;

           
            return stuff;
        }
    }
}


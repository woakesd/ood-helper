using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;
using PusherServer;

namespace OodHelper.Website
{
    class PushResultNotification
    {
        public static void ResultPublished()
        {
            //var _pusher = new Pusher("44473", "4ea1508f11de611598d3", "91be27c6919a7451209b");
            var _pusher = new Pusher(Settings.PusherAppId, Settings.PusherAppKey, Settings.PusherAppSecret);
            ITriggerResult _res = _pusher.Trigger("PEYC-Results", "Results-Published", new { message = "Results Published" });
        }
    }
}

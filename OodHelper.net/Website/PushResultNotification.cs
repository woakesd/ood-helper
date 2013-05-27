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
            if (Settings.PusherAppId != string.Empty && Settings.PusherAppKey != string.Empty && Settings.PusherAppSecret != string.Empty)
            {
                var _pusher = new Pusher(Settings.PusherAppId, Settings.PusherAppKey, Settings.PusherAppSecret);
                ITriggerResult _res = _pusher.Trigger("PEYC-Results", "Results-Published", new { message = "Results Published" });
            }
        }
    }
}

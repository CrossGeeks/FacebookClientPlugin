using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.FacebookClient.Abstractions
{
    public class FacebookShareVideo
    {
        public FacebookShareVideo(Uri localUrl)
        {
            LocalUrl = localUrl;
        }

        public Uri LocalUrl { get; }
    }
}

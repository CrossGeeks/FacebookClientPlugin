using System;

namespace Plugin.FacebookClient
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

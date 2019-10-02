using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.FacebookClient
{
    public class FacebookShareLinkContent : FacebookShareContent
    {
        public FacebookShareLinkContent(string quote, Uri contentLink, string hashtag = null, string placeId = null, string[] peopleIds = null, string @ref = null) : base(placeId, peopleIds, @ref, hashtag, contentLink)
        {
            Quote = quote;
        }

        public string Quote { get; set; }
    }
}

using System;

namespace Plugin.FacebookClient
{
    public class FacebookShareVideoContent : FacebookShareContent
    {
        public FacebookShareVideoContent(FacebookShareVideo video, Uri contentLink = null,string hashtag = null,string placeId =null, string[] peopleIds = null, string @ref = null) : base(placeId, peopleIds, @ref, hashtag, contentLink)
        {
            Video = video;
        }

        public FacebookShareVideo Video { get; set; }

    }
}

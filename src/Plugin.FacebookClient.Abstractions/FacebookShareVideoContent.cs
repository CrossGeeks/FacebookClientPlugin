using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.FacebookClient.Abstractions
{
    public class FacebookShareVideoContent : FacebookShareContent
    {
        public FacebookShareVideoContent(FacebookShareVideo video, FacebookSharePhoto previewPhoto = null, Uri contentLink = null,string hashtag = null,string placeId =null, string[] peopleIds = null, string @ref = null) : base(placeId, peopleIds, @ref, hashtag, contentLink)
        {
            Video = video;
            PreviewPhoto = previewPhoto;
        }

        public FacebookShareVideo Video { get; set; }

        public FacebookSharePhoto PreviewPhoto { get; set; }

    }
}

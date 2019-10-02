using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.FacebookClient
{
    public abstract class FacebookShareContent
    {
        public FacebookShareContent(string placeId, string[] peopleIds, string @ref, string hashtag, Uri contentLink)
        {
            PlaceId = placeId;
            PeopleIds = peopleIds;
            Ref = @ref;
            Hashtag = hashtag;
            ContentLink = contentLink;
        }

        public string PlaceId { get; set; }

        public string[] PeopleIds { get; set; }

        public string Ref { get; set; }

        public string Hashtag { get; set; }

        public Uri ContentLink { get; set; }
    }
}

using System;

namespace Plugin.FacebookClient
{
    public class FacebookSharePhoto 
    {
        public FacebookSharePhoto(string caption, byte[] image)
        {
            Caption = caption;
            Image = image;
        }

        public FacebookSharePhoto(string caption, Uri imageUrl)
        {
            Caption = caption;
            ImageUrl = imageUrl;
        }

        public string Caption { get;  }
        public byte[] Image { get;  }
        public  Uri ImageUrl { get;  }
    }
}

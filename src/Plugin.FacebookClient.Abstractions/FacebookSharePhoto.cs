using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.FacebookClient.Abstractions
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

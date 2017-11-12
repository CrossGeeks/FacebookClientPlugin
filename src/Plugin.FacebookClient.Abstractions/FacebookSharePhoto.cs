using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.FacebookClient.Abstractions
{
    public class FacebookSharePhoto 
    {
        public FacebookSharePhoto(string caption, byte[] image, Uri imageUrl = null)
        {
            Caption = caption;
            Image = image;
            ImageUrl = imageUrl;
        }

       public string Caption { get; set; }
       public byte[] Image { get; set; }
       public  Uri ImageUrl { get; set; }
    }
}

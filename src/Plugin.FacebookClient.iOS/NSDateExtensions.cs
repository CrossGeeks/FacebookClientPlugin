using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Plugin.FacebookClient
{
    internal static class NSDateExtensions
    {
        static DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ToDateTime(this NSDate date)
        {
            var utcDateTime = reference.AddSeconds(date.SecondsSinceReferenceDate);
            var dateTime = utcDateTime.ToLocalTime();
            return dateTime;
        }

        public static NSDate ToNSDate(this DateTime datetime)
        {
            var utcDateTime = datetime.ToUniversalTime();
            var date = NSDate.FromTimeIntervalSinceReferenceDate((utcDateTime - reference).TotalSeconds);
            return date;
        }
    }
}
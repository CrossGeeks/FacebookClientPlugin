using System;
namespace Plugin.FacebookClient
{
    public enum FacebookGameRequestActionType
    {
        None = 0,
        Send = 1,
        AskFor = 2,
        Turn = 3
    }

    public enum FacebookGameRequestFilter
    {
        None = 0,
        AppUsers = 1,
        AppNonUsers = 2
    }

    public class FacebookGameRequestContent
    {
        public string ObjectId { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public FacebookGameRequestFilter Filters { get; set; }

        public FacebookGameRequestActionType ActionType { get; set; }

        public string Data { get; set; }

        public string[] Recipients { get; set; }

        public string[] RecipientSuggestions { get; set; }
    }
}

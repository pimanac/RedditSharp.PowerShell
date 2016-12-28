using Newtonsoft.Json.Linq;
using RedditSharp.Things;

namespace RedditSharp.PowerShell
{
    internal static class Helpers
    {
        public static Thing GetSkeletonThing<T>(string id)
        {
            string json;
            JToken token;

            if (typeof(T) == typeof(Comment))
            {
                json = "{ kind: 't1', data: { id : '" + id + "', name: 't1_" + id + "' } }";
                token = JToken.Parse(json);
                return new Comment().Init(Session.Reddit, token, Session.WebAgent, null);
            }

            if (typeof(T) == typeof(Post))
            {
                json = "{ kind: 't3', data: { id : '" + id + "', name: 't3_" + id + "' } }";
                token = JToken.Parse(json);
                return new Post().Init(Session.Reddit, token, Session.WebAgent);
            }
            return new Thing();
        }
    }
}

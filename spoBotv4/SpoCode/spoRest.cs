using System;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using Microsoft.Bot.Builder.Dialogs;
using AuthBot;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using spoBotv4.Models;

namespace spoBotv4.SpoCode
{
    [Serializable]
    public class spoRest
    {
        //Variables used by the spoRest class.
        private static Lazy<string> aDResourceId = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"]);
        private static string spoBaseUri = ConfigurationManager.AppSettings["Sharepoint.BaseUri"];
        private static string spoListName = ConfigurationManager.AppSettings["Sharepoint.ListName"];
        public static string spoResult;
        public static string jsonResult;

        internal static async Task<string> o365Connect(string var1, string var2, IDialogContext context)
        {
       
            //Calls the getSpoList method to generate our final result set.
            JToken list = getSpoList(var1, var2, context);
            IEnumerable<JToken> childTokens = list.SelectTokens("*");

            //The final result set is put into a list that can be managed as a native .net object
            List<spoJson> someList = new List<spoJson>();

            foreach (JToken item in childTokens)
            {
                //The solution only wants specific tokens to return to the user.  This section does that.
                var i = item.Count();
                for (int j = 0; j < i; j++)
                {
                    spoJson line = new spoJson();
                    line.Property = (string)list.SelectToken("results[" + j + "].PropertyName"); //need to modify and add based on SPO List Properties you want to return.
                    someList.Add(line);
                }
                //reconvert the list to a JSON serialized string.
                jsonResult = JsonConvert.SerializeObject(someList, Formatting.Indented);
            }
            return jsonResult;

        }
        public static JToken getSpoList(string var1, string var2, IDialogContext context)
        {
            var spoUri = new Uri(spoBaseUri);
            var accessToken = context.GetAccessToken(aDResourceId.Value);

            using (var client = new WebClient())
            {
                client.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
                client.Headers.Add("Authorization", "Bearer " + accessToken.Result);
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json;odata=verbose");
                client.Headers.Add(HttpRequestHeader.Accept, "application/json;odata=verbose");

                Uri endPointUri = null;
                endPointUri = new Uri(spoUri, string.Format(spoUri + "_api/web/lists/getbytitle('" + spoListName + "')/items/?$filter=((SomeSharepointListPropertyName eq '" + var1 + "') and (AnotherSharepointListProperty eq '" + var2 + "'))"));
                spoResult = client.DownloadString(endPointUri);

                JToken t = null;
                t = JToken.Parse(spoResult);
                return t["d"];
            }
        }
    }
}
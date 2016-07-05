using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using spoBotv4.SpoCode;
using spoBotv4.Models;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace spoBotv4.Dialogs
{
    [Serializable]
    public class LuisDialog
    {
        //Multiple constants and static variables used by the LuisDialog class
        public const string Some_Entity_Type = "a LUIS Entity Type";
        public static string spoReturn;
        public static string var1;
        public static string var2;
        private static Rootobject _Data;

        //User query is sent to the LUIS.ai service via a rest call.
        public static async Task<Rootobject> ParseUserInput(string strInput)
        {
            //modifies the query to proper Uri format
            string strEsc = Uri.EscapeUriString(strInput);

            using (var client = new HttpClient())
            {
                //This string contains a unique client ID and subscription Key used by LUIS to identify the specific LUIS model
                string luisUri = "https://api.projectoxford.ai/luis/v1/application?id=<Your_LUIS_Model_ID>&subscription-key=<Your_LUIS_Model_Key>&q=" + strEsc;
                HttpResponseMessage msg = await client.GetAsync(luisUri);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonResponse = await msg.Content.ReadAsStringAsync();
                    _Data = JsonConvert.DeserializeObject<Rootobject>(JsonResponse);
                    return _Data;
                }
            }
            return null;
        }

        //A valid intent is passed from ActionDialog class 
        public static async Task<string> GoodIntent(IDialogContext context, Rootobject strLuis)
        {
            {
                //Serializes LUIS results for parsing using JSON
                string luisString = JsonConvert.SerializeObject(strLuis);
                JObject luisJObject = JObject.Parse(luisString);

                //Enumerates the sub-tokens within the entities token in the LUIS results.  Results are placed in a list.
                IEnumerable<JToken> childToken = luisJObject.SelectTokens("entities");

                List<Entity> luisList = new List<Entity>();

                foreach (JToken item in childToken)
                {
                    var i = item.Count();
                    for (int j = 0; j < i; j++)
                    {
                        Entity line = new Entity();
                        line.entity = (string)item.SelectToken("[" + j + "].entity");
                        line.type = (string)item.SelectToken("[" + j + "].type");
                        luisList.Add(line);
                    }
                }

                //The list is looped through to define specific entity type and their related values
                foreach (var entity in luisList)
                {
                    if (entity.type == Some_Entity_Type)
                    {
                        var1 = entity.entity.ToString();
                    }
                }
                //The first intent returned by the LUIS Model always has the highest score.  Therefore we can assume [0] is correct for what the query is searching for....i.e., Breakfast, Lunch, Pizza, etc....
                var2 = strLuis.intents[0].intent;

                //Different REST queries must be generated based on the  variable.  For the following we pass var2 to build the REST URI.
                if (var2 == "Some LUIS Intent")
                {
                    spoReturn = await spoRest.o365Connect(var1, var2, context);
                    return spoReturn;
                }
                else
                {
                    string error1 = "This is an error message";
                    await context.PostAsync(error1);
                }
                return null;
            }
        }
    }
}

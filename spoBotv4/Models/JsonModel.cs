//This is the JSON model for the LUIS model and building the LUIS results for presentation back to the end user.
namespace spoBotv4.Models
{
    public class Rootobject
    {
        public string query { get; set; }
        public Intent[] intents { get; set; }
        public Entity[] entities { get; set; }
    }

    public class Intent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public float score { get; set; }
    }

    public class spoJson
    {
        public string Property { get; set; }
    }
}
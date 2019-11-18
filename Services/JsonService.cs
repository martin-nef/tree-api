using Newtonsoft.Json;
using tree_api.Interfaces;

namespace tree_api.Services {
    public class JsonService : IJsonService {
        private readonly JsonSerializerSettings _jsonSettings;

        public JsonService (JsonSerializerSettings jsonSettings) {
            _jsonSettings = jsonSettings;
        }

        public T Deserialize<T> (string json) {
            return JsonConvert.DeserializeObject<T> (json, _jsonSettings);
        }

        public string Serialize (object obj) {
            return JsonConvert.SerializeObject (obj, _jsonSettings);
        }
    }
}

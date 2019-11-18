namespace tree_api.Interfaces {
    public interface IJsonService {
        string Serialize (object obj);
        T Deserialize<T> (string json);
    }
}

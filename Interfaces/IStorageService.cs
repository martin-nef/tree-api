using tree_api.Models;

namespace tree_api.Interfaces {
    public interface IStorageService {
        Tree GetTree ();
        void SaveTree (Tree tree);
        void DeleteTree ();
    }
}

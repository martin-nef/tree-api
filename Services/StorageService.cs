using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using tree_api.Interfaces;
using tree_api.Models;

namespace tree_api.Services {
    public class StorageService : IStorageService {
#warning TODO : replace storage with a nosql database where each node is stored separately, with children represented by their ID's (their names)
        /// Note: for ACID compliance, will need to lock on delete/edit, so reads get consistent data. Reads can be parallel when nothing is editing.
        /// for BASE compliance, will need to 
        private readonly IJsonService _jsonService;
        private const string TreeFileName = "tree.json";

        public StorageService (IJsonService jsonService, IWebHostEnvironment env) {
            _jsonService = jsonService;
            if (!File.Exists (TreeFileName) && env.IsDevelopment ()) {
                SaveTree(new Tree{ });
                // SaveTree (new Tree {
                //     Root = new Node ("A",
                //         new Node ("B",
                //             new Node ("E"),
                //             new Node ("F"),
                //             new Node ("G")),
                //         new Node ("C",
                //             new Node ("H")),
                //         new Node ("D",
                //             new Node ("I",
                //                 new Node ("K"),
                //                 new Node ("L"),
                //                 new Node ("M")),
                //             new Node ("J"))
                //     )
                // });
            }
        }

        public void DeleteTree () {
            File.Delete (TreeFileName);
        }

        public Tree GetTree () {
            try {
                return (Tree) _jsonService.Deserialize<Tree> (File.ReadAllText (TreeFileName));
            } catch (Exception e) {
                throw new Exception ($"Failed to get tree", e);
            }
        }

        public void SaveTree (Tree tree) {
            try {
                File.WriteAllText (TreeFileName, _jsonService.Serialize (tree));
            } catch (Exception e) {
                throw new Exception ($"Failed to save tree", e);
            }
        }
    }
}
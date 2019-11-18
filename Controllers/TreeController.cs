using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using tree_api.Interfaces;
using tree_api.Models;

namespace tree_api.Controllers {
    [ApiController]
    [Route ("Tree")]
    public class SortedTreeController : ControllerBase {
        private readonly ILogger<SortedTreeController> _logger;
        private readonly IStorageService _storage;
        private readonly IJsonService _jsonService;
        private readonly IWebHostEnvironment _env;

        public SortedTreeController (ILogger<SortedTreeController> logger, IStorageService storage, IJsonService jsonService, IWebHostEnvironment env) {
            _logger = logger;
            _storage = storage;
            _jsonService = jsonService;
            _env = env;
        }

        [HttpGet]
        [Route ("Get/{name}")]
        public string Get (string name) {
            return HandleAction (() => _storage.GetTree ().Root.GetNodeByName (name), $"Failed to retreive tree node named {name}");
        }

        [HttpDelete]
        [Route ("Delete")]
        public string Delete (string name) {
            return HandleAction (action: () => {
                var tree = _storage.GetTree ();
                tree.Root.DeleteNodeByName (name);
                _storage.SaveTree (tree);
            }, failMessage: $"Failed to delete tree node named {name}");
        }

        [HttpPut]
        [Route ("Update")]
        public string Update (Node node) {
            return HandleAction (action: () => {
                var tree = _storage.GetTree ();
                var oldNode = tree.Root.GetNodeByName (node.Name);
                oldNode.UpdateWith (node);
                _storage.SaveTree (tree);
            }, failMessage: $"Failed to update node named {node.Name}");
        }

        [HttpPost]
        [Route ("Create")]
        public string Create ([FromBody] Node node, string parentName = null) {
            return HandleAction (action: () => {
                    var tree = _storage.GetTree ();
                    if (parentName != null) {
                        var oldNode = tree.Root.GetNodeByName (name: parentName);
                        oldNode.AddChild (oldNode);
                    } else {
                        tree.Root.AddChild (node);
                    }
                    _storage.SaveTree (tree);
                },
                failMessage: $"Failed to add node (with name {node?.Name}) to the tree");
        }

        private string HandleAction (Func<object> action, string failMessage) {
            try {
                var result = action?.Invoke ();
                return _jsonService.Serialize (result);
            } catch (Exception e) {
                _logger.LogError (e, failMessage);
                if (_env.IsDevelopment ()) {
                    throw;
                } else {
                    return _jsonService.Serialize (new {
                        success = false,
                            message = failMessage,
                    });
                }
            }
        }

        private string HandleAction (Action action, string failMessage) {
            return HandleAction (action: () => {
                action ();
                return null;
            }, failMessage : failMessage);
        }
    }
}
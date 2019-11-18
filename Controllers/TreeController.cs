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
        [Route ("Get/{name?}")]
        public string Get (string name = null) {
            return HandleAction (() => {
                var tree = _storage.GetTree ();
                if (string.IsNullOrWhiteSpace (name)) {
                    return tree.Root;
                } else {
                    return tree.Root.GetNodeByName (name);
                }
            }, $"Failed to retreive tree node named {name}");
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

        [HttpPost]
        [Route ("Update/{name}/{value}")]
        public string Update (string name, string value) {
            return Update (new Node (name) { Value = value, Children = null });
        }

        [HttpPost]
        [Route ("Update")]
        public string Update ([FromBody] Node node) {
            return HandleAction (action: () => {
                var tree = _storage.GetTree ();
                var oldNode = tree.Root.GetNodeByName (node.Name);
                oldNode.UpdateWith (node);
                _storage.SaveTree (tree);
            }, failMessage: $"Failed to update node named {node.Name}");
        }

        [HttpPut]
        [Route ("Create/{name}/{value}/{parentName?}")]
        public string Create (string name, string value, string parentName = null) {
            return Create (new Node (name: name) { Value = value }, parentName);
        }

        [HttpPost]
        [Route ("Create/{parentName?}")]
        public string Create ([FromBody] Node node, string parentName = null) {
            return HandleAction (action: () => {
                    var tree = _storage.GetTree ();
                    if (parentName != null) {
                        var oldNode = tree.Root.GetNodeByName (name: parentName);
                        oldNode.AddChild (oldNode);
                    } else {
                        tree.Root = node;
                    }
                    _storage.SaveTree (tree);
                },
                failMessage: $"Failed to add node (with name {node?.Name}) to the tree");
        }

        private string HandleAction (Func<object> action, string failMessage) {
            try {
                var result = action?.Invoke ();
                return _jsonService.Serialize (new {
                    success = true,
                    result = result
                });
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
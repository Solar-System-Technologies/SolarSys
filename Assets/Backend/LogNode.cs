using System;
using System.Linq;
using System.Collections.Generic;
using static Backend.StringExtensions;

using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Backend
{
    public enum ChangeEnum
    {
        AddNode,
        AddEdge,
        UpdateNode,
        UpdateEdge,
        DeleteNode,
        DeleteEdge,
        // TODO to remove below 
        Create,
        Update,
        Delete
    }
    public class LogNode
    {
        public string Body { get; set; }
        public ChangeEnum Change { get; private set; }
        public Guid Id { get; private set; }
        public DateTime TimeStamp { get; private set; }


        public LogNode(ChangeEnum change)
        {
            Change = change;
            Id = Guid.NewGuid();
            TimeStamp = DateTime.UtcNow;
        }

        // Old code , to delete
        public LogNode(ChangeEnum change, string body)
        {
            Change = change;
            Body = body;
        }

        public LogNode(string body, ChangeEnum change, Guid id, DateTime timeStamp)
        {
            Body = body;
            Change = change;
            Id = id;
            TimeStamp = timeStamp;
        }

        public static LogNode FromJObject(GraphProject project, JObject obj)
        {
            Enum.TryParse((string)obj["change"], out ChangeEnum change);
            string body = (string)obj["body"];
            Guid guid = Guid.Parse((string)obj["guid"]);
            DateTime timeStamp = DateTime.Parse((string)obj["timestamp"]);

            return new LogNode(body, change, guid, timeStamp);
        }

        // public static void Validate<T>(T obj)
        // {
        //     var results = new List<ValidationResult>();

        //     var validate = Validator.TryValidateObject(obj, new ValidationContext(obj), results, true);

        //     if (!validate)
        //     {
        //         Console.WriteLine(String.Join("\n", results.Select(o => o.ErrorMessage)));
        //         Debug.Log("Validation error!");
        //     }
        // }
    }
    public class NodeCreationLog : LogNode
    {
        public NodeCreationLog(NodeCreationSchema log) : base(ChangeEnum.AddNode)
        {
            //Validate(log);
            Body = JsonConvert.SerializeObject(log);
        }
    }

    public class EdgeCreationLog : LogNode
    {
        public EdgeCreationLog(EdgeCreationSchema log) : base(ChangeEnum.AddEdge)
        {
            //Validate(log);
            Body = JsonConvert.SerializeObject(log);
        }
    }

    public class NodeUpdateLog : LogNode
    {
        public NodeUpdateLog(NodeUpdateSchema log) : base(ChangeEnum.UpdateNode)
        {
            //Validate(log);
            //var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            Body = JsonConvert.SerializeObject(log);
        }
    }

    public class EdgeUpdateLog : LogNode
    {
        public EdgeUpdateLog(EdgeUpdateSchema log) : base(ChangeEnum.UpdateEdge)
        {
            // Validate(log);
            // var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            Body = JsonConvert.SerializeObject(log);
        }
    }

    public class NodeDeletionLog : LogNode
    {
        public NodeDeletionLog(NodeCreationSchema log) : base(ChangeEnum.DeleteNode)
        {
            // Validate(log);
            Body = JsonConvert.SerializeObject(log);
        }
    }

    public class EdgeDeletionLog : LogNode
    {
        public EdgeDeletionLog(EdgeCreationSchema log) : base(ChangeEnum.DeleteEdge)
        {
            // Validate(log);
            Body = JsonConvert.SerializeObject(log);
        }
    }
}
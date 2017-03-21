using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// TFS Generic Response Objects
/// </summary>
namespace TFS_Release_Notes_Generator.Models
{
    public class Response<TEntity>
    {
        public int count { get; set; }

        [JsonProperty("value")]
        public List<TEntity> items { get; set; }
    }    
}

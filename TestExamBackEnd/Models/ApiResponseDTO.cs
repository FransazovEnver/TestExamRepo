using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TestExamBackEnd.Models
{
    public class ApiResponseDTO
    {

        [JsonPropertyName("msg")]
        public string Msg { get; set; }

        [JsonPropertyName("storyid")]
        public string? StoryId { get; set; }
    }
}

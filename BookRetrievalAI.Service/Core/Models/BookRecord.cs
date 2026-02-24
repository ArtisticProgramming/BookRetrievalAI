using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookRetrievalAI.Service.Core.Models
{
    public class BookRecord
    {
        public string Id { get; set; }
        public string FreebaseId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string PublicationDate { get; set; }
        public Dictionary<string, string> Genres { get; set; }
        public string Summary { get; set; }

        public override string ToString()
        {
            return $"[{Id}] {Title} by {Author} ({PublicationDate})";
        }
    }
}

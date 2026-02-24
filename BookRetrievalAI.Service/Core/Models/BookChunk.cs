using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookRetrievalAI.Service.Core.Models
{
    public class BookChunk
    {
        public string BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string PublicationDate { get; set; }
        public List<string> GenreNames { get; set; }
        public int ChunkIndex { get; set; }
        public int TotalChunks { get; set; }
        public string ChunkText { get; set; }
        public string FullContext { get; set; }
    }
}

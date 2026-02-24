using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookRetrievalAI.Service.Core.Models
{
    public class SearchResult
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string ChunkText { get; set; }
        public int ChunkIndex { get; set; }
        public int TotalChunks { get; set; }
        public float Score { get; set; }

        public override string ToString()
        {
            return $"📚 {Title} by {Author}\n" +
                   $"   Chunk {ChunkIndex + 1}/{TotalChunks} | Score: {Score:F4}\n" +
                   $"   {ChunkText.Substring(0, Math.Min(200, ChunkText.Length))}...\n";
        }
    }
}

using BookRetrievalAI.Service.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookRetrievalAI.Service.Core.Interfaces
{
    public interface IBookDataParser
    {
        List<BookRecord> ParseBooksFile(string filePath);
    }
}

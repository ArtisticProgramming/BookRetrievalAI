using BookRetrievalAI.Service.Core.Interfaces;
using BookRetrievalAI.Service.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookRetrievalAI.Service.Services.Parsing
{

    public class BookDataParser : IBookDataParser
    {
        public List<BookRecord> ParseBooksFile(string filePath)
        {
            var books = new List<BookRecord>();
            int lineNumber = 0;
            int successCount = 0;
            int errorCount = 0;

            Console.WriteLine($"Reading file: {filePath}");

            foreach (var line in File.ReadLines(filePath))
            {
                lineNumber++;

                try
                {
                    var columns = line.Split('\t');

                    if (columns.Length < 7)
                    {
                        Console.WriteLine($"Line {lineNumber}: Skipping - insufficient columns ({columns.Length})");
                        errorCount++;
                        continue;
                    }

                    var book = new BookRecord
                    {
                        Id = columns[0].Trim(),

                        FreebaseId = columns[1].Trim(),

                        Title = columns[2].Trim(),

                        Author = columns[3].Trim(),

                        PublicationDate = columns[4].Trim(),

                        Genres = ParseGenresJson(columns[5]),

                        Summary = string.Join("\t", columns.Skip(6)).Trim()
                    };

                    if (string.IsNullOrWhiteSpace(book.Title) ||
                        string.IsNullOrWhiteSpace(book.Summary))
                    {
                        Console.WriteLine($"Line {lineNumber}: Skipping - missing title or summary");
                        errorCount++;
                        continue;
                    }

                    books.Add(book);
                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Line {lineNumber}: Error - {ex.Message}");
                    errorCount++;
                }
            }

            Console.WriteLine($"\nParsing complete:");
            Console.WriteLine($"Success: {successCount} books");
            Console.WriteLine($"Errors: {errorCount} lines");

            return books;
        }

        private static Dictionary<string, string> ParseGenresJson(string genresJson)
        {
            try
            {
                var genres = JsonSerializer.Deserialize<Dictionary<string, string>>(genresJson);
                return genres ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }
    }
}

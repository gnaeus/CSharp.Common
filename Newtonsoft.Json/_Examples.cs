using Newtonsoft.Json;
using Newtonsoft.Json.Common.Converters;

partial class _Examples
{
    class Book
    {
        [JsonConverter(typeof(RawJsonConverter))]
        public string Chapters { get; set; }
    }

    class BookService
    {
        public string GetBookJson()
        {
            var book = new Book
            {
                Chapters = "[1, 2, 3, 4, 5]",
            };

            return JsonConvert.SerializeObject(book);
            // {"Chapters": [1, 2, 3, 4, 5]}
            // instead of
            // {"Chapters": "[1, 2, 3, 4, 5]"}
        }
    }
}

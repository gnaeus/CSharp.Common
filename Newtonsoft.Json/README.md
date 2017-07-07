### RawJsonConverter
Custom value converter for passing string properties as RAW JSON values.

```cs
using Newtonsoft.Json.Common.Converters;

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
```

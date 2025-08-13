using System.Text.Json.Serialization;

namespace LoggingModule.Models.DTOs
{
    public class Pagination<T>
    {
        [JsonPropertyName("pagination")]
        public PaginationDetails PaginationDetails { get; set; }

        public List<T> Result { get; set; } = [];
    }

    public class PaginationDetails
    {
        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);

        public bool HasPreviousPage => CurrentPage > 1;

        public bool HasNextPage => CurrentPage < TotalPages;
    }
}

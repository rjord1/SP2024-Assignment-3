using System.ComponentModel.DataAnnotations;

namespace SP2024_Assignment_3.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Display(Name = "IMDB Link")]
        public string? IMDB { get; set; }
        public string? Genre { get; set; }

        [Display(Name = "Release Year")]
        public int? ReleaseYear { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Film Poster")]
        public byte[]? IMG { get; set; }


    }
}

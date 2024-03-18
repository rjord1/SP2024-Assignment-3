using System.ComponentModel.DataAnnotations;

namespace SP2024_Assignment_3.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string? Gender { get; set; }
        public string? Age { get; set; }
        public string? IMDB { get; set; }

        [DataType(DataType.Upload)]
        public byte[]? Photo { get; set; }
    }
}

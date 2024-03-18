using System;

namespace SP2024_Assignment_3.Models
{
    public class MovieDetailsVM
    {
        public Movie movie { get; set; }
        public List<Actor> actors { get; set; }
        public List<string> Sentiments { get; set; } // List of sentiment strings
        public List<string> Posts { get; set; } // List of post strings
    }
}

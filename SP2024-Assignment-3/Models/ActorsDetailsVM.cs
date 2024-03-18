namespace SP2024_Assignment_3.Models
{
    public class ActorsDetailsVM
    {
        public Actor actor { get; set; }
        public List<Movie> movies { get; set; }
        public List<string> Sentiments { get; set; } // List of sentiment strings
        public List<string> Posts { get; set; } // List of post strings
    }
}

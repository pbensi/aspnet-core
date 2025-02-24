namespace app.entities
{
    public class OpenResourcePath
    {
        public int Id { get; set; }
        public string RequestMethod { get; set; }
        public string RequestPath { get; set; }
        public string Description { get; set; }
        public string AllowedRole { get; set; }
    }
}

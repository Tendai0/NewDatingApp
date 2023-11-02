namespace API.Helpers
{
    public class MessageParams:PaginationParms
    {
        public string Username { get; set; }
        public string Container { get; set; }
    }
}
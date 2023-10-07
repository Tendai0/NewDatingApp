namespace API.Helpers
{
    public class LikesParams:PaginationParms
    {
      public int UserId { get; set; }
      public string Predicate { get; set; }
    }
}
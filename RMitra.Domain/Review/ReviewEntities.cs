namespace RMitra.Domain.Review;

public class Review
{
    public Guid Id { get; set; }
    public Guid OrderGuid { get; set; }
    public Guid KitchenGuid { get; set; }
    public Guid CustomerUserId { get; set; }
    public int Rating { get; set; }
    public int? RiderRating { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }
}

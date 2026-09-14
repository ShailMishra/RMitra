using System.ComponentModel.DataAnnotations;

namespace RMitra.Application.Review;

public class CreateReviewRequest
{
    [Required] public Guid OrderId { get; set; }
    [Range(1, 5)] public int Rating { get; set; }
    [MaxLength(1000)] public string? Comments { get; set; }
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid KitchenId { get; set; }
    public Guid CustomerUserId { get; set; }
    public int Rating { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }
}

public interface IReviewService
{
    Task<ReviewDto> CreateAsync(Guid userId, CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReviewDto>> GetByKitchenAsync(Guid kitchenId, CancellationToken cancellationToken = default);
    Task<ReviewDto?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}

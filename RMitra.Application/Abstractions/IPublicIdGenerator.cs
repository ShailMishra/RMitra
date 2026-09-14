namespace RMitra.Application.Abstractions;

public interface IPublicIdGenerator
{
    Task<string> NextAsync(string prefix, CancellationToken cancellationToken = default);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
    void ValidateStrength(string password);
}

public interface IGeoCalculator
{
    double DistanceKm(double lat1, double lon1, double lat2, double lon2);
}

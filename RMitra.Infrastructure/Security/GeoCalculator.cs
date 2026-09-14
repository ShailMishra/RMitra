using RMitra.Application.Abstractions;

namespace RMitra.Infrastructure.Security;

public class GeoCalculator : IGeoCalculator
{
    public double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return Math.Round(r * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)), 2);
    }

    private static double ToRad(double value) => value * Math.PI / 180;
}

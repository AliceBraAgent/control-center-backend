using System.Text.RegularExpressions;
using RoutingV3.Domain.Models;

namespace RoutingV3.Engine;

public class PostalCodeMatcher
{
    /// <summary>
    /// Finds all PostalCodeAreas that match a given country + postal code.
    /// Pattern is treated as a regex anchored to start and end.
    /// </summary>
    public List<PostalCodeArea> FindMatches(string country, string postalCode, IEnumerable<PostalCodeArea> areas)
    {
        return areas
            .Where(a => a.Country.Equals(country, StringComparison.OrdinalIgnoreCase)
                        && IsMatch(postalCode, a.Pattern))
            .ToList();
    }

    public bool IsMatch(string postalCode, string pattern)
    {
        try
        {
            var regex = new Regex($"^{pattern}$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
            return regex.IsMatch(postalCode);
        }
        catch (RegexParseException)
        {
            return false;
        }
    }
}

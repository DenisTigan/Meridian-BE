using System.Text.RegularExpressions;

namespace MeridianEmployeeHub.Services.Wiki
{
    public static class SlugGenerator
    {
        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Convert to lower case
            string slug = input.ToLowerInvariant();

            // Replace non-alphanumeric characters with empty string, but keep spaces
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Replace spaces with hyphens
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');

            return slug;
        }
    }
}

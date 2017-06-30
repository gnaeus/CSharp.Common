using System;

namespace Common.Helpers
{
    public static class UriHelper
    {
        /// <summary>
        /// "http://localhost/SomeApp" => "localhost"
        /// </summary>
        public static string GetHost(string uriString)
        {
            Uri uri;
            return Uri.TryCreate(uriString, UriKind.Absolute, out uri) ? uri.DnsSafeHost : null;
        }

        /// <summary>
        /// <para>Works with Absolute and Relative URLs</para>
        /// <para>"http://localhost/SomeApp" => "http://localhost/SomeApp/"</para>
        /// </summary>
        public static string AddTrailingSlash(string url)
        {
            if (String.IsNullOrWhiteSpace(url)) {
                return url;
            }
            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute)) {
                return url;
            }
            return  url.EndsWith("/") ? url : url + "/";
        }

        /// <summary>
        /// ("http://localhost:8080/SomeApp", "127.0.0.1") => "http://127.0.0.1:8080/SomeApp"
        /// </summary>
        public static string ChangeHost(string absoluteUrl, string host)
        {
            var builder = new UriBuilder(absoluteUrl) {
                Host = host,
            };

            if (builder.Uri.IsDefaultPort) {
                // exclude :80 port
                builder.Port = -1;
            }

            return builder.ToString();
        }

        /// <summary>
        /// "http://localhost/SomeApp" == "http://localhost/someapp/"
        /// </summary>
        public static bool CanonicalEqual(string url1, string url2)
        {
            return Uri.Compare(
                new Uri(AddTrailingSlash(url1)), new Uri(AddTrailingSlash(url2)),
                UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase
            ) == 0;
        }
    }
}

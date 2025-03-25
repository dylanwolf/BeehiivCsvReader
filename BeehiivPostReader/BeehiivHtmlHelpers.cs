using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;

namespace BeehiivPostReader
{
    internal static class BeehiivHtmlHelpers
    {
        public static string ParseText(this string blockHtml)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(blockHtml);

            var node = doc.DocumentNode;

            try
            {
                var aHrefs = node.SelectNodes("//a[@class=\"link\"]").ToArray();

                foreach (var aHref in aHrefs)
                {
                    aHref.RemoveClass("link");

                    var url = aHref.GetAttributes(new string[] { "href" }).FirstOrDefault();
                    if (url != null)
                    {
                        url.Value = CleanUrl(url.Value);
                    }

                    var span = aHref.SelectSingleNode("//span");
                    if (span != null)
                    {
                        aHref.RemoveChild(span);
                        aHref.InnerHtml = span.InnerHtml;
                    }
                }
            }
            catch { }

            return doc.DocumentNode.InnerHtml.Trim();
        }

        public static string CleanUrl(this string url)
        {
            var uri = new UriBuilder(url);

            var queryString = uri.Query.Length >= 1 ? uri.Query.Substring(1) : string.Empty;

            var parts = queryString.Split("&");
            var kvps = parts.Select(x =>
            {
                var keyValue = x.Split(new char[] { '=' });
                return new KeyValuePair<string, string>(
                    HttpUtility.UrlDecode(keyValue.First()),
                    HttpUtility.UrlDecode(keyValue.Last())
                );
            })
                .ToArray();

            kvps = kvps.Where(x => !x.Key.StartsWith("utm_")).ToArray();
            kvps = kvps.Where(x => !x.Key.StartsWith("_bhlid")).ToArray();
            kvps = kvps.Where(x => !x.Key.StartsWith("t")).ToArray();

            uri.Query = string.Join("&", kvps.Select(x => $"{HttpUtility.UrlEncode(x.Key)}={HttpUtility.UrlEncode(x.Value)}"));

            // TODO: Convert URLs on the same site to not include hostname
            //if (uri.Host == "travelcheck.beehiiv.com" || uri.Host == "travelcheck.dylanwolf.com") return uri.Path;

            return uri.Uri.ToString();
        }
    }
}

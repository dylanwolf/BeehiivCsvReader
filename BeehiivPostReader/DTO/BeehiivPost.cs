using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeehiivPostReader.DTO
{
    public class BeehiivPost
    {
        public string id { get; set; }
        public string web_title { get; set; }
        public string status { get; set; }
        public string web_audiences { get; set; }
        public string content_tags { get; set; }
        public string url { get; set; }
        public string web_subtitle { get; set; }
        public string email_subject_line { get; set; }
        public string email_preview_text { get; set; }
        public string content_html { get; set; }
        public string thumbnail_url { get; set; }
        public DateTime created_at { get; set; }

        /// <summary>
        /// Returns the URL that will be used for a scheduled/unpublished post, parsing it out of the HTML.
        /// </summary>
        public string ExpectedUrl
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(url)) return url;

                try
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(content_html);

                    return doc
                        .DocumentNode
                        .SelectNodes("//td[@class=\"f\"]/p/a")
                        .First()
                        .Attributes["href"].Value
                        .Split(new char[] { '?' }).First()
                        .Split(new char[] { '/' }).Last();
                }
                catch
                {
                    return url;
                }
            }
        }

    }
}

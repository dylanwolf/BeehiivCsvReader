using BeehiivPostReader.DTO;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BeehiivPostReader
{
    public class BeehiivHtmlConverter
    {
        string _imageUrlPath;

        public BeehiivHtmlConverter(string imageUrlPath)
        {
            _imageUrlPath = imageUrlPath;
            
        }

        public BeehiivPostHtml Parse(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var ctx = new BeehiivHtmlConverterContext();

            ParseBlockSeries(
                doc.DocumentNode.SelectNodes("//td[@class=\"email-card-body\"]/table/tr/td"),
                ctx);

            return new BeehiivPostHtml()
            {
                Document = ctx.RootElement,
                ImageDownloadUrls = ctx.DownloadUrls
            };
        }

        private void ParseBlockSeries(HtmlNodeCollection blocks, BeehiivHtmlConverterContext ctx)
        {
            if (blocks != null)
            {
                foreach (var block in blocks)
                {
                    ParseBlock(block, ctx);
                }
            }
        }

        private void ParseBlock(HtmlNode block, BeehiivHtmlConverterContext ctx)
        {
            switch (block.Name)
            {
                case "table":
                case "tr":
                case "td":
                case "div":
                    if (block.Name == "td" && block.HasClass("i2"))
                    {
                        ConvertBlockquote(block, ctx);
                    }
                    else if (block.Name == "table" && block.HasClass("j"))
                    {
                        ConvertDivider(block, ctx);
                    }
                    else
                    {
                        ParseBlockSeries(block.ChildNodes, ctx);
                    }
                    break;

                case "h1":
                case "h2":
                case "h3":
                case "h4":
                    ConvertHeading(block, ctx);
                    break;

                case "p":
                    ConvertParagraph(block, ctx);
                    break;

                case "img":
                    ConvertImage(block, ctx);
                    break;

                case "ul":
                    ConvertUnorderedList(block, ctx);
                    break;

                case "li":
                    ConvertListItem(block, ctx);
                    break;

                default:
                    ctx.Current.Peek().AddChild(new BeehiivPostHtmlElement()
                    {
                        Type = BeehiivElementType.RawHtml,
                        Html = BeehiivHtmlHelpers.ParseText(block.OuterHtml)
                    });
                    break;
            }
        }

        private void ConvertDivider(HtmlNode block, BeehiivHtmlConverterContext ctx)
        {
            ctx.Current.Peek().AddChild(new BeehiivPostHtmlElement()
            {
                Type = BeehiivElementType.Divider
            });
        }

        private void ConvertImage(HtmlNode img, BeehiivHtmlConverterContext ctx)
        {
            img.Attributes.Remove("style");
            var imgUrl = img.GetAttributeValue("src", null);

            if (imgUrl == null) return;

            ctx.DownloadUrls.Add(imgUrl);
            var staticUrl = BeehiivHtmlHelpers.CleanUrl(imgUrl).Split(new char[] { '/' }).Last();
            img.SetAttributeValue("src", $"/static/{staticUrl}");
            
            if (img.Attributes["alt"] == null) img.Attributes.Add("alt", staticUrl);
            if (string.IsNullOrWhiteSpace(img.Attributes["alt"].Value)) img.Attributes["alt"].Value = staticUrl;

            img.Attributes.Remove("border");

            string? width = null;
            if (img.Attributes["width"] != null)
            {
                width = img.Attributes["width"].Value;
                img.Attributes.Remove("width");

                if (width != "auto") width = $"{width}px";
                else width = null;
            }

            string? height = null;
            if (img.Attributes["height"] != null)
            {
                height = img.Attributes["height"].Value;
                img.Attributes.Remove("height");

                if (height != "auto") height = $"{height}px";
                else height = null;
            }

            ctx.Current.Peek().AddChild(new BeehiivPostImage()
            {
                Type = BeehiivElementType.Image,
                Html = img.OuterHtml,
                Width = width,
                Height = height
            });
        }

        private void ConvertParagraph(HtmlNode block, BeehiivHtmlConverterContext ctx)
        {
            ctx.Current.Peek().AddChild(new BeehiivPostHtmlElement()
            {
                Type = BeehiivElementType.Paragraph,
                Html = block.InnerHtml.Trim().ParseText().Replace("<br>", "")
            });
        }

        private void ConvertBlockquote(HtmlNode block, BeehiivHtmlConverterContext ctx)
        {
            var blockquote = new BeehiivPostParentElement()
            {
                Type = BeehiivElementType.Blockquote
            };
            ctx.Current.Peek().AddChild(blockquote);
            ctx.Current.Push(blockquote);
            ParseBlockSeries(block.ChildNodes, ctx);
            ctx.Current.Pop();
        }

        private void ConvertHeading(HtmlNode block, BeehiivHtmlConverterContext ctx)
        {
            var heading = new BeehiivPostHeading()
            {
                Type = BeehiivElementType.Heading,
                HeadingType = block.Name
            };
            ctx.Current.Peek().AddChild(heading);
            ctx.Current.Push(heading);
            ParseBlockSeries(block.ChildNodes, ctx);
            ctx.Current.Pop();
        }

        private void ConvertUnorderedList(HtmlNode block, BeehiivHtmlConverterContext ctx)
        {
            var list = new BeehiivPostParentElement()
            {
                Type = BeehiivElementType.UnorderedList
            };
            ctx.Current.Peek().AddChild(list);
            ctx.Current.Push(list);
            ParseBlockSeries(block.ChildNodes, ctx);
            ctx.Current.Pop();
        }

        private void ConvertListItem(HtmlNode block, BeehiivHtmlConverterContext ctx)
        {
            var childNodes = (block.ChildNodes.Count == 1 && block.FirstChild.Name == "p") ? block.FirstChild.ChildNodes : block.ChildNodes;

            var li = new BeehiivPostHeading()
            {
                Type = BeehiivElementType.ListItem,
                HeadingType = block.Name
            };
            ctx.Current.Peek().AddChild(li);
            ctx.Current.Push(li);
            ParseBlockSeries(childNodes, ctx);
            ctx.Current.Pop();
        }

        internal class BeehiivHtmlConverterContext
        {
            public BeehiivHtmlConverterContext()
            {
                RootElement = new BeehiivPostParentElement() {  Type = BeehiivElementType.Root };
                Current.Push(RootElement);
            }

            public BeehiivPostParentElement RootElement;
            public Stack<BeehiivPostParentElement> Current = new Stack<BeehiivPostParentElement>();
            public List<string> DownloadUrls = new List<string>();
        }


    }
}

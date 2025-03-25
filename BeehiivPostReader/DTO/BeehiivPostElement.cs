using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeehiivPostReader.DTO
{
    public abstract class BeehiivPostElementBase
    {
        public BeehiivElementType Type { get; internal set; }
    }

    public class BeehiivPostHtmlElement : BeehiivPostElementBase
    {
        public string Html { get; internal set; }
    }

    public class BeehiivPostParentElement : BeehiivPostElementBase
    {
        private List<BeehiivPostElementBase> _children = new List<BeehiivPostElementBase>();
        public IEnumerable<BeehiivPostElementBase> Children
        {
            get { return _children; }
        }

        public void AddChild(BeehiivPostElementBase elm)
        {
            _children.Add(elm);
        }
    }

    public class BeehiivPostHeading : BeehiivPostParentElement
    {
        public string HeadingType { get; internal set; }
    }

    public class BeehiivPostImage : BeehiivPostHtmlElement
    {
        public string Width { get; internal set; }
        public string Height { get; internal set; }
    }

    public enum BeehiivElementType
    {
        Root,
        RawHtml,
        Paragraph,
        Blockquote,
        Heading,
        Image,
        UnorderedList,
        ListItem,
        Divider
    }
}

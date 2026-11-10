using Microsoft.AspNetCore.Razor.TagHelpers;
using TauWeb.Web.Services;

namespace TauWeb.Web.Rendering;

[HtmlTargetElement("a")]
public sealed class EditionLinkTagHelper(TauEditionContext edition) : TagHelper
{
    public override int Order => 1000;
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (output.Attributes["href"]?.Value is { } href)
            output.Attributes.SetAttribute("href", edition.Url(System.Net.WebUtility.HtmlDecode(href.ToString()!)));
    }
}

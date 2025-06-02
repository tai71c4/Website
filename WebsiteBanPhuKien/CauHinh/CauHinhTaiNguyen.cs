using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace WebsiteBanPhuKien.CauHinh
{
    public static class CauHinhTaiNguyen
    {
        public static void DangKyTaiNguyen(IServiceCollection services)
        {
            services.AddSingleton<HtmlEncoder>(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs }));
            services.AddSingleton<UrlHelperFactory>();
            
            // Custom tag helper components
            services.AddTransient<ITagHelperComponent, CustomScriptTagHelperComponent>();
            services.AddTransient<ITagHelperComponent, CustomLinkTagHelperComponent>();
        }
    }

    public class CustomScriptTagHelperComponent : TagHelperComponent
    {
        public override int Order => 1;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.Equals(context.TagName, "body", StringComparison.OrdinalIgnoreCase))
            {
                output.PostContent.AppendHtml("<script src='/js/site.js'></script>");
            }
        }
    }

    public class CustomLinkTagHelperComponent : TagHelperComponent
    {
        public override int Order => 1;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.Equals(context.TagName, "head", StringComparison.OrdinalIgnoreCase))
            {
                output.PostContent.AppendHtml("<link href='/css/site.css' rel='stylesheet' />");
            }
        }
    }
}
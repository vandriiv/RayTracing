using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RayTracing.Web.Helpers.TagHelpers
{
    [HtmlTargetElement("*", Attributes = "is-active-page")]
    public class ActivePageTagHelper : TagHelper
    {
        [HtmlAttributeName("is-active-page")]
        public string Page { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (ShouldBeActive())
            {
                MakeActive(output);
            }

            output.Attributes.RemoveAll("is-active-page");
        }

        private bool ShouldBeActive()
        {
            string currentPage = ViewContext.RouteData.Values["Page"].ToString();

            if (!string.IsNullOrWhiteSpace(Page) && currentPage.ToLower().Contains(Page.ToLower()))
            {
                return true;
            }

            return false;
        }

        private void MakeActive(TagHelperOutput output)
        {
            var classAttr = output.Attributes.FirstOrDefault(a => a.Name == "class");
            if (classAttr == null)
            {
                classAttr = new TagHelperAttribute("class", "menu-item-active");
                output.Attributes.Add(classAttr);
            }
            else if (classAttr.Value == null || classAttr.Value.ToString().IndexOf("menu-item-active") < 0)
            {
                output.Attributes.SetAttribute("class", classAttr.Value == null ? "menu-item-active" : classAttr.Value.ToString() + "  menu-item-active");
            }
        }
    }
}

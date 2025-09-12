using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace IdentityApp.TagHelpers;

[HtmlTargetElement("td", Attributes = "asp-role-users")]
public class RoleUsersTagHelper : TagHelper
{
    [HtmlAttributeName("asp-role-users")]
    public string RoleName { get; set; } = null!;

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var byName = ViewContext.ViewData["RoleUsersByName"] as Dictionary<string, List<string>>;
        var byId = ViewContext.ViewData["RoleUsersById"] as Dictionary<string, List<string>>;

        if (string.IsNullOrWhiteSpace(RoleName))
        {
            output.Content.SetContent("No users");
            return;
        }

        if (TryGetUsers(byName, byId, RoleName, out var users) && users.Count > 0)
        {
            output.Content.SetHtmlContent(BuildHtml(users));
        }
        else
        {
            output.Content.SetContent("No users");
        }
    }

    private static bool TryGetUsers(Dictionary<string, List<string>>? byName,
                                    Dictionary<string, List<string>>? byId,
                                    string key,
                                    out List<string> users)
    {
        users = new List<string>();
        if (byName != null && byName.TryGetValue(key, out var u1))
        {
            users = u1;
            return true;
        }
        if (byId != null && byId.TryGetValue(key, out var u2))
        {
            users = u2;
            return true;
        }
        return false;
    }

    private static string BuildHtml(List<string> userNames)
    {
        var sb = new StringBuilder();
        sb.Append("<ul>");
        foreach (var name in userNames)
            sb.Append("<li>").Append(System.Net.WebUtility.HtmlEncode(name)).Append("</li>");
        sb.Append("</ul>");
        return sb.ToString();
    }
}
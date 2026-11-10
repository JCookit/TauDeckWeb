using Microsoft.AspNetCore.Mvc.RazorPages;
using TauWeb.Web.Services;

namespace TauWeb.Web.Pages.Articles;

public sealed class IndexModel(TauArticleStore store) : PageModel
{
    public IReadOnlyList<TauArticle> Articles { get; private set; } = [];
    public void OnGet() => Articles = store.GetAll();
}

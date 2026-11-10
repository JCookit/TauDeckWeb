using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TauWeb.Web.Services;

namespace TauWeb.Web.Pages.Articles;

public sealed class DetailModel(TauArticleStore store) : PageModel
{
    public TauArticle Article { get; private set; } = null!;
    public TauArticle? Previous { get; private set; }
    public TauArticle? Next { get; private set; }
    public int Position { get; private set; }
    public int Total { get; private set; }
    public string Html { get; private set; } = "";

    public IActionResult OnGet(string slug)
    {
        var articles = store.GetAll().Where(article => article.IsLive).ToList();
        var index = articles.FindIndex(article => article.Slug == slug);
        if (index < 0) return NotFound();
        Article = articles[index];
        Previous = index > 0 ? articles[index - 1] : null;
        Next = index + 1 < articles.Count ? articles[index + 1] : null;
        Position = index + 1;
        Total = articles.Count;
        Html = TauArticleStore.Render(Article.Markdown);
        return Page();
    }

    public IActionResult OnGetSource(string slug)
    {
        var article = store.GetAll().SingleOrDefault(article => article.IsLive && article.Slug == slug);
        return article is null ? NotFound() : File(Encoding.UTF8.GetBytes(article.Source), "text/markdown; charset=utf-8", slug + ".md");
    }
}

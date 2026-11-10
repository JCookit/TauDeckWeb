using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TauWeb.Core.Models;
using TauWeb.Web.Services;

namespace TauWeb.Web.Pages.Cards;

public sealed class DetailModel(TauContentStore store) : PageModel
{
    public TauCard Card { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var data = await store.GetAsync();
        var card = data.Cards.FirstOrDefault(item => item.Slug == slug);
        if (card is null)
        {
            return NotFound();
        }

        Card = card;
        return Page();
    }
}

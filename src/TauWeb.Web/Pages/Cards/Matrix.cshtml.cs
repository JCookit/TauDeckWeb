using Microsoft.AspNetCore.Mvc.RazorPages;
using TauWeb.Core.Models;
using TauWeb.Web.Services;

namespace TauWeb.Web.Pages.Cards;

public sealed class MatrixModel(TauContentStore store) : PageModel
{
    public TauSiteData Data { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Data = await store.GetAsync();
    }
}

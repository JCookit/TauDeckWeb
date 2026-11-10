using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TauWeb.Core.Models;
using TauWeb.Web.Services;

namespace TauWeb.Web.Pages.Systems;

public sealed class DetailModel(TauContentStore store) : PageModel
{
    public TauSystem System { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(string key)
    {
        var data = await store.GetAsync();
        var system = data.Systems.FirstOrDefault(item => item.Key == key);
        if (system is null)
        {
            return NotFound();
        }

        System = system;
        return Page();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventarioOperaciones.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Historial");
}
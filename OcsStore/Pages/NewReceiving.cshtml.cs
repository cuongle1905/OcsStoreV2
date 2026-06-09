using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class NewReceivingModel : PageModel
    {

        [FromQuery(Name = "sourceurl")]
        public string SourceUrl { get; set; }


        [FromQuery(Name = "item")]
        public short Item { get; set; } = 0;

        public void OnGet()
        {
        }
    }
}

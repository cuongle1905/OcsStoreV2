using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class NewReceivingModel : SourceUrlPageModel
    {

        [FromQuery(Name = "item")]
        public short Item { get; set; } = 0;

        public void OnGet()
        {
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OcsStore
{
    public class SourceUrlPageModel: PageModel
    {
        [FromQuery(Name = "sourceurl")]
        public string SourceUrl { get; set; }
    }
}

using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class RawProcessingModel : PageModel
    {
        private readonly ProcessingController _processingController;

        public RawProcessingModel(ProcessingController controller)
        {
            _processingController = controller;
        }

        public void OnGet()
        {
        }
    }
}

using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class ProcessingRawModel : PageModel
    {
        private readonly ProcessingController _processingController;

        public ProcessingRawModel(ProcessingController controller)
        {
            _processingController = controller;
        }

        public void OnGet()
        {
        }
    }
}

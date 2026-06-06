using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class ProcessingsModel : PageModel
    {
        [FromQuery(Name = "type")]
        public int Type { get; set; }
        public string TypeName;

        private readonly ProcessingController _processingController;

        public ProcessingsModel(ProcessingController controller)
        {
            _processingController = controller;
        }

        public void OnGet()
        {
            if (Type == 0)
            {
                Type = 1;
            }
            TypeName = _processingController.GetProcessingType(Type).Name;
        }
    }
}

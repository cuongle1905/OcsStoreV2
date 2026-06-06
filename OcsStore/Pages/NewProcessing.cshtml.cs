using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;
using System.Diagnostics;

namespace OcsStore.Pages
{
    public class NewProcessingModel : PageModel
    {
        private readonly ProcessingController _processingController;
        
        [FromQuery(Name = "type")]
        public int Type { get; set; }
        public string TypeName;

        public ProcessingModel[] Models = [];

        public List<ProcessingDetailView> Details = new List<ProcessingDetailView>();

        public NewProcessingModel(ProcessingController controller)
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
            Models = _processingController.GetModels(Type);
        }
    }
}

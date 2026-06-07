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
        public short Type { get; set; }
        public string TypeName;

        public List<ProcessingInputView> Inputs = new List<ProcessingInputView>();

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
            TypeName = _processingController.GetProcessingName(Type);
        }
    }
}

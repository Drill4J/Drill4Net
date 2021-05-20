using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Drill4Net.Demo.Razor.App.Pages
{
    public class CoveragePageModel : PageModel
    {
        [Parameter] public int PageNumber { get; set; }
        [Parameter] public int TotalRecords { get; set; }
        [Parameter] public int PageSize { get; set; } = 20;
        [Parameter] public string LinkUrl { get; set; }
        public int TotalPages => (int)Math.Ceiling((decimal)TotalRecords / PageSize);

        private readonly ILogger<IndexModel> _logger;

        public CoveragePageModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}

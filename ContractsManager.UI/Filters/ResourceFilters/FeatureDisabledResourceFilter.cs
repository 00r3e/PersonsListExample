using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PersonsListExample.Filters.ResultFilters;

namespace PersonsListExample.Filters.ResourceFilters
{
    public class FeatureDisabledResourceFilter : IAsyncResourceFilter
    {
        private readonly ILogger<FeatureDisabledResourceFilter> _logger;
        private readonly bool _isDisabled;

        public FeatureDisabledResourceFilter(ILogger<FeatureDisabledResourceFilter> logger, bool isDisabled = true)
        {
            _logger = logger;
            _isDisabled = isDisabled;   
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            _logger.LogInformation("{FilterName}.{MethodName} - before", nameof(FeatureDisabledResourceFilter),
                nameof(OnResourceExecutionAsync));

            if (_isDisabled){ 
                
                //context.Result = new NotFoundResult(); // 404 - Not Found

                context.Result = new StatusCodeResult(501); // 501 - Not Implemented
            }
            else
            {
                await next();
            }

            _logger.LogInformation("{FilterName}.{MethodName} - after", nameof(FeatureDisabledResourceFilter),
                nameof(OnResourceExecutionAsync));

        }
    }
}

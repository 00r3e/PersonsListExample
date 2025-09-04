using Microsoft.AspNetCore.Mvc.Filters;

namespace PersonsListExample.Filters.ResultFilters
{
    public class PersonAlwaysRunResultFilter : IAlwaysRunResultFilter
    {
        public readonly ILogger<PersonAlwaysRunResultFilter> _logger;

        public PersonAlwaysRunResultFilter(ILogger<PersonAlwaysRunResultFilter> logger)
        {
            _logger = logger;
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            if (context.Filters.OfType<SkipFilter>().Any())
            {
                return;
            }
            //after logic here
            _logger.LogInformation("{FilterName}.{MethodName} - after", nameof(PersonAlwaysRunResultFilter),
                nameof(OnResultExecuted));
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Filters.OfType<SkipFilter>().Any()) 
            {
                return;
            }
            //before logic here
            _logger.LogInformation("{FilterName}.{MethodName} - before", nameof(PersonAlwaysRunResultFilter),
                nameof(OnResultExecuting));

        }
    }
}

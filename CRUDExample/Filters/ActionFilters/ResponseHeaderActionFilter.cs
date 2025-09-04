using Microsoft.AspNetCore.Mvc.Filters;


namespace PersonsListExample.Filters.ActionFilters
{
    public class ResponseHeaderFilterFactoryAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;
        public readonly string Key;
        public readonly string Value;
        public readonly int Ordrer;


        public ResponseHeaderFilterFactoryAttribute(string key, string value, int order)
        {
            Key = key;
            Value = value;
            Ordrer = order;
        }

        //Controller -> FilterFactory -> Filter
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            ResponseHeaderActionFilter filter = serviceProvider.GetRequiredService<ResponseHeaderActionFilter>();
            filter.Key = Key;
            filter.Value = Value;
            filter.Order = Ordrer;
            //return filter object
            return filter;
        }
    }

    public class ResponseHeaderActionFilter : IAsyncActionFilter, IOrderedFilter
    {

        public string Key { get; set; }
        public string Value { get; set; }
        public int Order { get; set; }
        public readonly ILogger<ResponseHeaderActionFilter> _logger;


        public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger)
        {
            _logger = logger;
        }

        public  async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //on action executing
            _logger.LogInformation("{FilterName}.{MethodName} method - before", nameof(ResponseHeaderActionFilter)
                , nameof(OnActionExecutionAsync));

            await next();  //calls the subsequent filter or action method

            //on action executed
            _logger.LogInformation("{FilterName}.{MethodName} method - after", nameof(ResponseHeaderActionFilter)
                , nameof(OnActionExecutionAsync));
            context.HttpContext.Response.Headers[Value] = Key;
        }
    }
}

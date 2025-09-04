using System.Reflection;
using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using ServiceContracts.DTO;

namespace PersonsListExample.Filters.ActionFilters
{
    public class PersonsListActionFilter : IActionFilter
    {
        private readonly ILogger<PersonsListActionFilter> _logger;

        public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger)
        {
            _logger = logger;
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("{FilterName}.{MethodName} method", nameof(PersonsListActionFilter)
                ,nameof(OnActionExecuted));

            PersonsController personsController = (PersonsController)context.Controller;

            IDictionary<string, object?>? parameters = (IDictionary<string, object?>?)context.HttpContext.Items["arguments"];

            if (parameters != null) 
            {
                if (parameters.ContainsKey("CurrentSearchBy"))
                {
                    personsController.ViewData["CurrentSearchBy"] = Convert.ToString(parameters["CurrentSearchBy"]);
                };
            };

            if (parameters != null)
            {
                if (parameters.ContainsKey("CurrentSearchString"))
                {
                    personsController.ViewData["CurrentSearchString"] = Convert.ToString(parameters["CurrentSearchString"]);
                };
            };

            if (parameters != null)
            {
                if (parameters.ContainsKey("CurrentSortBy"))
                {
                    personsController.ViewData["CurrentSortBy"] = Convert.ToString(parameters["CurrentSortBy"]);
                };
            };

            if (parameters != null)
            {
                if (parameters.ContainsKey("CurrentSortOrder"))
                {
                    personsController.ViewData["CurrentSortOrder"] = Convert.ToString(parameters["CurrentSortOrder"]);
                };
            };

            personsController.ViewBag.SearchFields = new Dictionary<string, string>()
            {
                {nameof(PersonResponse.PersonName), "Person Name" },
                {nameof(PersonResponse.Email), "Email" },
                {nameof(PersonResponse.DateOfBirth), "Date Of Birth" },
                {nameof(PersonResponse.Gender), "Gender" },
                {nameof(PersonResponse.CountryID), "Country" },
                {nameof(PersonResponse.Address), "Address" },
            };
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Items["arguments"] = context.ActionArguments;

            _logger.LogInformation("{FilterName}.{MethodName} method", nameof(PersonsListActionFilter), 
                nameof(OnActionExecuting));

            //reset searchBy parameter value
            if (context.ActionArguments.ContainsKey("searchBy"))
            {
                string? searchBy = Convert.ToString(context.ActionArguments["searchBy"]);
                
                //validate searchBy parameter value
                if (!string.IsNullOrEmpty(searchBy))
                {
                    var searchOptions = new List<string>()
                    {
                        nameof(PersonResponse.PersonName),
                        nameof(PersonResponse.Email),
                        nameof(PersonResponse.DateOfBirth),
                        nameof(PersonResponse.Gender),
                        nameof(PersonResponse.CountryID),
                        nameof(PersonResponse.Address)
                    };

                    if (searchOptions.Any(temp => temp == searchBy) == false)
                    {
                        _logger.LogInformation("search by actual value: {searchBy}", searchBy);

                        context.ActionArguments["searchby"] = nameof(PersonResponse.PersonName);

                        _logger.LogInformation("search by updated value: {searchBy}", context.ActionArguments["searchby"]);

                    };
                }

            }
        }
    }
}

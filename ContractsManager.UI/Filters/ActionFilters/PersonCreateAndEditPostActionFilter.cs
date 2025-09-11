using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;

namespace PersonsListExample.Filters.ActionFilters
{
    public class PersonCreateAndEditPostActionFilter : IAsyncActionFilter
    {
        private readonly ICountriesGetterService _countriesService;
        public PersonCreateAndEditPostActionFilter(ICountriesGetterService countriesService)
        {
            _countriesService = countriesService;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if(context.Controller is PersonsController personsController)
            {
                if (!personsController.ModelState.IsValid)
                {
                    List<CountryResponse> countries = await _countriesService.GetAllCountries();
                    personsController.ViewBag.Countries = countries;
                    personsController.ViewBag.Countries = countries.Select(c => new SelectListItem() { Text = c.CountryName, Value = c.CountryID.ToString() });


                    personsController.ViewBag.Errors = personsController.ModelState.Values
                            .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                            .ToList();

                    var personAddRequest = context.ActionArguments["personRequest"];

                    context.Result = personsController.View(personAddRequest); //short-circuits or skips the subsequent action filters
                }
                else
                {
                    await next();
                }
            }
            else
            {
                await next();
            }
        }
    }
}

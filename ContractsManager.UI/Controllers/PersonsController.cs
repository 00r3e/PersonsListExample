using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PersonsListExample.Filters;
using PersonsListExample.Filters.ActionFilters;
using PersonsListExample.Filters.AuthorizationFilter;
using PersonsListExample.Filters.ExceptionFilters;
using PersonsListExample.Filters.ResourceFilters;
using PersonsListExample.Filters.ResultFilters;
using Rotativa.AspNetCore;
using Serilog;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    [ResponseHeaderFilterFactory("X-Controller-Custom-Key", "Controller-Custom-Value", 3)]
    [TypeFilter(typeof(HandleExceptionFilter))]
    [TypeFilter(typeof(PersonAlwaysRunResultFilter))]
    public class PersonsController : Controller
    {
        //private fields
        private readonly ICountriesGetterService _countriesService;
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly IPersonsSorterService _personsSorterService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly ILogger<PersonsController> _logger;

        public PersonsController(ICountriesGetterService countriesService, IPersonsGetterService personsGetterService,
            IPersonsAdderService personsAdderService, IPersonsDeleterService personsDeleterService,
            IPersonsSorterService personsSorterService, IPersonsUpdaterService personsUpdaterService, 
            ILogger<PersonsController> logger)
        {
            _countriesService = countriesService;
            _personsGetterService = personsGetterService;
            _personsAdderService = personsAdderService;
            _personsSorterService = personsSorterService;
            _personsDeleterService = personsDeleterService;
            _personsUpdaterService = personsUpdaterService;

            _logger = logger;
        }


        [Route("[action]")]
        [Route("/")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [TypeFilter(typeof(PersonsListActionFilter), Order = 4)]
        [TypeFilter(typeof(PersonsListResultFilter))]
        [SkipFilter]
        [ResponseHeaderFilterFactory("X-Action-Custom-Key", "Action-Custom-Value", 1)]

        public async Task<IActionResult> Index(string searchBy, string? searchString, 
                                    string sortBy = nameof(PersonResponse.PersonName),
                                    SortOrderOptions sortOrder = SortOrderOptions.ASCENDING)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(Index), nameof(PersonsController));

            _logger.LogDebug($"searchBy : {searchBy}, searchString : {searchString}, sortOrder : {sortOrder}");
            

            //Filtered Persons
            List<PersonResponse> persons = await _personsGetterService.GetFilteredPersons(searchBy, searchString);

            //Sorting Persons
            persons = await _personsSorterService.GetSortedPersons(persons, sortBy, sortOrder);

            return View(persons);
        }


        [Route("[action]")]
        [HttpGet]
        [ResponseHeaderFilterFactory("X-Action-Custom-Key", "Action-Custom-Value", 4)]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("{MethodName} action method of {ControllerName}", nameof(Create), nameof(PersonsController));

            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(c => new SelectListItem { Text = c.CountryName, Value = c.CountryID.ToString() });

            return View();
        }

        [Route("[action]")]
        [HttpPost]
        [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
        [TypeFilter(typeof(FeatureDisabledResourceFilter), Arguments = new object[] { false })]
        public async Task<IActionResult> Create(PersonAddRequest personRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}",  nameof(Create), nameof(PersonsController));

            //call the service method
            PersonResponse personResponse = await _personsAdderService.AddPerson(personRequest);

            //navigate to Index action method (it makes another get request to "persons/index")
            return RedirectToAction("Index","Persons");
        }

        [Route("[action]/{personID}")]
        [HttpGet]
        [TypeFilter(typeof(TokenResultFilter))]

        public async Task<IActionResult> Edit(Guid personID)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(Edit), nameof(PersonsController));

            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }
            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries;
            ViewBag.Countries = countries.Select(c => new SelectListItem { Text = c.CountryName, Value = c.CountryID.ToString() });

            return View(personUpdateRequest);  
        }

        [Route("[action]/{personID}")]
        [HttpPost]
        [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
        [TypeFilter(typeof(TokenAuthorizationFilter))]

        public async Task<IActionResult> Edit(PersonUpdateRequest personRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(Edit), nameof(PersonsController));

            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personRequest.PersonID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            PersonResponse? personResponseFromUpdate = await _personsUpdaterService.UpdatePerson(personRequest);

            if(personResponse == null)
            {
                return View(personRequest);
            }

            return RedirectToAction("Index");

        }

        [HttpGet]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(Guid? personID) 
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(Delete), nameof(PersonsController));

            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);

            if(personResponse == null) { return RedirectToAction("Index"); }

            return View(personResponse);
        }

        [HttpPost]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(PersonResponse? personResponse) 
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}",  nameof(Delete), nameof(PersonsController));

            PersonResponse? personResponseFromGet = await _personsGetterService.GetPersonByPersonID(personResponse?.PersonID);

            if (personResponseFromGet == null) { return RedirectToAction("Index"); }

            bool isDeleted = await _personsDeleterService.DeletePerson(personResponse?.PersonID);

            if (isDeleted) { return RedirectToAction("Index"); }
            
            return View(personResponse);
        }
        [Route("PeronsPDF")]
        public async Task<IActionResult> PersonsPDF()
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(PersonsPDF), nameof(PersonsController));

            //Get list of persons
            List<PersonResponse> persons = await _personsGetterService.GetAllPersons();

            //Return view as pdf
            return new ViewAsPdf("PersonsPDF", persons, ViewData)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins() { Top = 20, Bottom = 20, Left = 20, Right = 20 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }

        [Route("PersonsCSV")]
        public async Task<IActionResult> PersonsCSV()
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(PersonsCSV), nameof(PersonsController));

            MemoryStream memoryStream = await _personsGetterService.GetPersonsCSV();

            return File(memoryStream, "application/octet-stream", "persons.csv");
        }

        [Route("PersonsExcel")]
        public async Task<IActionResult> PersonsExcel()
        {
            _logger.LogInformation("{Methodname} action method of {ControllerName}", nameof(PersonsExcel), nameof(PersonsController));

            MemoryStream memoryStream = await _personsGetterService.GetPersonsExcel();

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "persons.xlsx");
        }

    }
}

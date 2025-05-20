using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    public class PersonsController : Controller
    {
        //private fields
        private readonly ICountriesService _countryService;
        private readonly IPersonsService _personsService;

        public PersonsController(ICountriesService countriesService, IPersonsService personsService)
        {
            _countryService = countriesService;
            _personsService = personsService;
        }


        [Route("[action]")]
        [Route("/")]
        public IActionResult Index(string searchBy, string? searchString, 
                                    string sortBy = nameof(PersonResponse.PersonName),
                                    SortOrderOptions sortOrder = SortOrderOptions.ASCENDING)
        {
            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                {nameof(PersonResponse.PersonName), "Person Name" },
                {nameof(PersonResponse.Email), "Email" },
                {nameof(PersonResponse.DateOfBirth), "Date Of Birth" },
                {nameof(PersonResponse.Gender), "Gender" },
                {nameof(PersonResponse.CountryID), "Country" },
                {nameof(PersonResponse.Address), "Address" },
            };

            //Filtered Persons
            List<PersonResponse> persons = _personsService.GetFilteredPersons(searchBy, searchString);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            //Sorting Persons
            persons = _personsService.GetSortedPersons(persons, sortBy, sortOrder);
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortorder = sortOrder;

            return View(persons);
        }


        //Executes when the user clicks on "Create Person" hyperlink
        //(While opening the create view)
        [Route("[action]")]
        [HttpGet]
        public IActionResult Create()
        {
            List<CountryResponse> countries = _countryService.GetAllCountries();
            ViewBag.Countries = countries.Select(c => new SelectListItem { Text = c.CountryName, Value = c.CountryID.ToString() });

            return View();
        }

        [Route("[action]")]
        [HttpPost]
        public IActionResult Create(PersonAddRequest personAddRequest)
        {
            if (!ModelState.IsValid) 
            {
                List<CountryResponse> countries = _countryService.GetAllCountries();
                ViewBag.Countries = countries;
                ViewBag.Countries = countries.Select(c => new SelectListItem { Text = c.CountryName, Value = c.CountryID.ToString() });


                ViewBag.Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                return View();
            }

            //call the service method
            PersonResponse personResponse = _personsService.AddPerson(personAddRequest);

            //navigate to Index action method (it makes another get request tp "persons/index"
            return RedirectToAction("Index","Persons");
        }
    }
}

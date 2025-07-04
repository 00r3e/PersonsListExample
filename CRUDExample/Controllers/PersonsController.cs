﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using Serilog;
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
        private readonly ILogger<PersonsController> _logger;

        public PersonsController(ICountriesService countriesService, IPersonsService personsService, 
            ILogger<PersonsController> logger)
        {
            _countryService = countriesService;
            _personsService = personsService;
            _logger = logger;
        }


        [Route("[action]")]
        [Route("/")]
        public async Task<IActionResult> Index(string searchBy, string? searchString, 
                                    string sortBy = nameof(PersonResponse.PersonName),
                                    SortOrderOptions sortOrder = SortOrderOptions.ASCENDING)
        {
            _logger.LogInformation("Index action method of PersonsController");

            _logger.LogDebug($"searchBy : {searchBy}, searchString : {searchString}, sortOrder : {sortOrder}");
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
            List<PersonResponse> persons = await _personsService.GetFilteredPersons(searchBy, searchString);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            //Sorting Persons
            persons = await _personsService.GetSortedPersons(persons, sortBy, sortOrder);
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortorder = sortOrder;

            return View(persons);
        }


        //Executes when the user clicks on "Create Person" hyperlink
        //(While opening the create view)
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Create action method of PersonsController");

            List<CountryResponse> countries = await _countryService.GetAllCountries();
            ViewBag.Countries = countries.Select(c => new SelectListItem { Text = c.CountryName, Value = c.CountryID.ToString() });

            return View();
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Create(PersonAddRequest personAddRequest)
        {
            _logger.LogInformation("Create action method of PersonsController");

            if (!ModelState.IsValid) 
            {
                List<CountryResponse> countries = await _countryService.GetAllCountries();
                ViewBag.Countries = countries;
                ViewBag.Countries = countries.Select(c => new SelectListItem { Text = c.CountryName, Value = c.CountryID.ToString() });


                ViewBag.Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                return View(personAddRequest);
            }

            //call the service method
            PersonResponse personResponse = await _personsService.AddPerson(personAddRequest);

            //navigate to Index action method (it makes another get request to "persons/index")
            return RedirectToAction("Index","Persons");
        }

        [Route("[action]/{personID}")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid personID)
        {
            _logger.LogInformation("Edit action method of PersonsController");

            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }
            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            List<CountryResponse> countries = await _countryService.GetAllCountries();
            ViewBag.Countries = countries;
            ViewBag.Countries = countries.Select(c => new SelectListItem { Text = c.CountryName, Value = c.CountryID.ToString() });

            return View(personUpdateRequest);  
        }

        [Route("[action]/{personID}")]
        [HttpPost]
        public async Task<IActionResult> Edit(PersonUpdateRequest personUpdateRequest)
        {
            _logger.LogInformation("Edit action method of PersonsController");

            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personUpdateRequest.PersonID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                List<CountryResponse> countries = await _countryService.GetAllCountries();
                ViewBag.Countries = countries;
                ViewBag.Countries = countries.Select(c => new SelectListItem { Text = c.CountryName, Value = c.CountryID.ToString() });


                ViewBag.Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                return View(personResponse.ToPersonUpdateRequest());
            }

            PersonResponse? personResponseFromUpdate = await _personsService.UpdatePerson(personUpdateRequest);

            if(personResponse == null)
            {
                return View(personUpdateRequest);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(Guid? personID) 
        {
            _logger.LogInformation("Delete action method of PersonsController");

            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);

            if(personResponse == null) { return RedirectToAction("Index"); }

            return View(personResponse);
        }

        [HttpPost]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(PersonResponse? personResponse) 
        {
            _logger.LogInformation("Delete action method of PersonsController");

            PersonResponse? personResponseFromGet = await _personsService.GetPersonByPersonID(personResponse?.PersonID);

            if (personResponseFromGet == null) { return RedirectToAction("Index"); }

            bool isDeleted = await _personsService.DeletePerson(personResponse?.PersonID);

            if (isDeleted) { return RedirectToAction("Index"); }
            
            return View(personResponse);
        }
        [Route("PeronsPDF")]
        public async Task<IActionResult> PersonsPDF()
        {
            _logger.LogInformation("PersonsPDF action method of PersonsController");

            //Get list of persons
            List<PersonResponse> persons = await _personsService.GetAllPersons();

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
            _logger.LogInformation("PersonsCSV action method of PersonsController");

            MemoryStream memoryStream = await _personsService.GetPersonsCSV();

            return File(memoryStream, "application/octet-stream", "persons.csv");
        }

        [Route("PersonsExcel")]
        public async Task<IActionResult> PersonsExcel()
        {
            _logger.LogInformation("PersonsExcel action method of PersonsController");

            MemoryStream memoryStream = await _personsService.GetPersonsExcel();

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "persons.xlsx");
        }

    }
}

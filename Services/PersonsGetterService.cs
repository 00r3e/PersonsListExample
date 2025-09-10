using System;
using Entities;
using ServiceContracts.DTO;
using ServiceContracts;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Services.Helpers;
using System.Reflection;
using ServiceContracts.Enums;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using OfficeOpenXml;
using RepositoryContracts;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Serilog;
using SerilogTimings;
using Exceptions;

namespace Services
{
    public class PersonsGetterService : IPersonsGetterService
    {
        //private field
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsGetterService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;

        public PersonsGetterService(IPersonsRepository personsRepository, ILogger<PersonsGetterService> logger,
            IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            _logger.LogInformation("{MethodName} of {ServiceName}", nameof(GetAllPersons), nameof(PersonsGetterService));

            var persons = await _personsRepository.GetAllPersons();

            return persons.Select(p => p.ToPersonResponse()).ToList();
        }

        public async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
        {
            _logger.LogInformation("{MethodName} of {ServiceName}", nameof(GetPersonByPersonID), nameof(PersonsGetterService));

            if (!personID.HasValue)
            {
                return null;
            }

            Person? person = await _personsRepository.GetPersonById(personID.Value);

            if (person == null)
            {
                return null;
            }

            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            _logger.LogInformation("{MethodName} of {ServiceName}", nameof(GetFilteredPersons), nameof(PersonsGetterService));

            List<Person> persons;

            using (Operation.Time("Time for Filtered Persons from database"))
            {
                persons = searchBy switch
                {
                    nameof(PersonResponse.PersonName) =>
                     await _personsRepository.GetFilteredPersons(temp =>
                     temp.PersonName.Contains(searchString)),

                    nameof(PersonResponse.Email) =>
                     await _personsRepository.GetFilteredPersons(temp =>
                     temp.Email.Contains(searchString)),

                    nameof(PersonResponse.DateOfBirth) =>
                     await _personsRepository.GetFilteredPersons(temp =>
                     temp.DateOfBirth.Value.ToString().Contains(searchString)),


                    nameof(PersonResponse.Gender) =>
                     await _personsRepository.GetFilteredPersons(temp =>
                     temp.Gender.Contains(searchString)),

                    nameof(PersonResponse.CountryID) =>
                     await _personsRepository.GetFilteredPersons(temp =>
                     temp.Country.CountryName.Contains(searchString)),

                    nameof(PersonResponse.Address) =>
                    await _personsRepository.GetFilteredPersons(temp =>
                    temp.Address.Contains(searchString)),

                    _ => await _personsRepository.GetAllPersons()
                };
            }

            _diagnosticContext.Set("Persons", persons);

            return persons.Select(temp => temp.ToPersonResponse()).ToList();
        }

        public async Task<MemoryStream> GetPersonsCSV()
        {
            _logger.LogInformation("{MethodName} of {ServiceName}", nameof(GetPersonsCSV), nameof(PersonsGetterService));

            MemoryStream memoryStream = new MemoryStream();

            StreamWriter writer = new StreamWriter(memoryStream);

            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);

            CsvWriter csvWriter = new CsvWriter(writer, csvConfiguration);

            csvWriter.WriteField(nameof(PersonResponse.PersonName));
            csvWriter.WriteField(nameof(PersonResponse.Email));
            csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
            csvWriter.WriteField(nameof(PersonResponse.Age));
            csvWriter.WriteField(nameof(PersonResponse.Gender));
            csvWriter.WriteField(nameof(PersonResponse.Address));
            csvWriter.WriteField(nameof(PersonResponse.ReceiveNewsLetters));

            csvWriter.NextRecord();

            List<PersonResponse> responseList = await GetAllPersons();

            foreach (PersonResponse response in responseList)
            {
                csvWriter.WriteField(response.PersonName);
                csvWriter.WriteField(response.Email);
                csvWriter.WriteField(response.DateOfBirth?.ToString("dd-MM-yyyy"));
                csvWriter.WriteField(response.Age);
                csvWriter.WriteField(response.Gender);
                csvWriter.WriteField(response.Address);
                csvWriter.WriteField(response.ReceiveNewsLetters.ToString());

                csvWriter.NextRecord();
                csvWriter.Flush();
            }

            memoryStream.Position = 0;

            return memoryStream;
        }

        public async Task<MemoryStream> GetPersonsExcel()
        {
            _logger.LogInformation("{MethodName} of {ServiceName}", nameof(GetPersonsExcel), nameof(PersonsGetterService));

            MemoryStream memoryStream = new MemoryStream();

            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets.Add("PersonsSheet");

                excelWorksheet.Cells["A1"].Value = "Person Name";
                excelWorksheet.Cells["B1"].Value = "Email";
                excelWorksheet.Cells["C1"].Value = "Date of Birth";
                excelWorksheet.Cells["D1"].Value = "Age";
                excelWorksheet.Cells["E1"].Value = "Gender";
                excelWorksheet.Cells["F1"].Value = "Country";
                excelWorksheet.Cells["G1"].Value = "Address";
                excelWorksheet.Cells["H1"].Value = "Receive News Letters";

                using (ExcelRange headerCells = excelWorksheet.Cells["A1:H1"])
                {
                    headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;

                    headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    headerCells.Style.Font.Bold = true;
                }

                int row = 2;
                List<PersonResponse> personsList = await GetAllPersons();

                foreach (PersonResponse personResponse in personsList)
                {
                    excelWorksheet.Cells[row, 1].Value = personResponse.PersonName;
                    excelWorksheet.Cells[row, 2].Value = personResponse.Email;
                    if (personResponse.DateOfBirth.HasValue)
                    {
                        excelWorksheet.Cells[row, 2].Value = personResponse.DateOfBirth;
                    }
                    else { excelWorksheet.Cells[row, 3].Value = ""; }
                    excelWorksheet.Cells[row, 4].Value = personResponse.Age;
                    excelWorksheet.Cells[row, 5].Value = personResponse.Gender;
                    excelWorksheet.Cells[row, 6].Value = personResponse.Country;
                    excelWorksheet.Cells[row, 7].Value = personResponse.Address;
                    excelWorksheet.Cells[row, 8].Value = personResponse.ReceiveNewsLetters;
                    row++;
                }

                excelWorksheet.Cells[$"A1:H{row}"].AutoFitColumns();

                await excelPackage.SaveAsync();
            }

            memoryStream.Position = 0;
            return memoryStream;
        }

    }
}

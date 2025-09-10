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
    public class PersonsDeleterService : IPersonsDeleterService
    {
        //private field
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsDeleterService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;

        public PersonsDeleterService(IPersonsRepository personsRepository, ILogger<PersonsDeleterService> logger,
            IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public async Task<bool> DeletePerson(Guid? personID)
        {
            _logger.LogInformation("{MethodName} of {ServiceName}", nameof(DeletePerson), nameof(PersonsDeleterService));

            if (personID == null)
            {
                throw new ArgumentNullException(nameof(personID));
            }

            Person? person = await _personsRepository.GetPersonById(personID.Value);
            if (person == null) { return false; }

            using (Operation.Time("Time for Delete a Person from database"))
            {
                await _personsRepository.DeletePersonByPersonID(personID.Value);
            }
            return true;
        }

    }
}

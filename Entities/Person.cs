using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Person
    {
        [Key]
        public Guid PersonID { get; set; }
        [StringLength(40)]//nvarchar(40)
        public string? PersonName { get; set; }
        [StringLength(40)]
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [StringLength(40)]
        public string? Gender { get; set; }
        //uniqueidentifier
        public Guid? CountryID { get; set; }
        [StringLength(200)]
        public string? Address { get; set; }
        //bit
        public bool ReceiveNewsLetters { get; set; }

        public string? TIN { get; set; } = string.Empty;
        [ForeignKey(nameof(CountryID))]
        public virtual Country? Country { get; set; }

        public override string ToString()
        {
            return $"Person ID: {PersonID}, Person Name: {PersonName}, Email: {Email}, " +
                $"Date Of Birth: {DateOfBirth?.ToString("MM/dd/yyyy")}, Gender: {Gender}, " +
                $"Country ID: {CountryID}, Country: {Country?.CountryName}, Address: {Address}" +
                $"Receive News Letters : {ReceiveNewsLetters}";
        }
    }
}



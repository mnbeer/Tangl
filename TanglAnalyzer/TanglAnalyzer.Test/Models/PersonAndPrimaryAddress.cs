using System;
using System.Collections.Generic;

namespace Tangl.Tests.Domain
{
    public class PersonAndPrimaryAddress
    {
        [Tangl(target: "Person.PersonId")]
        public long PersonId { get; set; }
        [Tangl("Person.FirstName")]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Birthdate { get; set; }

        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Tangl.Tests.Domain
{
    public class Person
    {
        public int PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Birthdate { get; set; }

        public List<Address> Addresses { get; set; } = new List<Address>();
    }
}



namespace Tangl.Tests.Domain
{
    public class Address
    {
        public int AddressId { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public bool IsPrimary { get; set; }

        public string AddressType { get; set; }

        public int PersonId { get; set; }
    }
}

using EntityWorker.Core.Attributes;
using System;

namespace LightData.CMS.Modules.Library
{
    public class Address
    {
        [PrimaryKey]
        public Guid? Id { get; set; }

        [NotNullable]
        public string Name { get; set; }

        [ForeignKey(typeof(Country))]
        public Guid CountryId { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string TownOrCity { get; set; }

        public string PostalCode { get; set; }

        public string Area { get; set; }

        [IndependentData]
        public Country Country { get; set; }

        [ForeignKey(typeof(Person))]
        public Guid PersonId { get; set; }
    }
}

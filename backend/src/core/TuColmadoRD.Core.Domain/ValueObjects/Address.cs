using TuColmadoRD.Core.Domain.Base.Result;

namespace TuColmadoRD.Core.Domain.ValueObjects
{
    public record Address
    {
        public string Province { get; private init; }
        public string Sector { get; private init; }
        public string Street { get; private init; }
        public string Reference { get; private init; }
        public string? HouseNumber { get; private init; }
        public double? Latitude { get; private init; }
        public double? Longitude { get; private init; }

        private Address(string province, string sector, string street, string? houseNumber, string reference, double? latitude = null, double? longitude = null)
        {
            Province = province;
            Sector = sector;
            Street = street;
            HouseNumber = houseNumber;
            Reference = reference;
            Latitude = latitude;
            Longitude = longitude;
        }

        public static OperationResult<Address, string> Create(
            string province,
            string sector,
            string street,
            string reference,
            string? houseNumber = null,
            double? latitude = null,
            double? longitude = null)
        {
            if (string.IsNullOrWhiteSpace(province))
                return OperationResult<Address, string>.Bad("La Provincia Es Requerida.");

            if (string.IsNullOrWhiteSpace(sector))
                return OperationResult<Address, string>.Bad("El sector Es Requerido.");

            if (string.IsNullOrWhiteSpace(street))
                return OperationResult<Address, string>.Bad("La Calle Es Requerida.");

            if (string.IsNullOrWhiteSpace(reference))
                return OperationResult<Address, string>.Bad("Para un Colmado, la referencia (por ejemplo, 'Frente al parque') es vital.");

            return OperationResult<Address, string>.Good(
                new Address(province.Trim(), sector.Trim(), street.Trim(), houseNumber?.Trim(), reference.Trim(), latitude, longitude)
            );
        }
        public override string ToString()
            => $"{Street} #{HouseNumber ?? "S/N"}, {Sector}, {Province}. (Ref: {Reference})";
    }
}

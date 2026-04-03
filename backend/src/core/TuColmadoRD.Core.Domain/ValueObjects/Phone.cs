using System.Text.RegularExpressions;
using TuColmadoRD.Core.Domain.Base.Result;

namespace TuColmadoRD.Core.Domain.ValueObjects
{
    public record Phone
    {
        public string Value { get; private init; }

        private Phone(string value) => Value = value;

        public static OperationResult<Phone, string> Create(string? input)
        {
            var sanitized = Regex.Replace(input ?? "", @"[^0-9]", "");

            if (sanitized.Length != 10)
                return OperationResult<Phone, string>.Bad("El teléfono debe tener 10 dígitos.");

            if (!Regex.IsMatch(sanitized, @"^(809|829|849)"))
                return OperationResult<Phone, string>.Bad("El prefijo no corresponde a República Dominicana.");

            return OperationResult<Phone, string>.Good(new Phone(sanitized));
        }

        public string ToFormattedString() 
            => $"({Value[..3]}) {Value.Substring(3, 3)}-{Value[6..]}";
    }
}

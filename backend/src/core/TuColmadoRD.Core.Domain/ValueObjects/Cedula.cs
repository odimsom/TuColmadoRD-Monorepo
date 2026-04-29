using System.Text.RegularExpressions;
using TuColmadoRD.Core.Domain.Base.Result;

namespace TuColmadoRD.Core.Domain.ValueObjects
{
    public record Cedula
    {
        public string Value { get; private init; }
        private Cedula(string value)
        {
            Value = value;
        }
        public static OperationResult<Cedula, string> Create(string? input)
        {
            string sanitized = Sanitize(input);

            if (string.IsNullOrWhiteSpace(sanitized) || sanitized.Length != 11)
            {
                return OperationResult<Cedula, string>.Bad("La cédula debe tener exactamente 11 dígitos numéricos.");
            }

            return OperationResult<Cedula, string>.Good(new Cedula(sanitized));
        }
        private static string Sanitize(string? value)
            => Regex.Replace(value ?? "", @"[^0-9]", "");
        private static bool VerifyDigit(string cedula)
        {
            int vnTotal = 0;
            int[] weight = [1, 2, 1, 2, 1, 2, 1, 2, 1, 2];

            for (int i = 0; i < 10; i++)
            {
                int digit = int.Parse(cedula[i].ToString());
                int res = digit * weight[i];
                if (res > 9) res = (res / 10) + (res % 10);
                vnTotal += res;
            }

            int checkDigit = (10 - (vnTotal % 10)) % 10;
            return checkDigit == int.Parse(cedula[10].ToString());
        }
        public string ToFormattedString()
            => $"{Value[..3]}-{Value[3..10]}-{Value[10..]}";
    }
}

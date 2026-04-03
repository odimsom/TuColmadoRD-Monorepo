using System.Text.RegularExpressions;
using TuColmadoRD.Core.Domain.Base.Result;

namespace TuColmadoRD.Core.Domain.ValueObjects
{
    public record Rnc
    {
        public string Value { get; private init; }

        private Rnc(string value) => Value = value;

        public static OperationResult<Rnc, string> Create(string? input)
        {
            string sanitized = Sanitize(input);

            if (string.IsNullOrWhiteSpace(sanitized) || sanitized.Length != 9)
                return OperationResult<Rnc, string>.Bad("El RNC debe tener exactamente 9 dígitos.");

            if (!VerifyDigit(sanitized))
                return OperationResult<Rnc, string>.Bad("El RNC no es válido (Fallo de dígito verificador).");

            return OperationResult<Rnc, string>.Good(new Rnc(sanitized));
        }

        private static string Sanitize(string? value)
            => Regex.Replace(value ?? "", @"[^0-9]", "");

        private static bool VerifyDigit(string rnc)
        {
            int[] weights = { 7, 9, 8, 6, 5, 4, 3, 2 };
            int sum = 0;

            for (int i = 0; i < 8; i++)
            {
                int digit = int.Parse(rnc[i].ToString());
                sum += digit * weights[i];
            }

            int remainder = sum % 11;
            int digitCheck;

            if (remainder == 0)
                digitCheck = 2;
            else if (remainder == 1)
                digitCheck = 1;
            else
                digitCheck = 11 - remainder;

            if (digitCheck == 10) digitCheck = 1;

            return digitCheck == int.Parse(rnc[8].ToString());
        }

        public override string ToString() => Value;

        public string ToFormattedString()
            => $"{Value.Substring(0, 1)}-{Value.Substring(1, 2)}-{Value.Substring(3, 5)}-{Value.Substring(8, 1)}";
    }
}
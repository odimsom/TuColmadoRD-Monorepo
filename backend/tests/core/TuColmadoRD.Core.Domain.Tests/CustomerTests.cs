using System;
using TuColmadoRD.Core.Domain.Entities.Customers;
using TuColmadoRD.Core.Domain.ValueObjects;
using Xunit;

namespace TuColmadoRD.Core.Domain.Tests.Entities.Customers
{
    public class CustomerTests
    {
        [Fact]
        public void Create_WithValidData_ReturnsSuccess()
        {
            var tenantId = TenantIdentifier.Validate(Guid.NewGuid()).Result;
            var cedulaResult = Cedula.Create("40212345678");
            var cedula = cedulaResult.Result;
            string fullName = "Juan Perez";

            var result = Customer.Create(tenantId!, fullName, cedula!);

            Assert.True(result.IsGood);
            Assert.NotNull(result.Result);
            Assert.Equal(fullName, result.Result.FullName);
            Assert.True(result.Result.IsActive);
        }

        [Fact]
        public void Create_WithEmptyName_ReturnsFailure()
        {
            var tenantId = TenantIdentifier.Validate(Guid.NewGuid()).Result;
            var cedulaResult = Cedula.Create("40212345678");
            var cedula = cedulaResult.Result;
            string fullName = "";

            var result = Customer.Create(tenantId!, fullName, cedula!);

            Assert.False(result.IsGood);
            Assert.Equal("El nombre completo es obligatorio.", result.Error);
        }
    }
}

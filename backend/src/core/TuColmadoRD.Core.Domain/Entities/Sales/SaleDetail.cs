using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Sales
{
    public class SaleDetail
    {
        private SaleDetail()
        {
            Quantity = Quantity.Of(1m).Result;
            UnitPrice = Money.Zero;
            TaxAmount = Money.Zero;
            SubTotal = Money.Zero;
        }
        public Guid Id { get; private set; }
        public Guid SaleId { get; private set; }
        public Guid ProductId { get; private set; }
        public Quantity Quantity { get; private set; }
        public Money UnitPrice { get; private set; }
        public Money TaxAmount { get; private set; }
        public Money SubTotal { get; private set; }

        private SaleDetail(Guid saleId, Guid productId, Quantity quantity, Money unitPrice, Money taxAmount, Money subTotal)
        {
            Id = Guid.NewGuid();
            SaleId = saleId;
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            TaxAmount = taxAmount;
            SubTotal = subTotal;
        }

        internal static OperationResult<SaleDetail, string> Create(Guid saleId, Guid productId, Quantity quantity, Money unitPrice, TaxRate taxRate)
        {
            var subtotalResult = Money.FromDecimal(quantity.Value * unitPrice.Amount);
            if (!subtotalResult.IsGood) return OperationResult<SaleDetail, string>.Bad(subtotalResult.Error.ToString());

            var taxResult = taxRate.CalculateTax(subtotalResult.Result!);
            if (!taxResult.IsGood) return OperationResult<SaleDetail, string>.Bad(taxResult.Error.ToString());

            return OperationResult<SaleDetail, string>.Good(
                new SaleDetail(saleId, productId, quantity, unitPrice, taxResult.Result!, subtotalResult.Result!)
            );
        }
    }
}

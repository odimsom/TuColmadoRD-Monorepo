using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;

public sealed class FundExpenseCategory : Enumeration
{
    public static readonly FundExpenseCategory StockPurchase  = new(1, nameof(StockPurchase));
    public static readonly FundExpenseCategory Operational    = new(2, nameof(Operational));
    public static readonly FundExpenseCategory Loss           = new(3, nameof(Loss));
    public static readonly FundExpenseCategory ExternalFund   = new(4, nameof(ExternalFund));
    public static readonly FundExpenseCategory Other          = new(5, nameof(Other));

    private FundExpenseCategory(int id, string name) : base(id, name) { }

    public static OperationResult<FundExpenseCategory, DomainError> FromId(int id)
    {
        var result = id switch
        {
            1 => StockPurchase,
            2 => Operational,
            3 => Loss,
            4 => ExternalFund,
            5 => Other,
            _ => null
        };

        return result is null
            ? OperationResult<FundExpenseCategory, DomainError>.Bad(DomainError.Validation("fund_expense_category.unknown_id"))
            : OperationResult<FundExpenseCategory, DomainError>.Good(result);
    }

    public static implicit operator int(FundExpenseCategory c) => c.Id;
}

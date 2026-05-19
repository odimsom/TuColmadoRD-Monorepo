using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;

public sealed class FundTransactionType : Enumeration
{
    public static readonly FundTransactionType Deposit = new(1, nameof(Deposit));
    public static readonly FundTransactionType Expense = new(2, nameof(Expense));

    private FundTransactionType(int id, string name) : base(id, name) { }

    public static OperationResult<FundTransactionType, DomainError> FromId(int id)
    {
        var result = id switch
        {
            1 => Deposit,
            2 => Expense,
            _ => null
        };

        return result is null
            ? OperationResult<FundTransactionType, DomainError>.Bad(DomainError.Validation("fund_transaction_type.unknown_id"))
            : OperationResult<FundTransactionType, DomainError>.Good(result);
    }

    public static implicit operator int(FundTransactionType t) => t.Id;
}

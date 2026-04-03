namespace TuColmadoRD.Presentation.API.Endpoints.Expenses;

public sealed record RegisterExpenseRequest(
    decimal Amount,
    string Category,
    string Description);

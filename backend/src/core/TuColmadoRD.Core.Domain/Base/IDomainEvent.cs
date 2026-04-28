using MediatR;

namespace TuColmadoRD.Core.Domain.Base;

/// <summary>
/// Base interface for all domain events.
/// Inherits from INotification to support MediatR publishing.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredAt { get; }
}

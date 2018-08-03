namespace Lykke.Service.History.Contracts.Cqrs.Models.Enums
{
    public enum OrderStatus
    {
        Unknown,
        Placed,
        PartiallyMatched,
        Matched,
        Pending,
        Cancelled,
        Replaced,
        Rejected
    }
}

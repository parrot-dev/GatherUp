namespace GatherUp.Order.Parsing
{
    interface IOrderParser
    {
        Profile ToProfile();
        bool IsValidVersion { get; }
    }
}

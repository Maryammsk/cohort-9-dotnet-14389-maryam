namespace TaskManagement.Identity;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Client = "Client";
    public const string Manager = "Manager";

    public static readonly IReadOnlyCollection<string> All = [Admin, Client, Manager];
}

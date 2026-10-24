namespace WebAPIPreparation.Auth
{
    public static class Permissions
    {
        public const string CustomersView = "Customers.View";
        public const string CustomersEdit = "Customers.Edit";
        public const string CustomersDelete = "Customers.Delete";

        public const string MastersView = "Masters.View";    // Country, State
        public const string MastersEdit = "Masters.Edit";

        public const string UsersManage = "Users.Manage";

        // ONE place that says which role gets which permission
        public static readonly Dictionary<string, string[]> ByRole = new()
        {
            [Roles.Admin] = [CustomersView, CustomersEdit, CustomersDelete, MastersView, MastersEdit, UsersManage],
            [Roles.Manager] = [CustomersView, CustomersEdit, CustomersDelete, MastersView, MastersEdit],
            [Roles.User] = [CustomersView, MastersView]
        };
    }
}

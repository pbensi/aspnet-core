namespace api.repository.Utilities
{
    public enum OperationType
    {
        None,
        Add,
        Update,
        Remove,
    }

    public static class UserAudit
    {
        public static int Id { get; set; }
    }
}

namespace api.shared.Dto
{
    public class PermissionDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool CanAdd { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanRemove { get; set; }
        public bool CanView { get; set; }
        public List<PermissionDto> Children { get; set; }
    }
}

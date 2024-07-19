namespace TheSailOS.Permissions;

public class Permissions
{
    public string Owner { get; set; }
    public string Group { get; set; }
    public PermissionType OwnerPermissions { get; set; }
    public PermissionType GroupPermissions { get; set; }
    public PermissionType OtherPermissions { get; set; }

    public Permissions(string owner, string group, PermissionType ownerPermissions, PermissionType groupPermissions, PermissionType otherPermissions)
    {
        Owner = owner;
        Group = group;
        OwnerPermissions = ownerPermissions;
        GroupPermissions = groupPermissions;
        OtherPermissions = otherPermissions;
    }
}
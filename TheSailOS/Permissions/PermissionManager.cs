using System;
using System.Linq;
using TheSailOS.Users;

namespace TheSailOS.Permissions;

public class PermissionManager
{
    private User currentUser;
    private PermissionStore permissionStore;

    public PermissionManager(User currentUser, PermissionStore permissionStore)
    {
        this.currentUser = currentUser;
        this.permissionStore = permissionStore;
    }

    public bool HasPermission(string path, PermissionType permissionType)
    {
        var permissions = permissionStore.GetPermissions(path);
        if (permissions == null)
        {
            throw new Exception($"Permissions not found for path {path}");
        }

        if (currentUser.Username == permissions.Owner)
        {
            return permissions.OwnerPermissions.HasFlag(permissionType);
        }

        if (currentUser.Groups.Any(group => group.Name == permissions.Group))
        {
            return permissions.GroupPermissions.HasFlag(permissionType);
        }

        return permissions.OtherPermissions.HasFlag(permissionType);
    }
    
    public void SetCurrentUser(User user)
    {
        currentUser = user;
    }
}
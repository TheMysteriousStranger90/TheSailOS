using System.Collections.Generic;
using TheSailOS.Users;

namespace TheSailOS.Permissions;

public class PermissionStore
{
    private Dictionary<string, TheSailOS.Permissions.Permissions> permissions;
    public User DefaultUser { get; set; }

    public PermissionStore(User defaultUser)
    {
        permissions = new Dictionary<string, TheSailOS.Permissions.Permissions>();
        DefaultUser = defaultUser;
    }

    public void SetPermissions(string path, TheSailOS.Permissions.Permissions permissions)
    {
        this.permissions[path] = permissions;
    }

    public TheSailOS.Permissions.Permissions GetPermissions(string path)
    {
        if (permissions.TryGetValue(path, out var pathPermissions))
        {
            return pathPermissions;
        }


        return new TheSailOS.Permissions.Permissions(DefaultUser.Username, "admin", PermissionType.All, PermissionType.All, PermissionType.All);
    }
}
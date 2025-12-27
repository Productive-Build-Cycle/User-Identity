
namespace Identity.Core.Dtos.Roles;

// ---------- Requests ----------
public record AddRoleRequest(
    string Name,
    string Description
)
{
    public AddRoleRequest() : this(default, default)
    {
    }
}
public record UpdateRoleRequest(
    Guid Id,
    string Name,
    string Description
)
{
    public UpdateRoleRequest() : this(default, default, default)
    { 
    }
};

public record AssignRoleToUserRequest(
    Guid UserId,
    string RoleName
)
{
    public AssignRoleToUserRequest() : this(default, default)
    {
    }
};

// ---------- Responses ----------
public record RoleResponse(
    Guid Id,
    string Name,
    string Description
)
{
    public RoleResponse() : this(default, default, default)
    {
    }
};

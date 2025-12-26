namespace Identity.Core.Dtos.Roles;

// ---------- Requests ----------
public record AddRoleRequest(
    string Name
);

public record UpdateRoleRequest(
    Guid Id,
    string Name
);

public record AssignRoleToUserRequest(
    Guid UserId,
    string RoleName
);

// ---------- Responses ----------
public record RoleResponse(
    Guid Id,
    string Name
);

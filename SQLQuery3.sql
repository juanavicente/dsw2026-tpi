SELECT 
    u.Email,
    r.Name AS Rol
FROM Users u
LEFT JOIN UsersRoles ur ON u.Id = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.Id
WHERE u.Email = 'desarrollo@test.com';
namespace Authorization.SSO.Extensions
{
    public static class UserExtensions
    {
        public static IUser Model(this User user)
        {
            var model = new UserModel
            {
                BusinessTitle = user.BusinessTitle,
                CompanyId = user.CompanyId,
                CustomSignature = user.CustomSignature,
                Department = user.Department,
                Email = user.Email,
                FullName = user.FullName,
                Id = user.Id,
                Sub = user.Id.ToString(),
                IsInactive = user.IsInactive,
                IsSuperAdmin = user.IsSuperAdmin,
                MobilePhone = user.MobilePhone,
                OfficePhone = user.OfficePhone,
                Username = user.UserName,
                LastActivity = user.LastActivity
            };
            return model;
        }
        public static IUser Model(this User user, IEnumerable<UserRole> userRoles)
        {
            var model = user.Model();
            model.RoleIds = userRoles.Where(ur => ur.UserId == model.Id).Select(ur => ur.RoleId);
            return model;
        }


    }
}

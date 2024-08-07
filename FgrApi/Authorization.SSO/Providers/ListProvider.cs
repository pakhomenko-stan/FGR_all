using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Authorization.Core;
using Authorization.Core.Models;
using Authorization.SSO.Extensions;
using Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.SSO.Providers
{
    public class ListProvider
    {
        private const double timeout = 3000;

        private readonly IServiceProvider serviceProvider;
        private readonly Timer userTimer;
        private readonly Timer roleTimer;

        private List<UserRole> userRoles;
        private bool isRolesReady;
        private bool isUsersReady;
        private bool isUsersRolesReady;

        private readonly object usersLock = new();
        private readonly object rolesLock = new();

        private List<IUser> users;
        private List<Role> roles;

        public ListProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            isRolesReady = false;
            isUsersReady = false;
            isUsersRolesReady = false;

            userTimer = new Timer
            {
                Interval = timeout,
                AutoReset = false
            };

            roleTimer = new Timer
            {
                Interval = timeout,
                AutoReset = false
            };

            userTimer.Elapsed += UserTimer_Elapsed;
            roleTimer.Elapsed += RoleTimer_Elapsed;
        }
        public IEnumerable<Role> GetRoles => GetRolesMethod();
        public IEnumerable<IUser> GetUsers => GetUsersMethod();

        private void RoleTimer_Elapsed(object sender, ElapsedEventArgs e) => isRolesReady = false;

        private void UserTimer_Elapsed(object sender, ElapsedEventArgs e) => isUsersReady = false;


        private IEnumerable<Role> GetRolesMethod()
        {
            if (isRolesReady) return roles;
            lock (rolesLock)
            {
                using var scope = serviceProvider.CreateScope();
                RoleManager<Role> roleManager = scope.ServiceProvider.GetService<RoleManager<Role>>();

                roles = roleManager.Roles.ToList();
                isRolesReady = true;
                roleTimer.Stop();
                roleTimer.Start();
                return roles;
            }
        }

        private IEnumerable<IUser> GetUsersMethod()
        {
            if (isUsersReady) return users;
            lock (usersLock)
            {
                if (!isUsersRolesReady) SetUserRoles();
                using var scope = serviceProvider.CreateScope();
                UserManager<User> userManager = scope.ServiceProvider.GetService<UserManager<User>>();

                var usersWithoutRole = userManager.Users.ToList();
                users = usersWithoutRole.Select(u => u.Model(userRoles)).ToList();
                isUsersReady = true;
                userTimer.Stop();
                userTimer.Start();
                return users;
            }
        }

        private void SetUserRoles()
        {
            using var scope = serviceProvider.CreateScope();
            AuthenticationDbContext context = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
            userRoles = context.UserRoles.ToList();
            isUsersRolesReady = true;
        }
    }
}

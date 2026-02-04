using SmartInventoryManagementSystem.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public int UserId => 2; // TEMP: fake logged-in user
    }
}

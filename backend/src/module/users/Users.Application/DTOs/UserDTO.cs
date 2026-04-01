using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Application.DTOs
{
    public class UserDTO
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime LastLogin { get; set; }
    }
}

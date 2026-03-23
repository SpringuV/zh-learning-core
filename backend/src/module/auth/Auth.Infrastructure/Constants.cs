using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Application
{
    public class Constants
    {
        public const string JWT_ISSUER = "spring-uv-19";
        public const string ADMINISTRATORS = "Administrators";
        public const string USERS = "Users";
        public const string POLICY_USER_ONLY = "UserOnly";
        public const string POLICY_ADMIN_ONLY = "AdminOnly";
        public const string POLICY_ADMIN_OR_USER = "AdminOrUser";
    }
}

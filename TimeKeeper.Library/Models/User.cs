using System;
using System.Collections.Generic;

namespace TimeKeeper.SharedLibrary.Models
{
    public class User
    {
        public User()
        {
        }

        // Login info
        public string UserID { get; init; } // shouldn't modify once created.
        public string Username { get; set; } // username == email / Emails must be unique.
        public IList<string> Roles { get; init; } = new List<string>();

        // Profile info
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        // 
    }

    //public record Role(string roleName);
}

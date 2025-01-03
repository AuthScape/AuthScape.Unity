using System;
using System.Collections.Generic;

namespace Assets.Plugins.AuthScape.Models
{
    [Serializable]
    public class SignedInUser
    {
        public long id;
        public string email;
        public string firstName;
        public string lastName;
        public Guid? identifier;
        public long? companyId;
        public string companyName;
        public long locationId;    
        public string locationName;
        public string locale;
        public List<QueryRole> roles;
        public List<string> permissions;
    }
}

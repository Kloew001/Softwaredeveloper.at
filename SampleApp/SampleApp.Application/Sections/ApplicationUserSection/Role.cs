using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Application.Sections.ApplicationUserSection
{

    public enum UserRoleType
    {
        [EnumExtension(MultilingualDisplayNameKey = "Admin", Id = "A61EC1E6-5EEC-4A9D-8B8C-0AC1813202C7")]
        Admin = 0,

        [EnumExtension(MultilingualDisplayNameKey = "User", Id = "51B48311-1067-4C4C-BBBB-96F2F29188E2")]
        User = 1
    }
}

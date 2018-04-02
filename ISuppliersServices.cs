using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Helpers;

namespace IMS.Controllers.Services
{
    interface ISuppliersServices
    {
        Response CheckForDuplicateCode(Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput duplicateCodeCheckInput);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using IMS.Models;
using System.Configuration;
using IMS.Helpers;
using System.Data;
using System.Data.SqlClient;

namespace IMS.Controllers.Services
{
    interface IStoresServices
    {
        Response CheckForDuplicateCode(Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput duplicateCodeCheckInput);
    }
}

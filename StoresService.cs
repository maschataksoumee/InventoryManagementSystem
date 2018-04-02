using IMS.Controllers.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IMS.Helpers;
using IMS.Models;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using IMS.Helpers.Constants;
using IMS.Helpers.Logger;

namespace IMS.Controllers.Implementation
{
    public class StoresService: IStoresServices
    {
        public Response CheckForDuplicateCode(Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput duplicateCodeCheckInput)
        {
            Response response = new Response();

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Reports.CheckForDuplicateCode";
            Logger.Write(logIdentifier, duplicateCodeCheckInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES_SERVICE__CHECK_FOR_DUPLICATE_CODE);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.STORES_SERVICE__CHECK_FOR_DUPLICATE_CODE);
            //-Soumee - 29-March-2018 - Adding Loggers

            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            response.IsOk = true;

            string duplicateCodeCheck = @"SELECT CODE
                                            FROM " + duplicateCodeCheckInput.TableName + @"
                                            WHERE CODE = @CODE
                                                AND ACTIVE = 1 
                                                AND DELETED = 0";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(duplicateCodeCheck, con))
                {
                    cmd.Parameters.AddWithValue("@CODE", duplicateCodeCheckInput.Code);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            response.IsOk = false;

                            //Soumee - 29-March-2018 - Adding Loggers        
                            Logger.Write(logIdentifier, "Checking for response inside the reader: " + response.IsOk, DateTime.Now.Ticks, LoggerConstants.STORES_SERVICE__CHECK_FOR_DUPLICATE_CODE);
                        }
                    }
                }
            }

            //+Soumee - 29-March-2018 - Adding Loggers
            Logger.Write(logIdentifier, duplicateCodeCheckInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES_SERVICE__CHECK_FOR_DUPLICATE_CODE);
            Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES_SERVICE__CHECK_FOR_DUPLICATE_CODE);
            //-Soumee - 29-March-2018 - Adding Loggers

            return (response);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using IMS.Models;
using System.Configuration;
using IMS.Helpers;
using System.Data;
using System.Data.SqlClient;
using IMS.Controllers.Implementation;
using IMS.Helpers.Logger;
using IMS.Helpers.Constants;

namespace IMS.Controllers
{
    [RoutePrefix("api/Stores")]
    public class StoresController : ApiController
    {
        /// <summary>
        /// This api is used to fetch the Store categories. If StoreCategoryId = 0, all store categories are fetched.
        /// </summary>
        /// Created By: Soumee
        /// Date: 15th March, 2018
        /// Issue No.: td-957
        /// Issue Description: Inhouse | Api | Inventory | Store Category
        /// Input Json
        /// {
        /// "StoreCategoryId": 0
        /// }
        /// <param name="storeRequest">StoreCategoryId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("FetchStoreCategory")]
        public IHttpActionResult FetchStoreCategory(Models.Stores.FetchStoreCategory.Request.StoreRequest storeRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            Response<List<Models.Stores.FetchStoreCategory.Response.StoreResponse>> storeResponseListResponse = new Response<List<Models.Stores.FetchStoreCategory.Response.StoreResponse>>();
            List<Models.Stores.FetchStoreCategory.Response.StoreResponse> storeResponseList = new List<Models.Stores.FetchStoreCategory.Response.StoreResponse>();
            
            //+Soumee - 28-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Stores.FetchStoreCategory";
            Logger.Write(logIdentifier, storeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_CATEGORY);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_CATEGORY);
            //-Soumee - 28-March-2018 - Adding Loggers

            string fetchStoreCategories = "";

            if (storeRequest.StoreCategoryId == 0)
            {
                //When no StoreCategoryId is passed
                fetchStoreCategories += @"SELECT Id,
                                                Name,
                                                Code
                                            FROM store_categories
                                            WHERE ACTIVE = 1
                                                AND DELETED = 0";

                Logger.Write(logIdentifier, "storeRequest.StoreCategoryId == 0", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_CATEGORY);//Soumee - 28-March-2018 - Adding Loggers
            }
            else
            {
                //When StoreCategoryId is passed
                fetchStoreCategories += @"SELECT Id,
                                                Name,
                                                Code
                                        FROM store_categories
                                        WHERE Id=@StoreId
                                          AND ACTIVE = 1
                                          AND DELETED = 0";

                Logger.Write(logIdentifier, "storeRequest.StoreCategoryId != 0", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_CATEGORY);//Soumee - 28-March-2018 - Adding Loggers
            }

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(fetchStoreCategories, con))
                {
                    if (storeRequest.StoreCategoryId != 0)
                    {
                        cmd.Parameters.AddWithValue("@StoreId", storeRequest.StoreCategoryId);
                    }

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Models.Stores.FetchStoreCategory.Response.StoreResponse storeResponseReader = new Models.Stores.FetchStoreCategory.Response.StoreResponse();

                            storeResponseReader.StoreCategoryId = Convert.ToInt32(dr["Id"]);
                            storeResponseReader.StoreCategoryName = Convert.ToString(dr["Name"]);
                            storeResponseReader.StoreCategoryCode = Convert.ToString(dr["Code"]);

                            storeResponseList.Add(storeResponseReader);
                            Logger.Write(logIdentifier, "Populating the object list Inside the Reader", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_CATEGORY);//Soumee - 28-March-2018 - Adding Loggers
                        }
                        storeResponseListResponse.ResponseObject = storeResponseList;
                    }
                }

                if (storeResponseList.Count > 0)
                {
                    storeResponseListResponse.IsOk = true;
                    storeResponseListResponse.Message = "Store Category Details are";

                    //+Soumee - 28-March-2018 - Adding Loggers
                    Logger.Write(logIdentifier, storeResponseListResponse.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_CATEGORY);
                    Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_CATEGORY);                    
                    //-Soumee - 28-March-2018 - Adding Loggers
                }
                else
                {
                    storeResponseListResponse.Message = "No store categories found";

                    //+Soumee - 28-March-2018 - Adding Loggers
                    Logger.Write(logIdentifier, storeResponseListResponse.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_CATEGORY);
                    Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_CATEGORY);                    
                    //-Soumee - 28-March-2018 - Adding Loggers
                }
                return Ok(storeResponseListResponse);
            }

        }

        /// <summary>
        /// This api is used to fetch the Store types. If StoreTypeId = 0, all store types are fetched.
        /// </summary>
        /// Created By: Soumee
        /// Date: 15th March, 2018
        /// Issue No.: td-958
        /// Issue Description: Inhouse | API | Inventory | Store Types
        /// Input Json
        /// {
        /// "StoreTypeId": 0
        /// }
        /// <param name="storeRequest">StoreTypeId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("FetchStoreType")]
        public IHttpActionResult FetchStoreType(Models.Stores.FetchStoreType.Request.StoreTypeRequest storeTypeRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            List<Models.Stores.FetchStoreType.Response.StoreTypeResponse> storeTypeResponseList = new List<Models.Stores.FetchStoreType.Response.StoreTypeResponse>();
            Response<List<Models.Stores.FetchStoreType.Response.StoreTypeResponse>> storeTypeResponseListResponse = new Response<List<Models.Stores.FetchStoreType.Response.StoreTypeResponse>>();

            //+Soumee - 28-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Stores.FetchStoreType";
            Logger.Write(logIdentifier, storeTypeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_TYPE);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_TYPE);
            //-Soumee - 28-March-2018 - Adding Loggers

            string fetchStoreType = "";

            if (storeTypeRequest.StoreTypeId == 0)
            {
                //When no StoreTypeId is passed
                fetchStoreType += @"SELECT ID,
                                           NAME,
                                           CODE
                                    FROM STORE_TYPES
                                    WHERE ACTIVE = 1
                                        AND DELETED = 0";
            }
            else
            {
                //When StoreTypeId is passed
                fetchStoreType += @"SELECT ID,
                                           NAME,
                                           CODE
                                    FROM STORE_TYPES
                                    WHERE ID = @ID
                                    AND ACTIVE = 1
                                        AND DELETED = 0";
            }
            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(fetchStoreType, con))
                {
                    if (storeTypeRequest.StoreTypeId != 0)
                    {
                        cmd.Parameters.AddWithValue("@ID", storeTypeRequest.StoreTypeId);
                    }

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Models.Stores.FetchStoreType.Response.StoreTypeResponse storeTypeResponse = new Models.Stores.FetchStoreType.Response.StoreTypeResponse();

                            storeTypeResponse.StoreTypeId = Convert.ToInt32(dr["ID"]);
                            storeTypeResponse.StoreTypeName = Convert.ToString(dr["NAME"]);
                            storeTypeResponse.StoreTypeCode = Convert.ToString(dr["Code"]);

                            storeTypeResponseList.Add(storeTypeResponse);
                            Logger.Write(logIdentifier, "Populating the object list Inside the Reader", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_TYPE);//Soumee - 28-March-2018 - Adding Loggers
                        }
                    }
                    storeTypeResponseListResponse.ResponseObject = storeTypeResponseList;
                }
            }

            if (storeTypeResponseList.Count != 0)
            {
                storeTypeResponseListResponse.IsOk = true;
                storeTypeResponseListResponse.Message = "Store types are";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeTypeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_TYPE);                
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            else
            {
                storeTypeResponseListResponse.Message = "No store types found.";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeTypeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORE_TYPE);                
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            return Ok(storeTypeResponseListResponse);
        }

        /// <summary>
        /// This api is used to create stores
        /// </summary>
        /// Created By: Soumee
        /// Date: 15th March, 2018
        /// Issue No.: td-959
        /// Issue Description: Inhouse | API | Inventory | Create a Store
        /// Input JSON:
        /// {
        /// "StoreName": "You and Me",
        /// "StoreCode": "YM",
        /// "StoreTypeId": [1,2,3],
        /// "StoreCategoryId": [1,2],
        /// "StoreInvoicePrefix": "YM",
        /// "StoreMobile":"501812845",
        /// "StoreEmail": "youandme@gmail.com",
        /// "StoreLocation": "Kthao Akta ache",
        /// "StoreCity": "Kolkata",
        /// "StoreState": "West Bengal"
        /// }
        /// <param name="storeRequest">StoreName, StoreCode, StoreTypeId, StoreCategoryId, StoreInvoicePrefix, StoreMobile, StoreEmail, StoreLocation, StoreCity, StoreState</param>
        /// <returns> An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("CreateStore")]
        public IHttpActionResult CreateStore(Models.Stores.CreateStore.Request.StoreInputRequest storeRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            Response<Models.Stores.CreateStore.Response.StoreResponse> storeResponse = new Response<Models.Stores.CreateStore.Response.StoreResponse>();
            Models.Stores.CreateStore.Response.StoreResponse responseStoreId = new Models.Stores.CreateStore.Response.StoreResponse();

            //+Soumee - 28-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Stores.CreateStore";
            Logger.Write(logIdentifier, storeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE);
            //-Soumee - 28-March-2018 - Adding Loggers

            //+Soumee - 28-March-2018 - Check for code Duplicacy
            Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput duplicateCodeCheckInput = new Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput();

            duplicateCodeCheckInput.Code = storeRequest.StoreCode;
            duplicateCodeCheckInput.TableName = "Stores";

            IMS.Controllers.Services.IStoresServices storesServices = new StoresService();
            storeResponse.IsOk = storesServices.CheckForDuplicateCode(duplicateCodeCheckInput).IsOk;

            if (storeResponse.IsOk == false)
            {
                storeResponse.Message = "This Code already exists. Please try a different code.";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE);
                //-Soumee - 28-March-2018 - Adding Loggers

                return Ok(storeResponse);
            }
            //-Soumee - 28-March-2018 - Check for code Duplicacy
            
            int storeId = 0;
            //Query to insert into Stores table
            string createStore = @"INSERT INTO Stores
                                    VALUES(@NAME,
                                           @CODE,
                                           @INVOICE_PREFIX,
                                           @STORE_CATEGORY_ID,
                                           @STORE_TYPE_ID,
                                           @MOBILE,
                                           @EMAIL,
                                           @LOCATION,
                                           @CITY,
                                           @I_STATE,
                                           @ACTIVE,
                                           @DELETED,
                                           @CREATED,
                                           @MODIFIED,
                                           @CREATED_BY,
                                           @MODIFIED_BY);
                                SELECT SCOPE_IDENTITY()";

            //Query to insert into dbo.Store_to_Store_Categories. Pass Store_Category_Id = StoreCatgeoryId for a single id, Store_Category_Id = 0 for all the store categories, pass Store_Category_Id = -1 for multiple store categories, pass Store_Category_Id = -2 for no category Ids.
            string storeToStoreCategories = "INSERT INTO STORE_TO_STORE_CATEGORIES(Store_Id, Store_Category_Id, Active, Deleted, Created_Time, Modified_Time, Created_By, Modified_By) VALUES";

            //Query to insert into dbo.Store_to_Store_Types. Pass Store_Type_Id = StoreTypeId for a single id, Store_Type_Id = 0 for all the store categories, pass Store_Type_Id = -1 for multiple store categories. This is a mandatory field.
            string storeToStoreTypes = "INSERT INTO STORE_TO_STORE_TYPES(Store_Id, Store_Type_Id, Active, Deleted, Created_Time, Modified_Time, Created_By, Modified_By) VALUES";

            int storeCategoryCount = 0; // To store the total count of Store Categories given as input
            int storeCategoryCountInDB = 0; //To fetch the total count of the Store Categories in db
            int storeCategoryId = 0; //For single storeCategoryId

            //To fetch the count of Store_Categories
            string fetchStoreCategoryCount = @"SELECT COUNT(*) AS STORECATEGORYCOUNT
                                                FROM Store_Categories";

            int storeTypeCount = 0; // To store the total count of the Store Types given as input
            int storeTypeCountInDB = 0; // To fetch the total count of the Store Types in db
            int storeTypeId = 0; // For single storeTypeId
            
            //To fetch the total count of Store_Types
            string fetchStoreTypeCount = @"SELECT COUNT(*) AS STORETYPECOUNT
                                            FROM Store_Types";

            if(storeRequest.StoreCategoryId != null)
            {
                storeCategoryCount = storeRequest.StoreCategoryId.Count;
            }
            else
            {
                storeCategoryCount = 0;
            }
            
            storeTypeCount = storeRequest.StoreTypeId.Count; 

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();

                if (storeRequest.StoreCategoryId != null)
                {
                    using (SqlCommand cmd = new SqlCommand(fetchStoreCategoryCount, con))
                    {
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                storeCategoryCountInDB = Convert.ToInt32(dr["STORECATEGORYCOUNT"]);
                                Logger.Write(logIdentifier, "Fetching the Store Catgeory Count", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE);//Soumee - 28-March-2018 - Adding Loggers
                            }
                        }
                    }

                    //If store belongs to all the store categories
                    if (storeCategoryCount == storeCategoryCountInDB)
                    {
                        storeCategoryId = -1;
                    }
                    //If the store belongs to few of the Store categories
                    else if (storeCategoryCount < storeCategoryCountInDB)
                    {
                        storeCategoryId = 0;
                    }
                    else //No store categories
                    {
                        storeCategoryId = -2;
                    }
                }                    

                using (SqlCommand cmd = new SqlCommand(fetchStoreTypeCount, con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            storeTypeCountInDB = Convert.ToInt32(dr["STORETYPECOUNT"]);
                            Logger.Write(logIdentifier, "Fetching the Store Type Count", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE);//Soumee - 28-March-2018 - Adding Loggers
                        }
                    }
                }                

                //If the store belongs to all the store types
                if (storeTypeCount == storeTypeCountInDB)
                {
                    storeTypeId = -1;
                }
                //If the store belongs to a few of the store types
                else if (storeTypeCount < storeTypeCountInDB)
                {
                    storeTypeId = 0;
                }
                else //No store types. This case should not be considered as store types is mandatory
                {
                    storeTypeId = -2;
                }

                //Creating a new store
                using (SqlCommand cmd = new SqlCommand(createStore, con))
                {
                    cmd.Parameters.AddWithValue("@NAME", storeRequest.StoreName);
                    cmd.Parameters.AddWithValue("@CODE", storeRequest.StoreCode);
                    cmd.Parameters.AddWithValue("@INVOICE_PREFIX", storeRequest.StoreInvoicePrefix);
                    cmd.Parameters.AddWithValue("@STORE_CATEGORY_ID", storeCategoryId);
                    cmd.Parameters.AddWithValue("@STORE_TYPE_ID", storeTypeId);
                    cmd.Parameters.AddWithValue("@MOBILE", storeRequest.StoreMobile);
                    cmd.Parameters.AddWithValue("@EMAIL", storeRequest.StoreEmail);
                    cmd.Parameters.AddWithValue("@LOCATION", storeRequest.StoreLocation);
                    cmd.Parameters.AddWithValue("@CITY", storeRequest.StoreCity);
                    cmd.Parameters.AddWithValue("@I_STATE", storeRequest.StoreState);
                    cmd.Parameters.AddWithValue("@ACTIVE", 1);
                    cmd.Parameters.AddWithValue("@DELETED", 0);
                    cmd.Parameters.AddWithValue("@CREATED", DateTime.Now);
                    cmd.Parameters.AddWithValue("@MODIFIED", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CREATED_BY", 0);
                    cmd.Parameters.AddWithValue("@MODIFIED_BY", 0);

                    storeId = int.Parse(cmd.ExecuteScalar().ToString());
                    responseStoreId.StoreId = storeId; //Storing the storeId generated in response
                }

                //As this is not mandatory, we will run this part only when the user has selected the storeCategory
                if(storeRequest.StoreCategoryId != null)
                {
                    foreach (int storeCategoryIdentifier in storeRequest.StoreCategoryId)
                    {
                        storeToStoreCategories += " (";
                        storeToStoreCategories += " '" + storeId + "',";
                        storeToStoreCategories += " '" + storeCategoryIdentifier + "',";
                        storeToStoreCategories += " 1, 0, GETDATE(), GETDATE(), 0, 0),";
                    }
                    storeToStoreCategories = storeToStoreCategories.RemoveTrailingCharacters(1);

                    using (SqlCommand cmd = new SqlCommand(storeToStoreCategories, con))
                    {
                        cmd.ExecuteNonQuery();
                        Logger.Write(logIdentifier, "Inserting in table dbo.Store_to_Store_Categories", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE);//Soumee - 28-March-2018 - Adding Loggers
                    }
                }                

                //Classifying a store in storeType in mandatory
                foreach (int storeType in storeRequest.StoreTypeId)
                {
                    storeToStoreTypes += " (";
                    storeToStoreTypes += " '" + storeId + "',";
                    storeToStoreTypes += " '" + storeType + "',";
                    storeToStoreTypes += " 1, 0, GETDATE(), GETDATE(), 0, 0),";
                }                
                storeToStoreTypes = storeToStoreTypes.RemoveTrailingCharacters(1);                

                using (SqlCommand cmd = new SqlCommand(storeToStoreTypes, con))
                {
                    cmd.ExecuteNonQuery();
                    Logger.Write(logIdentifier, "Inserting in table dbo.Store_to_Store_Types", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE);//Soumee - 28-March-2018 - Adding Loggers
                }
            }

            if (storeId > 0)
            {
                storeResponse.IsOk = true;
                storeResponse.ResponseObject = responseStoreId;
                storeResponse.Message = "Store Created";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE);                
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            else
            {
                storeResponse.IsOk = false;
                storeResponse.Message = "Unable to create the store. Please try again.";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE);                
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            return Ok(storeResponse);
        }

        /// <summary>
        /// This api is used to edit the store details
        /// </summary>
        /// Created By: Soumee
        /// Date: 15th March, 2018
        /// Issue No.: td-963
        /// Issue Description: Inhouse | API | Inventory | Edit store details
        /// Input JSON:
        /// {
        /// "StoreId":1,
        /// "StoreName": "Apurv Stores",
        /// "StoreCode": "APRV1",
        /// "StoreTypeId": [1],
        /// "StoreCategoryId": [1,2],
        /// "StoreInvoicePrefix": "APRV",
        /// "StoreMobile":"501512945",
        /// "StoreEmail": "apurv.vawsum@gmail.com",
        /// "StoreLocation": "Kthao Akta ache",
        /// "StoreCity": "Kolkata",
        /// "StoreState": "West Bengal"
        /// }
        /// <param name="storeRequest">StoreId, StoreName, StoreCode, StoreTypeId, StoreCategoryId, StoreInvoicePrefix, StoreMobile, StoreEmail, StoreLocation, StoreCity, StoreState</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("EditStore")]
        public IHttpActionResult EditStore(Models.Stores.EditStore.Request.StoreInput storeRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 28-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Stores.EditStore";
            Logger.Write(logIdentifier, storeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);
            //-Soumee - 28-March-2018 - Adding Loggers

            Response response = new Response();

            //+Soumee - 29-March-2018 - Check for code Duplicacy
            Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput duplicateCodeCheckInput = new Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput();

            duplicateCodeCheckInput.Code = storeRequest.StoreCode;
            duplicateCodeCheckInput.TableName = "Stores";

            IMS.Controllers.Services.IStoresServices storesServices = new StoresService();
            response.IsOk = storesServices.CheckForDuplicateCode(duplicateCodeCheckInput).IsOk;

            if (response.IsOk == false)
            {
                response.Message = "This Code already exists. Please try a different code.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);
                //-Soumee - 29-March-2018 - Adding Loggers

                return Ok(response);
            }
            //-Soumee - 29-March-2018 - Check for code Duplicacy

            int rowsAffected = 0;
            string deleteStoreToStoreCategoryMappings = "";
            string storeToStoreCategories = "";

            //Query to edit the store details
            string editStoreDetails = "UPDATE STORES SET";
            editStoreDetails += " Name = '" + storeRequest.StoreName + "',";
            editStoreDetails += " Code = '" + storeRequest.StoreCode + "',";
            editStoreDetails += " Invoice_Prefix = '" + storeRequest.StoreInvoicePrefix + "',";

            //As store category selection is not mandatory
            if(storeRequest.StoreCategoryId != null)
            {
                //Delete the StoreToStoreCategory Mappings
                deleteStoreToStoreCategoryMappings = @"DELETE
                                                        FROM STORE_TO_STORE_CATEGORIES
                                                        WHERE STORE_ID = @STORE_ID";

                //Query to insert into StoreToStoreCategory Mappings
                storeToStoreCategories = "INSERT INTO STORE_TO_STORE_CATEGORIES(Store_Id, Store_Category_Id, Active, Deleted, Created_Time, Modified_Time, Created_By, Modified_By) VALUES";

            }


            //Delete the StoreToStoreType Mappings
            string deleteStoreToStoreTypeMappings = @"DELETE
                                                    FROM STORE_TO_STORE_TYPES
                                                    WHERE STORE_ID=@STORE_ID";

            
            //Query to insert into StoreToStoreType Mappings
            string storeToStoreTypes = "INSERT INTO STORE_TO_STORE_TYPES(Store_Id, Store_Type_Id, Active, Deleted, Created_Time, Modified_Time, Created_By, Modified_By) VALUES";
            
            int storeCategoryCount = 0; // To store the total count of Store Categories given as input
            int storeCategoryCountInDB = 0; //To fetch the total count of the Store Categories in db
            int storeCategoryId = 0; //For single storeCategoryId

            //To fetch the total store categories count in db
            string fetchStoreCategoryCount = @"SELECT COUNT(*) AS STORECATEGORYCOUNT
                                                FROM Store_Categories";

            int storeTypeCount = 0; // To store the total count of Store Types given as input
            int storeTypeCountInDB = 0; //To fetch the total count of the Store Types in db
            int storeTypeId = 0; //For single storeTypeId

            //To fetch the total store type count in db
            string fetchStoreTypeCount = @"SELECT COUNT(*) AS STORETYPECOUNT
                                            FROM Store_Types";

            storeCategoryCount = storeRequest.StoreCategoryId.Count;
            storeTypeCount = storeRequest.StoreTypeId.Count;

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();

                //As store category is not mandatory
                if(storeRequest.StoreCategoryId != null)
                {                  
                    using (SqlCommand cmd = new SqlCommand(deleteStoreToStoreCategoryMappings, con))
                    {
                        cmd.Parameters.AddWithValue("@STORE_ID", storeRequest.StoreId);
                        cmd.ExecuteNonQuery();
                        Logger.Write(logIdentifier, "Deleting store_to_store_categories mappings", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);//Soumee - 28-March-2018 - Adding Loggers
                    }

                    using (SqlCommand cmd = new SqlCommand(fetchStoreCategoryCount, con))
                    {
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                storeCategoryCountInDB = Convert.ToInt32(dr["STORECATEGORYCOUNT"]);
                                Logger.Write(logIdentifier, "Fetching Store category count from db", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);//Soumee - 28-March-2018 - Adding Loggers
                            }
                        }
                    }
                }                

                using (SqlCommand cmd = new SqlCommand(deleteStoreToStoreTypeMappings, con))
                {
                    cmd.Parameters.AddWithValue("@STORE_ID", storeRequest.StoreId);
                    cmd.ExecuteNonQuery();
                    Logger.Write(logIdentifier, "Deleting store_to_store_types mappings", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);//Soumee - 28-March-2018 - Adding Loggers
                }                

                using (SqlCommand cmd = new SqlCommand(fetchStoreTypeCount, con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            storeTypeCountInDB = Convert.ToInt32(dr["STORETYPECOUNT"]);
                            Logger.Write(logIdentifier, "Fetching Store type count from db", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);//Soumee - 28-March-2018 - Adding Loggers
                        }
                    }
                }

                if (storeCategoryCount == storeCategoryCountInDB)
                {
                    storeCategoryId = -1;
                }
                else if (storeCategoryCount < storeCategoryCountInDB)
                {
                    storeCategoryId = 0;
                }
                else /*if(storeCategoryCount == 0)*/
                {
                    storeCategoryId = -2;
                }

                if (storeTypeCount == storeTypeCountInDB)
                {
                    storeTypeId = -1;
                }
                else if (storeTypeCount < storeTypeCountInDB)
                {
                    storeTypeId = 0;
                }
                else  /*if(storeTypeCount == 0)*/
                {
                    storeTypeId = -2;
                }

                editStoreDetails += " Store_Category_Id = " + storeCategoryId + ",";
                editStoreDetails += " Store_Type_Id = " + storeTypeId + ",";
                editStoreDetails += " Active = 1, Deleted = 0, Modified_Time = getdate(), Modified_By = 0 where Id = " + storeRequest.StoreId;

                using (SqlCommand cmd = new SqlCommand(editStoreDetails, con))
                {
                    rowsAffected = cmd.ExecuteNonQuery();
                    Logger.Write(logIdentifier, "Editing the store details", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);//Soumee - 28-March-2018 - Adding Loggers
                }

                if(storeRequest.StoreCategoryId != null)
                {
                    foreach (int storeCategoryIdentifier in storeRequest.StoreCategoryId)
                    {
                        storeToStoreCategories += " (";
                        storeToStoreCategories += " '" + storeRequest.StoreId + "',";
                        storeToStoreCategories += " '" + storeCategoryIdentifier + "',";
                        storeToStoreCategories += " 1, 0, GETDATE(), GETDATE(), 0, 0),";
                    }
                    storeToStoreCategories = storeToStoreCategories.RemoveTrailingCharacters(1);

                    using (SqlCommand cmd = new SqlCommand(storeToStoreCategories, con))
                    {
                        rowsAffected += cmd.ExecuteNonQuery();
                        Logger.Write(logIdentifier, "inserting in dbo.Store_to_Store_Categories", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);//Soumee - 28-March-2018 - Adding Loggers
                    }
                }                

                foreach (int storeType in storeRequest.StoreTypeId)
                {
                    storeToStoreTypes += " (";
                    storeToStoreTypes += " '" + storeRequest.StoreId + "',";
                    storeToStoreTypes += " '" + storeType + "',";
                    storeToStoreTypes += " 1, 0, GETDATE(), GETDATE(), 0, 0),";
                }                
                storeToStoreTypes = storeToStoreTypes.RemoveTrailingCharacters(1);                

                using (SqlCommand cmd = new SqlCommand(storeToStoreTypes, con))
                {
                    rowsAffected += cmd.ExecuteNonQuery();
                    Logger.Write(logIdentifier, "inserting in dbo.Store_to_Store_Types", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);//Soumee - 28-March-2018 - Adding Loggers
                }
            }

            if (rowsAffected >= 3)
            {
                response.IsOk = true;
                response.Message = "Store details updated";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            else
            {
                response.Message = "Error updating the store details. Let's try again.";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            return Ok(response);
        }

        /// <summary>
        /// This api is used to fetch the store details. If StoreId = 0, all stores are fetched.
        /// </summary>
        /// Created By: Soumee
        /// Date: 15th March, 2018
        /// Issue No.: td-964
        /// Issue Description: Inhouse | API | Inventory | Fetch store details
        /// Input JSON:
        /// {
        /// "StoreId": 0
        /// }
        /// <param name="storeRequest">StoreId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("FetchStores")]
        public IHttpActionResult FetchStores(Models.Stores.FetchStores.Request.StoreRequestParams storeRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 28-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Stores.FetchStores";
            Logger.Write(logIdentifier, storeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);
            //-Soumee - 28-March-2018 - Adding Loggers

            List<Models.Stores.FetchStores.Response.StoreResponse> storeResponseList = new List<Models.Stores.FetchStores.Response.StoreResponse>();
            List<Models.Stores.FetchStores.Response.StoreResponse> storeResponseListForReader = new List<Models.Stores.FetchStores.Response.StoreResponse>();
            Response<List<Models.Stores.FetchStores.Response.StoreResponse>> storeResponseListResponse = new Response<List<Models.Stores.FetchStores.Response.StoreResponse>>();

            int storeCategoryId = 0; //This is the storeCategoryId fetched from db. It can be -1 (multiple mappings), 0 (all mappings), -2 (no mappings) or single storeCategoryId
            int storeTypeId = 0; //This is the storeTypeId fetched from db. It can be -1 (multiple mappings), 0 (all mappings) or single storeTypeId [Note that -2 is not present here]

            //string category = "";
            //string type = "";

            //Fetch the store details
            string fetchStoreDetails = @"SELECT ID,
                                               NAME,
                                               CODE,
                                               INVOICE_PREFIX,
                                               STORE_CATEGORY_ID,
                                               STORE_TYPE_ID,
                                               EMAIL,
                                               MOBILE,
                                               LOCATION,
                                               CITY,
                                               I_STATE
                                        FROM STORES 
                                         WHERE ACTIVE = 1
                                           AND DELETED = 0";

            //Fetch store category
            string fetchStoreCategories = @"SELECT DISTINCT(SC.ID),
                                                    SC.NAME,
                                                   SC.CODE
                                            FROM STORE_TO_STORE_CATEGORIES SSC
                                            INNER JOIN STORE_CATEGORIES SC ON SC.ID = SSC.STORE_CATEGORY_ID";

            //Fetching the store types
            string fetchStoreTypes = @"SELECT DISTINCT(ST.ID),ST.NAME, ST.CODE
                                        FROM STORE_TO_STORE_TYPES SST
                                        INNER JOIN STORE_TYPES ST ON ST.ID = SST.STORE_TYPE_ID
                                        ";

            //Fetch single store type
            string fetchSingleStoreCategory = @"SELECT ID,
                                                        NAME,
                                                       CODE
                                                FROM STORE_CATEGORIES
                                                WHERE ID = @ID 
                                                 AND ACTIVE = 1
                                                 AND DELETED = 0";

            //Fetching the store categories
            string fetchSingleStoreType = @"SELECT ID, NAME, CODE
                                            FROM STORE_TYPES
                                            WHERE ID = @ID
                                             AND ACTIVE = 1
                                             AND DELETED = 0";

            if (storeRequest.StoreId != 0)
            {
                fetchStoreDetails += " AND ID = @ID"; //If all store details need to be displayed
                fetchStoreCategories += " WHERE SSC.STORE_ID = @STORE_ID";
                fetchStoreTypes += " WHERE SST.STORE_ID = @STORE_ID";
            }
            
            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();

                //Models.Stores.FetchStores.Response.StoreResponse storeResponse = new Models.Stores.FetchStores.Response.StoreResponse();

                using (SqlCommand cmd = new SqlCommand(fetchStoreDetails, con))
                {
                    if (storeRequest.StoreId != 0)
                    {
                        cmd.Parameters.AddWithValue("@ID", storeRequest.StoreId);
                        Logger.Write(logIdentifier, "Fetching Store details", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);//Soumee - 28-March-2018 - Adding Loggers
                    }

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Models.Stores.FetchStores.Response.StoreResponse storeResponseForReader = new Models.Stores.FetchStores.Response.StoreResponse();

                            storeResponseForReader.StoreId = Convert.ToInt32(dr["ID"]);
                            storeResponseForReader.StoreName = Convert.ToString(dr["NAME"]);
                            storeResponseForReader.StoreCode = Convert.ToString(dr["CODE"]);
                            storeResponseForReader.StoreInvoicePrefix = Convert.ToString(dr["INVOICE_PREFIX"]);
                            storeCategoryId = Convert.ToInt32(dr["STORE_CATEGORY_ID"]);
                            storeTypeId = Convert.ToInt32(dr["STORE_TYPE_ID"]);
                            storeResponseForReader.StoreEmail = Convert.ToString(dr["EMAIL"]);
                            storeResponseForReader.StoreMobile = Convert.ToString(dr["MOBILE"]);
                            storeResponseForReader.StoreLocation = Convert.ToString(dr["LOCATION"]);
                            storeResponseForReader.StoreCity = Convert.ToString(dr["CITY"]);
                            storeResponseForReader.StoreState = Convert.ToString(dr["I_STATE"]);

                            storeResponseListForReader.Add(storeResponseForReader);
                            Logger.Write(logIdentifier, "Fetching Store details inside the reader", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);//Soumee - 28-March-2018 - Adding Loggers
                        }
                    }

                    //storeResponseList = storeResponseListForReader;
                }

                foreach (Models.Stores.FetchStores.Response.StoreResponse storeRes in storeResponseListForReader)
                {
                    Models.Stores.FetchStores.Response.StoreResponse storeResponse = new Models.Stores.FetchStores.Response.StoreResponse();
                    List<Models.Stores.FetchStores.Core.StoreCategoryDetails> storeCategoryDetailsList = new List<Models.Stores.FetchStores.Core.StoreCategoryDetails>();
                    List<Models.Stores.FetchStores.Core.StoreTypeDetails> storeTypeDetailsList = new List<Models.Stores.FetchStores.Core.StoreTypeDetails>();

                    //Fetching store category details
                    if (storeCategoryId != -1 && storeCategoryId != 0 && storeCategoryId != -2)
                    {
                        using (SqlCommand cmd = new SqlCommand(fetchSingleStoreCategory, con))
                        {
                            cmd.Parameters.AddWithValue("@ID", storeCategoryId);

                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                if (dr.Read())
                                {
                                    Models.Stores.FetchStores.Core.StoreCategoryDetails storeCategoryDetails = new Models.Stores.FetchStores.Core.StoreCategoryDetails();

                                    storeCategoryDetails.StoreCategoryId = Convert.ToInt32(dr["ID"]);
                                    storeCategoryDetails.StoreCategoryName = Convert.ToString(dr["NAME"]);
                                    storeCategoryDetails.StoreCategoryCode = Convert.ToString(dr["CODE"]);

                                    storeCategoryDetailsList.Add(storeCategoryDetails);
                                    //storeResponse.StoreCategory.Add(category);
                                    Logger.Write(logIdentifier, "Fetching single Store category name; if any", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);//Soumee - 28-March-2018 - Adding Loggers
                                }

                                //storeResponse.StoreCategory = storeCategoryDetailsList;
                            }
                        }
                    }
                    else
                    {
                        using (SqlCommand cmd = new SqlCommand(fetchStoreCategories, con))
                        {
                            if (storeRequest.StoreId != 0)
                            {
                                cmd.Parameters.AddWithValue("@STORE_ID", storeRequest.StoreId);
                            }

                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    Models.Stores.FetchStores.Core.StoreCategoryDetails storeCategoryDetails = new Models.Stores.FetchStores.Core.StoreCategoryDetails();

                                    storeCategoryDetails.StoreCategoryId = Convert.ToInt32(dr["ID"]);
                                    storeCategoryDetails.StoreCategoryName = Convert.ToString(dr["NAME"]);
                                    storeCategoryDetails.StoreCategoryCode = Convert.ToString(dr["CODE"]);

                                    storeCategoryDetailsList.Add(storeCategoryDetails);

                                    //category = Convert.ToString(dr["NAME"]);
                                    //storeResponse.StoreCategory.Add(category);
                                    //storeResponseList.Add(storeCategoryDetails);
                                    Logger.Write(logIdentifier, "Fetching Store categories name; if any", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);//Soumee - 28-March-2018 - Adding Loggers
                                }

                                //storeResponse.StoreCategory = storeCategoryDetailsList;
                            }
                        }
                    }

                    if (storeTypeId != 0 && storeTypeId != -1 && storeTypeId != -2)
                    {
                        using (SqlCommand cmd = new SqlCommand(fetchSingleStoreType, con))
                        {
                            cmd.Parameters.AddWithValue("@ID", storeTypeId);

                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                if (dr.Read())
                                {
                                    Models.Stores.FetchStores.Core.StoreTypeDetails storeTypeDetails = new Models.Stores.FetchStores.Core.StoreTypeDetails();

                                    storeTypeDetails.StoreTypeId = Convert.ToInt32(dr["ID"]);
                                    storeTypeDetails.StoreTypeName = Convert.ToString(dr["NAME"]);
                                    storeTypeDetails.StoreTypeCode = Convert.ToString(dr["CODE"]);

                                    storeTypeDetailsList.Add(storeTypeDetails);

                                    Logger.Write(logIdentifier, "Fetching single Store type name", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);//Soumee - 28-March-2018 - Adding Loggers
                                }

                                //storeResponse.StoreType = storeTypeDetailsList;
                            }
                        }
                    }
                    else
                    {
                        using (SqlCommand cmd = new SqlCommand(fetchStoreTypes, con))
                        {
                            if (storeRequest.StoreId != 0)
                            {
                                cmd.Parameters.AddWithValue("@STORE_ID", storeRequest.StoreId);
                            }

                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    Models.Stores.FetchStores.Core.StoreTypeDetails storeTypeDetails = new Models.Stores.FetchStores.Core.StoreTypeDetails();

                                    storeTypeDetails.StoreTypeId = Convert.ToInt32(dr["ID"]);
                                    storeTypeDetails.StoreTypeName = Convert.ToString(dr["NAME"]);
                                    storeTypeDetails.StoreTypeCode = Convert.ToString(dr["CODE"]);

                                    storeTypeDetailsList.Add(storeTypeDetails);

                                    Logger.Write(logIdentifier, "Fetching Store types name", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);//Soumee - 28-March-2018 - Adding Loggers
                                }

                                //storeResponse.StoreType = storeTypeDetailsList;
                            }
                        }
                    }
                    storeResponse.StoreId = storeRes.StoreId;
                    storeResponse.StoreCategory = storeCategoryDetailsList;
                    storeResponse.StoreType = storeTypeDetailsList;
                    storeResponse.StoreName = storeRes.StoreName;
                    storeResponse.StoreMobile = storeRes.StoreMobile;
                    storeResponse.StoreLocation = storeRes.StoreLocation;
                    storeResponse.StoreInvoicePrefix = storeRes.StoreInvoicePrefix;
                    storeResponse.StoreCode = storeRes.StoreCode;
                    storeResponse.StoreCity = storeRes.StoreCity;
                    storeResponse.StoreEmail = storeRes.StoreEmail;
                    storeResponse.StoreState = storeRes.StoreState;

                    storeResponseList.Add(storeResponse);
                }
                //storeResponseList.Add(storeResponse);
                
            }

            if (storeResponseList.Count != 0)
            {
                storeResponseListResponse.IsOk = true;
                storeResponseListResponse.Message = "Store details are";
                storeResponseListResponse.ResponseObject = storeResponseList;

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            else
            {
                storeResponseListResponse.Message = "Error fetching the store list. Please try again";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            return Ok(storeResponseListResponse);
        }

        /// <summary>
        /// This api is used to create store categories
        /// </summary>
        /// Created By: Soumee
        /// Date: 21th March, 2018
        /// Issue No.: td-964
        /// Issue Description: Inhouse | API | Inventory | Fetch store details
        /// Input JSON:
        /// {
        /// "StoreCategoryName":"Renewable",
        /// "StoreCategoryCode":"RNW",
        /// }
        /// <param name="storeCategoryInput">StoreCategoryName, StoreCategoryCode</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("CreateStoreCategory")]
        public IHttpActionResult CreateStoreCategory(Models.Stores.CreateStoreCategory.Request.StoreCategoryInput storeCategoryInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            // +Soumee - 28 - March - 2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Stores.CreateStoreCategory";
            Logger.Write(logIdentifier, storeCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_CATEGORY);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_CATEGORY);
            //-Soumee - 28-March-2018 - Adding Loggers

            Models.Stores.CreateStoreCategory.Response.StoreCategoryOutput storeCategoryOutput = new Models.Stores.CreateStoreCategory.Response.StoreCategoryOutput();
            Response<Models.Stores.CreateStoreCategory.Response.StoreCategoryOutput> storeCategoryOutputResponse = new Response<Models.Stores.CreateStoreCategory.Response.StoreCategoryOutput>();

            //+Soumee - 28-March-2018 - Checking for duplicate codes
            Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput duplicateCodeCheckInput = new Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput();

            duplicateCodeCheckInput.Code = storeCategoryInput.StoreCategoryCode;
            duplicateCodeCheckInput.TableName = "Store_Categories";

            Logger.Write(logIdentifier, "Checking for code Duplicacy", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);//Soumee - 28-March-2018 - Adding Loggers
            IMS.Controllers.Services.IStoresServices storesServices = new StoresService();
            storeCategoryOutputResponse.IsOk = storesServices.CheckForDuplicateCode(duplicateCodeCheckInput).IsOk;

            if (storeCategoryOutputResponse.IsOk == false)
            {
                storeCategoryOutputResponse.Message = "This Code already exists. Please try a different code.";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_CATEGORY);
                //-Soumee - 28-March-2018 - Adding Loggers

                return Ok(storeCategoryOutputResponse);
            }
            //-Soumee - 28-March-2018 - Checking for duplicate codes

            //Query to create new store category
            string createStoreCategory = @"INSERT INTO STORE_CATEGORIES
                                                        VALUES(@NAME,
                                                               @CODE,
                                                               @ACTIVE,
                                                               @DELETED,
                                                               @CREATED_TIME,
                                                               @MODIFIED_TIME,
                                                               @CREATED_BY,
                                                               @MODIFIED_BY);
                                                    SELECT SCOPE_IDENTITY()";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(createStoreCategory, con))
                {
                    cmd.Parameters.AddWithValue("@NAME", storeCategoryInput.StoreCategoryName);
                    cmd.Parameters.AddWithValue("@CODE", storeCategoryInput.StoreCategoryCode);
                    cmd.Parameters.AddWithValue("@ACTIVE", 1);
                    cmd.Parameters.AddWithValue("@DELETED", 0);
                    cmd.Parameters.AddWithValue("@CREATED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CREATED_BY", 0);
                    cmd.Parameters.AddWithValue("@MODIFIED_BY", 0);

                    storeCategoryOutput.StoreCategoryId = int.Parse(cmd.ExecuteScalar().ToString());
                    Logger.Write(logIdentifier, "Creating new store category", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);//Soumee - 28-March-2018 - Adding Loggers
                }
            }

            if (storeCategoryOutput.StoreCategoryId != 0)
            {
                storeCategoryOutputResponse.IsOk = true;
                storeCategoryOutputResponse.Message = "Store Category created successfully.";
                storeCategoryOutputResponse.ResponseObject = storeCategoryOutput;

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_CATEGORY);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            else
            {
                storeCategoryOutputResponse.IsOk = false;
                storeCategoryOutputResponse.Message = "OOPS! Unable to create the store category.";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_CATEGORY);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            return Ok(storeCategoryOutputResponse);
        }

        /// <summary>
        /// This api is used to edit the store category details
        /// </summary>
        /// Created By: Soumee
        /// Date: 21th March, 2018
        /// Issue No.: td-964
        /// Issue Description: Inhouse | API | Inventory | Fetch store details
        /// Input JSON:
        /// {
        /// "StoreCategoryId":3,
        /// "StoreCategoryName":"Small",
        /// "StoreCategoryCode":"NRNW",
        /// }
        /// <param name="editStoreCategoryInput">StoreCategoryId, StoreCategoryName, StoreCategoryCode</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("EditStoreCategory")]
        public IHttpActionResult EditStoreCategory(Models.Stores.EditStoreCategory.Request.EditStoreCategoryInput editStoreCategoryInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 28-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Stores.EditStoreCategory";
            Logger.Write(logIdentifier, editStoreCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_CATEGORY);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_CATEGORY);
            //-Soumee - 28-March-2018 - Adding Loggers

            Response response = new Response();

            int rowsAffected = 0;

            //+Soumee - 29-March-2018 - Check for code Duplicacy
            Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput duplicateCodeCheckInput = new Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput();

            duplicateCodeCheckInput.Code = editStoreCategoryInput.StoreCategoryCode;
            duplicateCodeCheckInput.TableName = "Store_Categories";

            IMS.Controllers.Services.IStoresServices storesServices = new StoresService();
            response.IsOk = storesServices.CheckForDuplicateCode(duplicateCodeCheckInput).IsOk;

            if (response.IsOk == false)
            {
                response.Message = "This Code already exists. Please try a different code.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, editStoreCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_CATEGORY);
                //-Soumee - 29-March-2018 - Adding Loggers

                return Ok(response);
            }
            //-Soumee - 29-March-2018 - Check for code Duplicacy

            //Query to edit the store category name or code
            string editStoreCategory = @"UPDATE STORE_CATEGORIES
                                                SET NAME = @NAME,
                                                    CODE = @CODE,
                                                    MODIFIED_TIME = @MODIFIED_TIME
                                                WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(editStoreCategory, con))
                {
                    cmd.Parameters.AddWithValue("@NAME", editStoreCategoryInput.StoreCategoryName);
                    cmd.Parameters.AddWithValue("@CODE", editStoreCategoryInput.StoreCategoryCode);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ID", editStoreCategoryInput.StoreCategoryId);

                    rowsAffected = (int)cmd.ExecuteNonQuery();
                    Logger.Write(logIdentifier, "Creating new store category", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_CATEGORY);//Soumee - 28-March-2018 - Adding Loggers
                }
            }
            if (rowsAffected > 0)
            {
                response.IsOk = true;
                response.Message = "Store Category information updated successfully!";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, editStoreCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_CATEGORY);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            else
            {
                response.Message = "Unable to update the store caetgories. Let's try again";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, editStoreCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_CATEGORY);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            return Ok(response);
        }

        /// <summary>
        /// This api is used to delete the store category
        /// </summary>
        /// Created By: Soumee
        /// Date: 21th March, 2018
        /// Issue No.: td-964
        /// Issue Description: Inhouse | API | Inventory | Fetch store details
        /// Input JSON:
        /// {
        /// "StoreCategoryId":3,
        /// }
        /// <param name="deleteStoreCategoryInput">StoreCategoryId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("DeleteStoreCategory")]
        public IHttpActionResult DeleteStoreCategory(Models.Stores.DeleteStoreCategory.Request.DeleteStoreCategoryInput deleteStoreCategoryInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 28-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Stores.DeleteStoreCategory";
            Logger.Write(logIdentifier, deleteStoreCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE_CATEGORY);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE_CATEGORY);
            //-Soumee - 28-March-2018 - Adding Loggers

            Response response = new Response();

            int rowsAffected = 0;
            string deleteStoreCategory = @"UPDATE STORE_CATEGORIES
                                                    SET ACTIVE = 0,
                                                        DELETED = 1,
                                                        MODIFIED_TIME = GETDATE()
                                                    WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(deleteStoreCategory, con))
                {
                    cmd.Parameters.AddWithValue("@ID", deleteStoreCategoryInput.StoreCategoryId);

                    rowsAffected = (int)cmd.ExecuteNonQuery();
                    Logger.Write(logIdentifier, "Deleting store category", DateTime.Now.Ticks, LoggerConstants.STORES__FETCH_STORES);//Soumee - 28-March-2018 - Adding Loggers
                }
            }
            if (rowsAffected > 0)
            {
                response.IsOk = true;
                response.Message = "Deleted the Store Category.";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, deleteStoreCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE_CATEGORY);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            else
            {
                response.Message = "Unable to delete the store category";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, deleteStoreCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE_CATEGORY);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            return Ok(response);
        }

        /// <summary>
        /// This api is used to create new store types
        /// </summary>
        /// Created By: Soumee
        /// Date: 21th March, 2018
        /// Issue No.: td-964
        /// Issue Description: Inhouse | API | Inventory | Fetch store details
        /// Input JSON:
        /// {
        /// "StoreTypeName":"Bakery",
        /// "StoreTypeCode":"BKR",
        /// }
        /// <param name="createStoreTypeInput"></param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("CreateStoreType")]
        public IHttpActionResult CreateStoreType(Models.Stores.CreateStoreType.Request.CreateStoreTypeInput createStoreTypeInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 28-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Stores.CreateStoreType";
            Logger.Write(logIdentifier, createStoreTypeInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_TYPE);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_TYPE);
            //-Soumee - 28-March-2018 - Adding Loggers

            //+Soumee - 28-March-2018 - Checking code duplicacy
            Models.Stores.CreateStoreType.Response.CreateStoreTypeOutput createStoreTypeOutput = new Models.Stores.CreateStoreType.Response.CreateStoreTypeOutput();
            Response<Models.Stores.CreateStoreType.Response.CreateStoreTypeOutput> createstoreTypeOutputResponse = new Response<Models.Stores.CreateStoreType.Response.CreateStoreTypeOutput>();

            Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput duplicateCodeCheckInput = new Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput();

            duplicateCodeCheckInput.Code = createStoreTypeInput.StoreTypeCode;
            duplicateCodeCheckInput.TableName = "Store_Types";

            Logger.Write(logIdentifier, "Checking code duplicacy", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_TYPE);//Soumee - 28-March-2018 - Adding Loggers
            IMS.Controllers.Services.IStoresServices storesServices = new StoresService();
            createstoreTypeOutputResponse.IsOk = storesServices.CheckForDuplicateCode(duplicateCodeCheckInput).IsOk;

            if (createstoreTypeOutputResponse.IsOk == false)
            {
                createstoreTypeOutputResponse.Message = "This Code already exists. Please try a different code.";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, createStoreTypeInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_TYPE);
                //-Soumee - 28-March-2018 - Adding Loggers

                return Ok(createstoreTypeOutputResponse);
            }
            //-Soumee - 28-March-2018 - Checking code duplicacy

            //Query to create store type
            string createStoretype = @"INSERT INTO STORE_TYPES
                                                VALUES(@NAME,
                                                        @CODE,
                                                        @ACTIVE,
                                                        @DELETED,
                                                        @CREATED_TIME,
                                                        @MODIFIED_TIME,
                                                        @CREATED_BY,
                                                        @MODIFIED_BY);
                                                SELECT SCOPE_IDENTITY()";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(createStoretype, con))
                {
                    cmd.Parameters.AddWithValue("@NAME", createStoreTypeInput.StoreTypeName);
                    cmd.Parameters.AddWithValue("@CODE", createStoreTypeInput.StoreTypeCode);
                    cmd.Parameters.AddWithValue("@ACTIVE", 1);
                    cmd.Parameters.AddWithValue("@DELETED", 0);
                    cmd.Parameters.AddWithValue("@CREATED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CREATED_BY", 0);
                    cmd.Parameters.AddWithValue("@MODIFIED_BY", 0);

                    createStoreTypeOutput.StoreTypeId = int.Parse(cmd.ExecuteScalar().ToString());
                    Logger.Write(logIdentifier, "Creating new store type", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_TYPE);//Soumee - 28-March-2018 - Adding Loggers
                }
            }

            if (createStoreTypeOutput.StoreTypeId != 0)
            {
                createstoreTypeOutputResponse.IsOk = true;
                createstoreTypeOutputResponse.Message = "New Store type created.";
                createstoreTypeOutputResponse.ResponseObject = createStoreTypeOutput;

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, createStoreTypeInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_TYPE);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            else
            {
                createstoreTypeOutputResponse.IsOk = false;
                createstoreTypeOutputResponse.Message = "Unable to create new store type. Let's try again.";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, createStoreTypeInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__CREATE_STORE_TYPE);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            return Ok(createstoreTypeOutputResponse);
        }

        /// <summary>
        /// This api is used to edit the store type details
        /// </summary>
        /// Created By: Soumee
        /// Date: 21th March, 2018
        /// Issue No.: td-964
        /// Issue Description: Inhouse | API | Inventory | Fetch store details
        /// Input JSON:
        /// {
        /// "StoreTypeId":1,
        /// "StoreTypeName":"Cafeteria",
        /// "StoreTypeCode":"CFTR",
        /// }
        /// <param name="editStoreTypeRequest">StoreTypeId, StoreTypeName, StoreTypeCode</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("EditStoreType")]
        public IHttpActionResult EditStoreType(Models.Stores.EditStoreType.Request.EditStoreTypeRequest editStoreTypeRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 28-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Stores.EditStoreType";
            Logger.Write(logIdentifier, editStoreTypeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_TYPE);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_TYPE);
            //-Soumee - 28-March-2018 - Adding Loggers

            Response response = new Response();

            int rowsAffected = 0;

            //+Soumee - 29-March-2018 - Check for code Duplicacy
            Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput duplicateCodeCheckInput = new Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput();

            duplicateCodeCheckInput.Code = editStoreTypeRequest.StoreTypeCode;
            duplicateCodeCheckInput.TableName = "Store_Types";

            IMS.Controllers.Services.IStoresServices storesServices = new StoresService();
            response.IsOk = storesServices.CheckForDuplicateCode(duplicateCodeCheckInput).IsOk;

            if (response.IsOk == false)
            {
                response.Message = "This Code already exists. Please try a different code.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, editStoreTypeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_TYPE);
                //-Soumee - 29-March-2018 - Adding Loggers

                return Ok(response);
            }
            //-Soumee - 29-March-2018 - Check for code Duplicacy

            //Query to edit store type name or code
            string editstoreTypeDetails = @"UPDATE STORE_TYPES
                                            SET NAME = @NAME,
                                                CODE = @CODE,
                                                MODIFIED_TIME = @MODIFIED_TIME
                                            WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(editstoreTypeDetails, con))
                {
                    cmd.Parameters.AddWithValue("@NAME", editStoreTypeRequest.StoreTypeName);
                    cmd.Parameters.AddWithValue("@CODE", editStoreTypeRequest.StoreTypeCode);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ID", editStoreTypeRequest.StoreTypeId);

                    rowsAffected = (int)cmd.ExecuteNonQuery();
                    Logger.Write(logIdentifier, "Editing the store type details", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_TYPE);//Soumee - 28-March-2018 - Adding Loggers
                }
            }
            if (rowsAffected > 0)
            {
                response.IsOk = true;
                response.Message = "Store Type details updated successfully.";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, editStoreTypeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_TYPE);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            else
            {
                response.Message = "Unable to update the store type";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, editStoreTypeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE_TYPE);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            return Ok(response);
        }

        /// <summary>
        /// This api is used to delet the store type
        /// </summary>
        /// Created By: Soumee
        /// Date: 21th March, 2018
        /// Issue No.: td-964
        /// Issue Description: Inhouse | API | Inventory | Fetch store details
        /// Input JSON:
        /// {
        /// "StoreTypeId":1,
        /// }
        /// <param name="deleteStoreTypeInput">StoreTypeId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("DeleteStoreType")]
        public IHttpActionResult DeleteStoreType(Models.Stores.DeleteStoreType.Request.DeleteStoreTypeInput deleteStoreTypeInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 28-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Stores.DeleteStoreType";
            Logger.Write(logIdentifier, deleteStoreTypeInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE_TYPE);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE_TYPE);
            //-Soumee - 28-March-2018 - Adding Loggers

            Response response = new Response();

            int rowsAffected = 0;
            string deleteStoreType = @"UPDATE STORE_TYPES
                                            SET ACTIVE = 0,
                                                DELETED = 1,
                                                MODIFIED_TIME = GETDATE()
                                            WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(deleteStoreType, con))
                {
                    cmd.Parameters.AddWithValue("@ID", deleteStoreTypeInput.StoreTypeId);

                    rowsAffected = (int)cmd.ExecuteNonQuery();
                    Logger.Write(logIdentifier, "deleting store type", DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE_TYPE);//Soumee - 28-March-2018 - Adding Loggers
                }
            }

            if (rowsAffected > 0)
            {
                response.IsOk = true;
                response.Message = "The store type has been deleted.";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, deleteStoreTypeInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE_TYPE);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            else
            {
                response.Message = "Unable to delete the Store type.";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, deleteStoreTypeInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE_TYPE);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            return Ok(response);
        }

        /// <summary>
        /// This api is used to delete the stores
        /// </summary>
        /// Created By: Soumee
        /// Date: 21th March, 2018
        /// Issue No.: td-964
        /// Issue Description: Inhouse | API | Inventory | Fetch store details
        /// Input JSON:
        /// {
        /// "StoreId":1,
        /// }
        /// <param name="deleteStoreInput">StoreId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("DeleteStore")]
        public IHttpActionResult DeleteStore(Models.Stores.DeleteStore.Request.DeleteStoreInput deleteStoreInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 28-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Stores.DeleteStore";
            Logger.Write(logIdentifier, deleteStoreInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE);
            //-Soumee - 28-March-2018 - Adding Loggers

            Response response = new Response();

            int rowsAffected = 0;

            //Query to inactivate the store
            string deleteStore = @"UPDATE STORES
                                        SET ACTIVE = 0,
                                            DELETED = 1,
                                            MODIFIED_TIME = GETDATE()
                                        WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(deleteStore, con))
                {
                    cmd.Parameters.AddWithValue("@ID", deleteStoreInput.StoreId);

                    rowsAffected = (int)cmd.ExecuteNonQuery();
                    Logger.Write(logIdentifier, "Deleting stores", DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE);//Soumee - 28-March-2018 - Adding Loggers
                }
            }
            if(rowsAffected > 0)
            {
                response.IsOk = true;
                response.Message = "Deleted the store";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, deleteStoreInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            else
            {
                response.Message = "Unable to delete the store.";

                //+Soumee - 28-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, deleteStoreInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__DELETE_STORE);
                //-Soumee - 28-March-2018 - Adding Loggers
            }
            return Ok(response);
        }       
    }
}

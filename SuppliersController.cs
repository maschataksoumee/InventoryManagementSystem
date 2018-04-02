using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using IMS.Helpers;
using System.Data;
using System.Data.SqlClient;
using IMS.Controllers.Implementation;
using IMS.Helpers.Constants;
using IMS.Helpers.Logger;

namespace IMS.Controllers
{
    [RoutePrefix("api/Suppliers")]
    public class SuppliersController : ApiController
    {
        /// <summary>
        /// This api is used to create Supplier Types
        /// </summary>
        /// Created By: Soumee
        /// Date: 16th March, 2018
        /// Issue No.: td-977
        /// Issue Description: Inhouse | Api | Inventory | Create Supplier Type
        /// Input Json:
        /// {
        /// "SupplierTypeName":"Precious Gifts",
        /// "SupplierTypeCode":"PCG"
        /// }
        /// <param name="supplierTypeRequest">SupplierTypeName, SupplierTypeCode</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("CreateSupplierType")]
        public IHttpActionResult CreateSupplierType(Models.Suppliers.CreateSupplierType.Request.SupplierTypeRequest supplierTypeRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Suppliers.CreateSupplierType";
            Logger.Write(logIdentifier, supplierTypeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIER_TYPE);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIER_TYPE);
            //-Soumee - 29-March-2018 - Adding Loggers

            //+Soumee - 29-March-2018 - Checking code duplicacy
            Models.Suppliers.CreateSupplierType.Response.SupplierTypeResponse supplierType = new Models.Suppliers.CreateSupplierType.Response.SupplierTypeResponse();
            Response<Models.Suppliers.CreateSupplierType.Response.SupplierTypeResponse> supplierTypeResponse = new Response<Models.Suppliers.CreateSupplierType.Response.SupplierTypeResponse>();

            Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput duplicateCodeCheckInput = new Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput();

            duplicateCodeCheckInput.Code = supplierTypeRequest.SupplierTypeCode;
            duplicateCodeCheckInput.TableName = "Supplier_Types";

            IMS.Controllers.Services.ISuppliersServices suppliersServices = new SuppliersService();
            supplierTypeResponse.IsOk = suppliersServices.CheckForDuplicateCode(duplicateCodeCheckInput).IsOk;

            if (supplierTypeResponse.IsOk == false)
            {
                supplierTypeResponse.Message = "This Code already exists. Please try a different code.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, supplierTypeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIER_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIER_TYPE);
                //-Soumee - 29-March-2018 - Adding Loggers

                return Ok(supplierTypeResponse);
            }
            //-Soumee - 29-March-2018 - Checking code duplicacy

            //Query to creat new supplier type
            string createSuppliertype = @"INSERT INTO SUPPLIER_TYPES
                                                        VALUES(@NAME,
                                                               @CODE,
                                                               @ACTIVE,
                                                               @DELETED,
                                                               @CREATED_TIME,
                                                               @MODIFIED_TIME,
                                                               @CREATED,
                                                               @MODIFIED);
                                                SELECT SCOPE_IDENTITY();";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(createSuppliertype, con))
                {
                    cmd.Parameters.AddWithValue("@NAME", supplierTypeRequest.SupplierTypeName);
                    cmd.Parameters.AddWithValue("@CODE", supplierTypeRequest.SupplierTypeCode);
                    cmd.Parameters.AddWithValue("@ACTIVE", 1);
                    cmd.Parameters.AddWithValue("@DELETED", 0);
                    cmd.Parameters.AddWithValue("@CREATED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CREATED", 0);
                    cmd.Parameters.AddWithValue("@MODIFIED", 0);

                    supplierType.SupplierTypeId = int.Parse(cmd.ExecuteScalar().ToString());

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "Creating new supplier type", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIER_TYPE);
                }
            }

            if(supplierType.SupplierTypeId != 0)
            {
                supplierTypeResponse.IsOk = true;
                supplierTypeResponse.Message = "New Supplier Type Created";
                supplierTypeResponse.ResponseObject = supplierType;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, supplierTypeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIER_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIER_TYPE);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                supplierTypeResponse.IsOk = false;
                supplierTypeResponse.Message = "Unable to create the supplier type. Let's try again.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, supplierTypeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIER_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIER_TYPE);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(supplierTypeResponse);
        }

        /// <summary>
        /// This api is used to fetch the Supplier Types. If SupplierTypeId = 0, all supplier types are fetched
        /// </summary>
        /// Created By: Soumee
        /// Date: 16th March, 2018
        /// Issue No.: td-978
        /// Issue Description: Inhouse | Api | Inventory | Fetch Supplier Type
        /// Input Json:
        /// {
        /// "SupplierTypeId":1
        /// }
        /// <param name="supplierTypeRequest">SupplierTypeId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("FetchSupplierType")]
        public IHttpActionResult FetchSupplierType(Models.Suppliers.FetchSupplierType.Request.SupplierTypeInput supplierTypeRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Suppliers.FetchSupplierType";
            Logger.Write(logIdentifier, supplierTypeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__FETCH_SUPPLIER_TYPE);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__FETCH_SUPPLIER_TYPE);
            //-Soumee - 29-March-2018 - Adding Loggers

            List<Models.Suppliers.FetchSupplierType.Response.SupplierTypeResponse> supplierTypeResponseList = new List<Models.Suppliers.FetchSupplierType.Response.SupplierTypeResponse>();
            Response<List<Models.Suppliers.FetchSupplierType.Response.SupplierTypeResponse>> supplierTypeResponseListResponse = new Response<List<Models.Suppliers.FetchSupplierType.Response.SupplierTypeResponse>>();

            //Query to fetch all supplier types when id = 0
            string fetchSuppliertype = @"SELECT ID,
                                               NAME,
                                               CODE
                                        FROM SUPPLIER_TYPES
                                        WHERE ACTIVE = 1
                                          AND DELETED = 0";

            //If SupplierTypeId is a ceratin id
            if (supplierTypeRequest.SupplierTypeId != 0)
            {
                fetchSuppliertype += " AND ID = @ID";
            }

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(fetchSuppliertype, con))
                {
                    if(supplierTypeRequest.SupplierTypeId != 0)
                    {
                        cmd.Parameters.AddWithValue("@ID", supplierTypeRequest.SupplierTypeId);
                    }

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while(dr.Read())
                        {
                            Models.Suppliers.FetchSupplierType.Response.SupplierTypeResponse suppliertypeResponse = new Models.Suppliers.FetchSupplierType.Response.SupplierTypeResponse();

                            suppliertypeResponse.SupplierTypeId = Convert.ToInt32(dr["ID"]);
                            suppliertypeResponse.SupplierTypeName = Convert.ToString(dr["NAME"]);
                            suppliertypeResponse.SupplierTypeCode = Convert.ToString(dr["CODE"]);

                            supplierTypeResponseList.Add(suppliertypeResponse);

                            //Soumee - 29-March-2018 - Adding Loggers        
                            Logger.Write(logIdentifier, "Fetching the supplier type inside the reader", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__FETCH_SUPPLIER_TYPE);
                        }                       
                    }
                }
            }

            if(supplierTypeResponseList.Count != 0)
            {
                supplierTypeResponseListResponse.IsOk = true;
                supplierTypeResponseListResponse.Message = "Supplier Types are";
                supplierTypeResponseListResponse.ResponseObject = supplierTypeResponseList;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, supplierTypeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__FETCH_SUPPLIER_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__FETCH_SUPPLIER_TYPE);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                supplierTypeResponseListResponse.Message = "Sorry! we don't have any Supplier Types now. Please insert some";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, supplierTypeRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__FETCH_SUPPLIER_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__FETCH_SUPPLIER_TYPE);
                //-Soumee - 29-March-2018 - Adding Loggers
            }

            return Ok(supplierTypeResponseListResponse);
        }

        /// <summary>
        /// This api is used to create new suppliers
        /// </summary>
        /// Created By: Soumee
        /// Date: 16th March, 2018
        /// Issue No.: td-979
        /// Issue Description: Inhouse | Api | Inventory | Create Suppliers
        /// Input Json:
        /// {
        /// "SupplierName":"I am Unique",
        /// "SupplierMobile":"5874123698",
        /// "SupplierLocation":"Kolkata",
        /// "SupplierTINNumber":"548BAIT",
        /// "SupplierRegion":"West Bengal",
        /// "SupplierHelpDesk":"This is Help Desk",
        /// "SupplierTypeId":1
        /// }
        /// <param name="suppliersRequest">SupplierName, SupplierMobile, SupplierLocation, SupplierTINNumber, SupplierRegion, SupplierHelpDesk, SupplierTypeId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("CreateSuppliers")]
        public IHttpActionResult CreateSuppliers(Models.Suppliers.CreateSuppliers.Request.SuppliersRequest suppliersRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Suppliers.CreateSuppliers";
            Logger.Write(logIdentifier, suppliersRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIERS);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIERS);
            //-Soumee - 29-March-2018 - Adding Loggers

            Response<Models.Suppliers.CreateSuppliers.Response.SupplierResponse> supplierResponse = new Response<Models.Suppliers.CreateSuppliers.Response.SupplierResponse>();
            Models.Suppliers.CreateSuppliers.Response.SupplierResponse supplier = new Models.Suppliers.CreateSuppliers.Response.SupplierResponse();


            string createSupplier = @"INSERT INTO Supplier_Details
                                                VALUES(@NAME,
                                                       @CONTACT_NO,
                                                       @LOCATION,
                                                       @SUPPLIER_TYPE_ID,
                                                       @TIN_NUMBER,
                                                       @REGION,
                                                       @HELP_DESK,
                                                       @ACTIVE,
                                                       @DELETED,
                                                       @CREATED_TIME,
                                                       @MODIFIED_TIME,
                                                       @CREATED_BY,
                                                       @MODIFIED_BY);
                                            SELECT SCOPE_IDENTITY();";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(createSupplier, con))
                {
                    cmd.Parameters.AddWithValue("@NAME", suppliersRequest.SupplierName);
                    cmd.Parameters.AddWithValue("@CONTACT_NO", suppliersRequest.SupplierMobile);
                    cmd.Parameters.AddWithValue("@LOCATION", suppliersRequest.SupplierLocation);
                    cmd.Parameters.AddWithValue("@SUPPLIER_TYPE_ID", suppliersRequest.SupplierTypeId);
                    cmd.Parameters.AddWithValue("@TIN_NUMBER", suppliersRequest.SupplierTINNumber);
                    cmd.Parameters.AddWithValue("@REGION", suppliersRequest.SupplierRegion);
                    cmd.Parameters.AddWithValue("@HELP_DESK", suppliersRequest.SupplierHelpDesk);
                    cmd.Parameters.AddWithValue("@ACTIVE", 1);
                    cmd.Parameters.AddWithValue("@DELETED", 0);
                    cmd.Parameters.AddWithValue("@CREATED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CREATED_BY", 0);
                    cmd.Parameters.AddWithValue("@MODIFIED_BY", 0);

                    supplier.SupplierId = int.Parse(cmd.ExecuteScalar().ToString());//New SupplierId generated

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "New SupplierId generated", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIERS);
                }
            }

            if(supplier.SupplierId != 0)
            {
                supplierResponse.IsOk = true;
                supplierResponse.Message = "The Supplier has been created successfully.";
                supplierResponse.ResponseObject = supplier;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, suppliersRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIERS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIERS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                supplierResponse.Message = "Oops! There seems to be an error. Let's try again.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, suppliersRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIERS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__CREATE_SUPPLIERS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(supplierResponse);
        }

        /// <summary>
        /// This api is used to edit supplier details
        /// </summary>
        /// Created By: Soumee
        /// Date: 20th March, 2018
        /// Issue No.: td-980
        /// Issue Description: Inhouse | Api | Inventory | Edit Suppliers
        /// Input Json:
        /// {
        /// "SupplierId":2,
        /// "SupplierName":"Genie House",
        /// "SupplierMobile":"5874123698",
        /// "SupplierLocation":"Kolkata",
        /// "SupplierTINNumber":"5GTHR",
        /// "SupplierRegion":"West Bengal",
        /// "SupplierHelpDesk":"This is Help Desk",
        /// "SupplierTypeId":1
        /// }
        /// <param name="supplierRequest">SupplierId, SupplierName, SupplierMobile, SupplierLocation, SupplierTINNumber, SupplierRegion, SupplierHelpDesk, SupplierTypeId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("EditSuppliers")]
        public IHttpActionResult EditSuppliers(Models.Suppliers.EditSuppliers.Request.SupplierRequest supplierRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Suppliers.EditSuppliers";
            Logger.Write(logIdentifier, supplierRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__EDIT_SUPPLIERS);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__EDIT_SUPPLIERS);
            //-Soumee - 29-March-2018 - Adding Loggers

            Response response = new Response();

            int rowsAffected = 0;
            string editSupplierDetails = @"UPDATE Supplier_Details
                                                SET Name = @NAME,
                                                    Contact_No = @MOBILE,
                                                    LOCATION = @LOCATION,
                                                               Supplier_Type_Id = @SUPPLIERTYPEID,
                                                               TIN_Number = @TINNO,
                                                               Region = @REGION,
                                                               Help_Desk = @HELPDESK,
                                                               Modified_Time = @MODIFIEDTIME
                                                WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(editSupplierDetails, con))
                {
                    cmd.Parameters.AddWithValue("@NAME", supplierRequest.SupplierName);
                    cmd.Parameters.AddWithValue("@MOBILE", supplierRequest.SupplierMobile);
                    cmd.Parameters.AddWithValue("@LOCATION", supplierRequest.SupplierLocation);
                    cmd.Parameters.AddWithValue("@SUPPLIERTYPEID", supplierRequest.SupplierTypeId);
                    cmd.Parameters.AddWithValue("@TINNO", supplierRequest.SupplierTINNumber);
                    cmd.Parameters.AddWithValue("@REGION", supplierRequest.SupplierRegion);
                    cmd.Parameters.AddWithValue("@HELPDESK", supplierRequest.SupplierHelpDesk);
                    cmd.Parameters.AddWithValue("@MODIFIEDTIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ID", supplierRequest.SupplierId);

                    rowsAffected = (int)cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "editing supplier details", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__EDIT_SUPPLIERS);
                }
            }

            if(rowsAffected > 0)
            {
                response.IsOk = true;
                response.Message = "Supplier details has been successfully edited.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, supplierRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__EDIT_SUPPLIERS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__EDIT_SUPPLIERS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                response.Message = "There has been an error in updating the information. Please try again";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, supplierRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__EDIT_SUPPLIERS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__EDIT_SUPPLIERS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(response);
        }

        /// <summary>
        /// This api is used to fetch supplier details. If SupplierId = 0, all the suppliers are fetched
        /// </summary>
        /// Created By: Soumee
        /// Date: 20th March, 2018
        /// Issue No.: td-981
        /// Issue Description: Inhouse | Api | Inventory | Fetch Supplier Details
        /// Input Json:
        /// {
        /// "SupplierId":0,
        /// }
        /// <param name="supplierDetailsRequest">SupplierId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("FetchSupplierDetails")]
        public IHttpActionResult FetchSupplierDetails(Models.Suppliers.FetchSupplierDetails.Request.SupplierDetailsRequest supplierDetailsRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Suppliers.FetchSupplierDetails";
            Logger.Write(logIdentifier, supplierDetailsRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__FETCH_SUPPLIER_DETAILS);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__FETCH_SUPPLIER_DETAILS);
            //-Soumee - 29-March-2018 - Adding Loggers

            List<Models.Suppliers.FetchSupplierDetails.Response.SupplierDetailsResponse> supplierDetailsResponseList = new List<Models.Suppliers.FetchSupplierDetails.Response.SupplierDetailsResponse>();
            Response<List<Models.Suppliers.FetchSupplierDetails.Response.SupplierDetailsResponse>> supplierDetailsResponseListResponse = new Response<List<Models.Suppliers.FetchSupplierDetails.Response.SupplierDetailsResponse>>();

            //Query to fetch supplier details when id = 0
            string fetchSupplierDetails = @"SELECT SD.ID,
                                                   SD.NAME,
                                                   SD.CONTACT_NO,
                                                   SD.LOCATION,
                                                   SD.SUPPLIER_TYPE_ID,
                                                   SD.TIN_NUMBER,
                                                   SD.REGION,
                                                   SD.HELP_DESK,
                                                   ST.NAME AS SUPPLIER_TYPE_NAME
                                            FROM SUPPLIER_DETAILS SD
                                            INNER JOIN SUPPLIER_TYPES ST ON ST.ID = SD.SUPPLIER_TYPE_ID
                                            WHERE SD.ACTIVE = 1
                                              AND SD.DELETED = 0";

            //If the id passed is of a certain data
            if(supplierDetailsRequest.SupplierId != 0)
            {
                fetchSupplierDetails += " AND SD.ID = @ID";
            }

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(fetchSupplierDetails, con))
                {
                    if(supplierDetailsRequest.SupplierId != 0)
                    {
                        cmd.Parameters.AddWithValue("@ID", supplierDetailsRequest.SupplierId);
                    }

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Models.Suppliers.FetchSupplierDetails.Response.SupplierDetailsResponse supplierDetailsResponse = new Models.Suppliers.FetchSupplierDetails.Response.SupplierDetailsResponse();

                            supplierDetailsResponse.SupplierId = Convert.ToInt32(dr["ID"]);
                            supplierDetailsResponse.SupplierName = Convert.ToString(dr["NAME"]);
                            supplierDetailsResponse.SupplierMobile = Convert.ToString(dr["CONTACT_NO"]);
                            supplierDetailsResponse.SupplierLocation = Convert.ToString(dr["LOCATION"]);
                            supplierDetailsResponse.SupplierTypeId = Convert.ToInt32(dr["SUPPLIER_TYPE_ID"]);
                            supplierDetailsResponse.SupplierTINNumber = Convert.ToString(dr["TIN_NUMBER"]);
                            supplierDetailsResponse.SupplierRegion = Convert.ToString(dr["REGION"]);
                            supplierDetailsResponse.SupplierHelpDesk = Convert.ToString(dr["HELP_DESK"]);
                            supplierDetailsResponse.SupplierTypeName = Convert.ToString(dr["SUPPLIER_TYPE_NAME"]);

                            supplierDetailsResponseList.Add(supplierDetailsResponse);

                            //Soumee - 29-March-2018 - Adding Loggers        
                            Logger.Write(logIdentifier, "fetching the supplier details inside the reader", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__FETCH_SUPPLIER_DETAILS);
                        }                            
                    }
                }
            }

            if(supplierDetailsResponseList.Count != 0)
            {
                supplierDetailsResponseListResponse.IsOk = true;
                supplierDetailsResponseListResponse.Message = "Supplier details list is";
                supplierDetailsResponseListResponse.ResponseObject = supplierDetailsResponseList;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, supplierDetailsRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__FETCH_SUPPLIER_DETAILS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__FETCH_SUPPLIER_DETAILS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                supplierDetailsResponseListResponse.Message = "Error fetching the supplier details list";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, supplierDetailsRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__FETCH_SUPPLIER_DETAILS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__FETCH_SUPPLIER_DETAILS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(supplierDetailsResponseListResponse);
        }

        /// <summary>
        /// This api is used to edit the supplier type details
        /// </summary>
        /// Created By: Soumee
        /// Date: 22 March, 2018
        /// Issue No.:
        /// Issue Description:
        /// Input Json:
        /// {
        /// "SupplierTypeId":1,
        /// "SupplierTypeName":"Haldiram Sweets",
        /// "SupplierTypeCode":"HLRS",
        /// }
        /// <param name="editSupplierTypeInput">SupplierTypeId, SupplierTypeName, SupplierTypeCode</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("EditSupplierType")]
        public IHttpActionResult EditSupplierType(Models.Suppliers.EditSupplierType.Request.EditSupplierTypeInput editSupplierTypeInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Suppliers.EditSupplierType";
            Logger.Write(logIdentifier, editSupplierTypeInput.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__EDIT_SUPPLIER_TYPE);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__EDIT_SUPPLIER_TYPE);
            //-Soumee - 29-March-2018 - Adding Loggers

            Response response = new Response();

            int rowsAffected = 0;

            //Query to edit supplier type details
            string editSupplierType = @"UPDATE SUPPLIER_TYPES
                                            SET NAME = @NAME,
                                                CODE = @CODE,
                                                MODIFIED_TIME = @MODIFIED_TIME
                                            WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(editSupplierType, con))
                {
                    cmd.Parameters.AddWithValue("@NAME", editSupplierTypeInput.SupplierTypeName);
                    cmd.Parameters.AddWithValue("@CODE", editSupplierTypeInput.SupplierTypeCode);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ID", editSupplierTypeInput.SupplierTypeId);

                    rowsAffected = (int)cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "editing supplier type details", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__EDIT_SUPPLIER_TYPE);
                }
            }
            if(rowsAffected > 0)
            {
                response.IsOk = true;
                response.Message = "Successfully edited the supplier type details.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, editSupplierTypeInput.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__EDIT_SUPPLIER_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__EDIT_SUPPLIER_TYPE);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                response.Message = "Unable to edit the supplier type details. Please try again.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, editSupplierTypeInput.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__EDIT_SUPPLIER_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__EDIT_SUPPLIER_TYPE);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(response);
        }

        /// <summary>
        /// This api is used to delete the supplier types
        /// </summary>
        /// Created By: Soumee
        /// Date: 22 March, 2018
        /// Issue No.:
        /// Issue Description:
        /// Input Json:
        /// {
        /// "SupplierTypeId":1,
        /// }
        /// <param name="deleteSupplierTypeInput">SupplierTypeId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("DeleteSupplierType")]
        public IHttpActionResult DeleteSupplierType(Models.Suppliers.DeleteSupplierType.Request.DeleteSupplierTypeInput deleteSupplierTypeInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Suppliers.DeleteSupplierType";
            Logger.Write(logIdentifier, deleteSupplierTypeInput.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__DELETE_SUPPLIER_TYPE);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__DELETE_SUPPLIER_TYPE);
            //-Soumee - 29-March-2018 - Adding Loggers

            Response response = new Response();

            int rowsAffected = 0;

            //Query to delete the supplier type
            string deleteSupplierType = @"UPDATE SUPPLIER_TYPES
                                                SET ACTIVE = 0,
                                                    DELETED = 1,
                                                    MODIFIED_TIME = GETDATE()
                                                WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(deleteSupplierType, con))
                {
                    cmd.Parameters.AddWithValue("@ID", deleteSupplierTypeInput.SupplierTypeId);

                    rowsAffected = (int)cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "deleting the supplier type", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__DELETE_SUPPLIER_TYPE);
                }
            }
            if(rowsAffected > 0)
            {
                response.IsOk = true;
                response.Message = "Supplier types deleted successfully.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, deleteSupplierTypeInput.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__DELETE_SUPPLIER_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__DELETE_SUPPLIER_TYPE);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                response.Message = "Unable to delete the supplier types. Please try again.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, deleteSupplierTypeInput.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__DELETE_SUPPLIER_TYPE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__DELETE_SUPPLIER_TYPE);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(response);
        }

        /// <summary>
        /// This api is used to delete the supplier details.
        /// </summary>
        /// Created By: Soumee
        /// Date: 22 March, 2018
        /// Issue No.:
        /// Issue Description:
        /// Input Json:
        /// {
        /// "SupplierId":1,
        /// }
        /// <param name="deleteSupplierInput">SupplierId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("DeleteSupplier")]
        public IHttpActionResult DeleteSupplier(Models.Suppliers.DeleteSupplier.Request.DeleteSupplierInput deleteSupplierInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Suppliers.DeleteSupplier";
            Logger.Write(logIdentifier, deleteSupplierInput.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__DELETE_SUPPLIER);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__DELETE_SUPPLIER);
            //-Soumee - 29-March-2018 - Adding Loggers

            Response response = new Response();

            int rowsAffected = 0;

            //Query to delete the supplier details
            string deleteSupplier = @"UPDATE SUPPLIER_DETAILS
                                                SET ACTIVE = 0,
                                                    DELETED = 1,
                                                    MODIFIED_TIME = GETDATE()
                                                WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(deleteSupplier, con))
                {
                    cmd.Parameters.AddWithValue("@ID", deleteSupplierInput.SupplierId);

                    rowsAffected = (int)cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "deleting the supplier details", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__DELETE_SUPPLIER);
                }
            }
            if (rowsAffected > 0)
            {
                response.IsOk = true;
                response.Message = "Supplier has been deleted.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, deleteSupplierInput.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__DELETE_SUPPLIER);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__DELETE_SUPPLIER);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                response.Message = "Unable to delete the supplier. Please try again.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, deleteSupplierInput.ToString(), DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__DELETE_SUPPLIER);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.SUPPLIERS__DELETE_SUPPLIER);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(response);
        }
    }
}

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
    [RoutePrefix("api/Items")]
    public class ItemsController : ApiController
    {
        /// <summary>
        /// This api is used to create new item Category
        /// </summary>
        /// Created By: Soumee
        /// Date: 16th March, 2018
        /// Issue No.: td-965
        /// Issue Description: Inhouse | API | Inventory | Create Item Category
        /// Input Json:
        /// {
        /// "ItemCategoryName":"Bakery",
        /// "ItemCategoryCode":"BKR"
        /// }
        /// <param name="itemCategoryRequest">ItemCategoryName, ItemCategoryCode</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("CreateItemCategory")]
        public IHttpActionResult CreateItemCategory(Models.Items.ItemCategoryCreateRequest.Request.ItemCategoryCreateRequest itemCategoryRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Items.CreateItemCategory";
            Logger.Write(logIdentifier, itemCategoryRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEM_CATEGORY);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEM_CATEGORY);
            //-Soumee - 29-March-2018 - Adding Loggers

            //+Soumee - 29-March-2018 - Checking for duplicate code
            Models.Items.CreateItemCategory.Response.ItemCategoryResponse itemCategory = new Models.Items.CreateItemCategory.Response.ItemCategoryResponse();
            Response<Models.Items.CreateItemCategory.Response.ItemCategoryResponse> itemCategoryResponse = new Response<Models.Items.CreateItemCategory.Response.ItemCategoryResponse>();

            Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput duplicateCodeCheckInput = new Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput();

            duplicateCodeCheckInput.Code = itemCategoryRequest.ItemCategoryCode;
            duplicateCodeCheckInput.TableName = "item_categories";

            IMS.Controllers.Services.IItemsServices itemsServices = new ItemsService();
            itemCategoryResponse.IsOk = itemsServices.CheckForDuplicateCode(duplicateCodeCheckInput).IsOk;

            if (itemCategoryResponse.IsOk == false)
            {
                itemCategoryResponse.Message = "This Code already exists. Please try a different code.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, itemCategoryRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEM_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEM_CATEGORY);
                //-Soumee - 29-March-2018 - Adding Loggers

                return Ok(itemCategoryResponse);
            }
            //-Soumee - 29-March-2018 - Checking for duplicate code

            //Query to create new item category
            string createItemCategory = @"INSERT INTO Item_Categories
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
                using (SqlCommand cmd = new SqlCommand(createItemCategory, con))
                {
                    cmd.Parameters.AddWithValue("@NAME", itemCategoryRequest.ItemCategoryName);
                    cmd.Parameters.AddWithValue("@CODE", itemCategoryRequest.ItemCategoryCode);
                    cmd.Parameters.AddWithValue("@ACTIVE", 1);
                    cmd.Parameters.AddWithValue("@DELETED", 0);
                    cmd.Parameters.AddWithValue("@CREATED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CREATED_BY", 0);
                    cmd.Parameters.AddWithValue("@MODIFIED_BY", 0);

                    itemCategory.ItemCategoryId = int.Parse(cmd.ExecuteScalar().ToString());
                    Logger.Write(logIdentifier, "Creating new item category", DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEM_CATEGORY);//Soumee - 29-March-2018 - Adding Loggers
                }
            }

            if(itemCategory.ItemCategoryId != 0)
            {
                itemCategoryResponse.IsOk = true;
                itemCategoryResponse.Message = "Item Category added successfully";
                itemCategoryResponse.ResponseObject = itemCategory;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, itemCategoryRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEM_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEM_CATEGORY);
                //-Soumee - 29-March-2018 - Adding Loggers

            }
            else
            {
                itemCategoryResponse.IsOk = false;
                itemCategoryResponse.Message = "Unable to add the Item Category. Please try again.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, itemCategoryRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEM_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEM_CATEGORY);
                //-Soumee - 29-March-2018 - Adding Loggers

            }
            return Ok(itemCategoryResponse);
        }

        /// <summary>
        /// This api is used to fetch the item categories. If ItemCategoryId = 0, all item categories are fetched
        /// </summary>
        /// Created By: Soumee
        /// Date: 16th March, 2018
        /// Issue No.: td-966
        /// Issue Description: Inhouse | API | Inventory | Fetch Item Category
        /// Input Json:
        /// {
        /// "ItemCategoryId":1
        /// }
        /// <param name="itemCategoryRequest">ItemCategoryId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("FetchItemCategory")]
        public IHttpActionResult FetchItemCategory(Models.Items.FetchItemCategory.Request.ItemCategoryRequest itemCategoryRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Items.FetchItemCategory";
            Logger.Write(logIdentifier, itemCategoryRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__FETCH_ITEM_CATEGORY);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.ITEMS__FETCH_ITEM_CATEGORY);
            //-Soumee - 29-March-2018 - Adding Loggers

            List<Models.Items.FetchItemCategory.Response.ItemCategoryResponse> itemCategoryResponseList = new List<Models.Items.FetchItemCategory.Response.ItemCategoryResponse>();
            Response<List<Models.Items.FetchItemCategory.Response.ItemCategoryResponse>> itemCategoryResponseListResponse = new Response<List<Models.Items.FetchItemCategory.Response.ItemCategoryResponse>>();

            //Query to fetch all item categories when id = 0
            string fetchItemCategory = @"SELECT ID,
                                                   NAME,
                                                   CODE
                                            FROM ITEM_CATEGORIES
                                            WHERE ACTIVE = 1
                                              AND DELETED = 0";

            //If id passed is a specific id
            if(itemCategoryRequest.ItemCategoryId != 0)
            {
                fetchItemCategory += " AND ID = @ID";
            }

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(fetchItemCategory, con))
                {
                    if(itemCategoryRequest.ItemCategoryId != 0)
                    {
                        cmd.Parameters.AddWithValue("@ID", itemCategoryRequest.ItemCategoryId);
                    }

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Models.Items.FetchItemCategory.Response.ItemCategoryResponse itemCategoryResponse = new Models.Items.FetchItemCategory.Response.ItemCategoryResponse();

                            itemCategoryResponse.ItemCategoryId = Convert.ToInt32(dr["ID"]);
                            itemCategoryResponse.ItemCategoryName = Convert.ToString(dr["NAME"]);
                            itemCategoryResponse.ItemCategoryCode = Convert.ToString(dr["CODE"]);

                            itemCategoryResponseList.Add(itemCategoryResponse);
                            Logger.Write(logIdentifier, "Inside the reader of fetching item categories", DateTime.Now.Ticks, LoggerConstants.ITEMS__FETCH_ITEM_CATEGORY);//Soumee - 29-March-2018 - Adding Loggers
                        }
                    }
                }
            }
            if(itemCategoryResponseList.Count != 0)
            {
                itemCategoryResponseListResponse.IsOk = true;
                itemCategoryResponseListResponse.Message = "Item Categories are";
                itemCategoryResponseListResponse.ResponseObject = itemCategoryResponseList;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, itemCategoryRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__FETCH_ITEM_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__FETCH_ITEM_CATEGORY);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                itemCategoryResponseListResponse.Message = "No item Categories found.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, itemCategoryRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__FETCH_ITEM_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__FETCH_ITEM_CATEGORY);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(itemCategoryResponseListResponse);
        }

        /// <summary>
        /// This api is used to fetch store items. If StoreId = 0, all store items are fetched. 
        /// If store item parameter and value is passed, along with storeId, it can be used for searching.
        /// </summary>
        ///  Created By: Soumee
        /// Date: 16th March, 2018
        /// Issue No.: td-972
        /// Issue Description: Inhouse | API | Inventory | Fetch Store Items
        /// Input Json:
        /// {
        /// "StoreId":0,
        /// "StoreItemParameter":"Name",
        /// "StoreItemValue":"Pencils",
        /// }
        /// <param name="storeItemRequest">StoreItemParameter, StoreItemValue</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("FetchStoreItems")]
        public IHttpActionResult FetchStoreItems(Models.Items.FetchStoreItems.Request.StoreItemRequest storeItemRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Items.FetchStoreItems";
            Logger.Write(logIdentifier, storeItemRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__FETCH_STORE_ITEMS);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.ITEMS__FETCH_STORE_ITEMS);
            //-Soumee - 29-March-2018 - Adding Loggers

            List<Models.Items.FetchStoreItems.Response.StoreItemResponse> storeItemResponseList = new List<Models.Items.FetchStoreItems.Response.StoreItemResponse>();
            Response<List<Models.Items.FetchStoreItems.Response.StoreItemResponse>> storeItemReponseListResponse = new Response<List<Models.Items.FetchStoreItems.Response.StoreItemResponse>>();

            string fetchStoreItems = "";

            //If StoreId = 0 fetch all the items
            if (storeItemRequest.StoreId == 0)
            {
                fetchStoreItems = @"SELECT DISTINCT(I.ID),
                                            S.ID AS STORE_ID,
                                            S.NAME AS STORE_NAME,
                                            IC.NAME AS ITEM_CATEGORY_NAME,
                                            IC.ID AS ITEM_CATEGORY_ID,
                                            I.NAME,
                                            I.CODE,
                                            I.BATCH_NO,
                                            I.QUANTITY,
                                            I.UNIT_PRICE,
                                            I.TAX,
                                            I.SELLABLE
                                    FROM ITEMS I
                                    INNER JOIN ITEM_TO_STORE ITS ON ITS.ITEM_ID = I.ID
                                    INNER JOIN STORES S ON S.ID = ITS.STORE_ID
                                    INNER JOIN ITEM_TO_ITEM_CATEGORY ITIC ON ITIC.ITEM_ID = ITS.ITEM_ID
                                    INNER JOIN ITEM_CATEGORIES IC ON IC.ID = ITIC.ITEM_CATEGORY_ID
                                    WHERE I.ACTIVE = 1
                                        AND I.DELETED = 0";

                //StoreItemParameter refers to the table column name.
                //StoreItemValue refers to its value.
                //Example: StoreItemParameter = 'Name' and StoreItemValue = 'CAKE'
                if (storeItemRequest.StoreItemParameter != null && storeItemRequest.StoreItemValue != null)
                {
                    fetchStoreItems += " AND I." + storeItemRequest.StoreItemParameter + " = '" + storeItemRequest.StoreItemValue + "'";
                }
            }
            //This query will execute when storeId is passed
            else
            {
                fetchStoreItems = @"SELECT DISTINCT(I.ID),
                                            S.ID AS STORE_ID,
                                            S.NAME AS STORE_NAME,
                                            IC.NAME AS ITEM_CATEGORY_NAME,
                                            IC.ID AS ITEM_CATEGORY_ID,
                                            I.NAME,
                                            I.CODE,
                                            I.BATCH_NO,
                                            I.QUANTITY,
                                            I.UNIT_PRICE,
                                            I.TAX,
                                            I.SELLABLE
                                    FROM ITEMS I
                                    INNER JOIN ITEM_TO_STORE ITS ON ITS.ITEM_ID = I.ID
                                    INNER JOIN STORES S ON S.ID = ITS.STORE_ID
                                    INNER JOIN ITEM_TO_ITEM_CATEGORY ITIC ON ITIC.ITEM_ID = ITS.ITEM_ID
                                    INNER JOIN ITEM_CATEGORIES IC ON IC.ID = ITIC.ITEM_CATEGORY_ID
                                    WHERE I.ACTIVE = 1
                                        AND I.DELETED = 0
                                        AND S.ID =@SID";

                //StoreItemParameter refers to the table column name.
                //StoreItemValue refers to its value.
                //Example: StoreItemParameter = 'Name' and StoreItemValue = 'CAKE'
                if (storeItemRequest.StoreItemParameter != null && storeItemRequest.StoreItemValue != null)
                {
                    fetchStoreItems += " AND I." + storeItemRequest.StoreItemParameter + " = '" + storeItemRequest.StoreItemValue + "'";
                }
            }
            

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(fetchStoreItems, con))
                {
                    if (storeItemRequest.StoreId != 0)
                    {
                        cmd.Parameters.AddWithValue("@SID", storeItemRequest.StoreId);
                    }

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Models.Items.FetchStoreItems.Response.StoreItemResponse storeItemResponse = new Models.Items.FetchStoreItems.Response.StoreItemResponse();

                            storeItemResponse.StoreItemId = Convert.ToInt32(dr["ID"]);
                            storeItemResponse.StoreItemName = Convert.ToString(dr["NAME"]);
                            storeItemResponse.StoreItemCode = Convert.ToString(dr["CODE"]);
                            storeItemResponse.StoreItemBatchNo = Convert.ToString(dr["BATCH_NO"]);
                            storeItemResponse.StoreItemQuantity = Convert.ToInt32(dr["QUANTITY"]);
                            storeItemResponse.StoreItemPrice = Convert.ToDouble(dr["UNIT_PRICE"]);
                            storeItemResponse.StoreItemTax = Convert.ToDouble(dr["TAX"]);
                            storeItemResponse.StoreItemSellable = Convert.ToInt32(dr["SELLABLE"]);
                            storeItemResponse.StoreName = Convert.ToString(dr["STORE_NAME"]);
                            storeItemResponse.ItemCategoryName = Convert.ToString(dr["ITEM_CATEGORY_NAME"]);
                            storeItemResponse.ItemCategoryId = Convert.ToInt32(dr["ITEM_CATEGORY_ID"]);
                            storeItemResponse.StoreId = Convert.ToInt32(dr["STORE_ID"]);

                            storeItemResponseList.Add(storeItemResponse);
                            Logger.Write(logIdentifier, "Inside the reader fetching items", DateTime.Now.Ticks, LoggerConstants.ITEMS__FETCH_STORE_ITEMS);//Soumee - 29-March-2018 - Adding Loggers
                        }
                    }
                }
            }

            if(storeItemResponseList.Count > 0)
            {
                storeItemReponseListResponse.IsOk = true;
                storeItemReponseListResponse.Message = "Item List is";
                storeItemReponseListResponse.ResponseObject = storeItemResponseList;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeItemRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__FETCH_STORE_ITEMS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__FETCH_STORE_ITEMS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                storeItemReponseListResponse.Message = "No items found";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeItemRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__FETCH_STORE_ITEMS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__FETCH_STORE_ITEMS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(storeItemReponseListResponse);
        }

        /// <summary>
        /// This api is used to create items for a store
        /// </summary>
        /// Created By: Soumee
        /// Date: 16th March, 2018
        /// Issue No.: td-973
        /// Issue Description: Inhouse | Api | Inventory | Create Items For A Store
        /// Input Json:
        /// {
        /// "StoreItemName":"Jewellary",
        /// "StoreItemCode":"JJ",
        /// "StoreItemBatchNo":"54236",
        /// "StoreItemQuantity":90,
        /// "StoreItemPrice":800,
        /// "StoreItemTax":"5",
        /// "StoreItemSellable":0,
        /// "ExpiryDate":"2022-03-25",
        /// }
        /// <param name="createItemRequest">StoreItemName, StoreItemCode, StoreItemBatchNo, StoreItemQuantity, StoreItemPrice, StoreItemTax, StoreItemSellable</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("CreateItemsForAStore")]
        public IHttpActionResult CreateItemsForAStore(Models.Items.CreateItemsForAStore.Request.CreateItemRequest createItemRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Items.CreateItemsForAStore";
            Logger.Write(logIdentifier, createItemRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEMS_FOR_A_STORE);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEMS_FOR_A_STORE);
            //-Soumee - 29-March-2018 - Adding Loggers

            Response<Models.Items.CreateItemsForAStore.Response.CreateItemResponse> createItemResponse = new Response<Models.Items.CreateItemsForAStore.Response.CreateItemResponse>();
            Models.Items.CreateItemsForAStore.Response.CreateItemResponse createItem = new Models.Items.CreateItemsForAStore.Response.CreateItemResponse();

            //+Soumee - 29-March-2018 - Checking for code duplicacy
            Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput duplicateCodeCheckInput = new Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput();

            duplicateCodeCheckInput.Code = createItemRequest.StoreItemCode;
            duplicateCodeCheckInput.TableName = "Items";

            IMS.Controllers.Services.IItemsServices itemsServices = new ItemsService();
            createItemResponse.IsOk = itemsServices.CheckForDuplicateCode(duplicateCodeCheckInput).IsOk;

            if (createItemResponse.IsOk == false)
            {
                createItemResponse.Message = "This Code already exists. Please try a different code.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, createItemRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEMS_FOR_A_STORE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEMS_FOR_A_STORE);
                //-Soumee - 29-March-2018 - Adding Loggers

                return Ok(createItemResponse);
            }
            //-Soumee - 29-March-2018 - Checking for code duplicacy

            //Query to create new items
            string createItems = @"INSERT INTO ITEMS
                                        VALUES(@NAME,
                                               @CODE,
                                               @BATCH_NO,
                                               @QUANTITY,
                                               @UNIT_PRICE,                                               
                                               @TAX,
                                               @SELLABLE,
                                               @ACTIVE,
                                               @DELETED,
                                               @CREATED_TIME,
                                               @MODIFIED_TIME,
                                               @CREATED_BY,
                                               @MODIFIED_BY,
                                               @ITEM_EXPIRY_DATE);

                                        SELECT SCOPE_IDENTITY()";

            //Query to make fresh entries in the no. of items available
            string createItemAvailability = @"INSERT INTO ITEM_AVAILABILITY
                                                        VALUES(@STORE_ID,
                                                               @ITEM_ID,
                                                               @QUANTITY_PURCHASED,
                                                               @QUANTITY_SOLD,
                                                               @QUANTITY_AVAILABLE,
                                                               @MODIFIED_ON)";

            string itemsInStoreMapping = @"INSERT INTO ITEM_TO_STORE
                                                    VALUES(@ITEM_ID,
                                                           @STORE_ID,
                                                           @ACTIVE,
                                                           @DELETED,
                                                           @CREATED_TIME,
                                                           @MODIFIED_TIME,
                                                           @CREATED_BY,
                                                           @MODIFIED_BY)";

            string item_toItemCategoryMapping = @"INSERT INTO ITEM_TO_ITEM_CATEGORY
                                                                VALUES(@ITEM_ID,
                                                                       @ITEM_CATEGORY_ID,
                                                                       @ACTIVE,
                                                                       @DELETED,
                                                                       @CREATED_TIME,
                                                                       @MODIFIED_TIME,
                                                                       @CREATED_BY,
                                                                       @MODIFIED_BY)";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(createItems, con))
                {
                    cmd.Parameters.AddWithValue("@NAME", createItemRequest.StoreItemName);
                    cmd.Parameters.AddWithValue("@CODE", createItemRequest.StoreItemCode);
                    cmd.Parameters.AddWithValue("@BATCH_NO", createItemRequest.StoreItemBatchNo);
                    cmd.Parameters.AddWithValue("@QUANTITY", createItemRequest.StoreItemQuantity);
                    cmd.Parameters.AddWithValue("@UNIT_PRICE", createItemRequest.StoreItemPrice);
                    cmd.Parameters.AddWithValue("@ITEM_EXPIRY_DATE", createItemRequest.ExpiryDate);
                    cmd.Parameters.AddWithValue("@TAX", createItemRequest.StoreItemTax);
                    cmd.Parameters.AddWithValue("@SELLABLE", createItemRequest.StoreItemSellable);
                    cmd.Parameters.AddWithValue("@ACTIVE", 1);
                    cmd.Parameters.AddWithValue("@DELETED", 0);
                    cmd.Parameters.AddWithValue("@CREATED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CREATED_BY", 0);
                    cmd.Parameters.AddWithValue("@MODIFIED_BY", 0);

                    createItem.ItemId = int.Parse(cmd.ExecuteScalar().ToString()); //ItemId generated.

                    //Soumee - 29-March-2018 - Adding Loggers
                    Logger.Write(logIdentifier, "Creating a new item", DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEMS_FOR_A_STORE);
                }

                using (SqlCommand cmd = new SqlCommand(createItemAvailability, con))
                {
                    cmd.Parameters.AddWithValue("@STORE_ID", createItemRequest.StoreId);
                    cmd.Parameters.AddWithValue("@ITEM_ID", createItem.ItemId);
                    cmd.Parameters.AddWithValue("@QUANTITY_PURCHASED", createItemRequest.StoreItemQuantity);
                    cmd.Parameters.AddWithValue("@QUANTITY_SOLD", 0);
                    cmd.Parameters.AddWithValue("@QUANTITY_AVAILABLE", createItemRequest.StoreItemQuantity);
                    cmd.Parameters.AddWithValue("@MODIFIED_ON", DateTime.Now);

                    cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers
                    Logger.Write(logIdentifier, "Making fresh entries of the new item in the item_availability table", DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEMS_FOR_A_STORE);
                }

                using (SqlCommand cmd = new SqlCommand(itemsInStoreMapping, con))
                {
                    cmd.Parameters.AddWithValue("@ITEM_ID", createItem.ItemId);
                    cmd.Parameters.AddWithValue("@STORE_ID", createItemRequest.StoreId);
                    cmd.Parameters.AddWithValue("@ACTIVE", 1);
                    cmd.Parameters.AddWithValue("@DELETED", 0);
                    cmd.Parameters.AddWithValue("@CREATED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CREATED_BY", 0);
                    cmd.Parameters.AddWithValue("@MODIFIED_BY", 0);

                    cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers
                    Logger.Write(logIdentifier, "Creating item to store mapping", DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEMS_FOR_A_STORE);
                }

                using (SqlCommand cmd = new SqlCommand(item_toItemCategoryMapping, con))
                {
                    cmd.Parameters.AddWithValue("@ITEM_ID", createItem.ItemId);
                    cmd.Parameters.AddWithValue("@ITEM_CATEGORY_ID", createItemRequest.ItemCategoryId);
                    cmd.Parameters.AddWithValue("@ACTIVE", 1);
                    cmd.Parameters.AddWithValue("@DELETED", 0);
                    cmd.Parameters.AddWithValue("@CREATED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CREATED_BY", 0);
                    cmd.Parameters.AddWithValue("@MODIFIED_BY", 0);

                    cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers
                    Logger.Write(logIdentifier, "Creating item to item_category mapping", DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEMS_FOR_A_STORE);
                }
            }

            if(createItem.ItemId != 0)
            {
                createItemResponse.IsOk = true;
                createItemResponse.Message = "Item created successfully";
                createItemResponse.ResponseObject = createItem;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, createItemRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEMS_FOR_A_STORE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEMS_FOR_A_STORE);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                createItemResponse.IsOk = false;
                createItemResponse.Message = "Unable to create the item Lets's try again";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, createItemRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEMS_FOR_A_STORE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__CREATE_ITEMS_FOR_A_STORE);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(createItemResponse);
        }

        /// <summary>
        /// This api is used to edit an item details
        /// </summary>
        /// Created By: Soumee
        /// Date: 16th March, 2018
        /// Issue No.: td-974
        /// Issue Description: Inhouse | Api | Inventory | Edit Store Items
        /// Input Json:
        /// {
        /// "StoreItemId":3,
        /// "StoreItemCode":"JJLY1",
        /// "StoreItemBatchNo":"54236",
        /// "StoreItemQuantity":50,
        /// "StoreItemPrice":800000,
        /// "StoreItemTax":"65",
        /// "StoreItemSellable":1,
        /// "StoreId":1,
        /// "StoreItemName":"Cake",
        /// "StoreItemCatgeoryId":2,
        /// }
        /// <param name="storeItemRequest">StoreItemId, StoreItemCode, StoreItemBatchNo, StoreItemQuantity, StoreItemPrice, StoreItemTax, StoreItemSellable, StoreId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("EditStoreItems")]
        public IHttpActionResult EditStoreItems(Models.Items.EditStoreItems.Request.StoreItemInput storeItemRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Items.EditStoreItems";
            Logger.Write(logIdentifier, storeItemRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_STORE_ITEMS);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_STORE_ITEMS);
            //-Soumee - 29-March-2018 - Adding Loggers

            Response response = new Response();
            Models.Items.EditStoreItems.Core.ItemAvailableInformation itemAvailableInformation = new Models.Items.EditStoreItems.Core.ItemAvailableInformation();

            int rowsAffected = 0;

            //+Soumee - 29-March-2018 - Checking for code duplicacy
            Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput duplicateCodeCheckInput = new Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput();

            duplicateCodeCheckInput.Code = storeItemRequest.StoreItemCode;
            duplicateCodeCheckInput.TableName = "Items";

            IMS.Controllers.Services.IItemsServices itemsServices = new ItemsService();
            response.IsOk = itemsServices.CheckForDuplicateCode(duplicateCodeCheckInput).IsOk;

            if (response.IsOk == false)
            {
                response.Message = "This Code already exists. Please try a different code.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeItemRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_STORE_ITEMS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_STORE_ITEMS);
                //-Soumee - 29-March-2018 - Adding Loggers

                return Ok(response);
            }
            //-Soumee - 29-March-2018 - Checking for code duplicacy

            //Query to edit the item details
            string editItems = @"UPDATE ITEMS
                                    SET CODE = @CODE,
                                        NAME = @NAME,
                                        BATCH_NO = @BATCH_NO,
                                        QUANTITY = @QUANTITY,
                                        UNIT_PRICE = @UNIT_PRICE,
                                        TAX = @TAX,
                                        SELLABLE = @SELLABLE,
                                        MODIFIED_TIME = @MODIFIED_TIME
                                    WHERE ID = @ID";

            //Query to update the items available
            string updateItemAvailability = @"UPDATE ITEM_AVAILABILITY
                                        SET QUANTITY_PURCHASED = @QUANTITY_PURCHASED,
                                            MODIFIED_ON = @MODIFIED_ON,
                                            QUANTITY_AVAILABLE = @QUANTITY_AVAILABLE
                                        WHERE ITEM_ID = @ITEM_ID
                                          AND STORE_ID = @STORE_ID";

            //Query to fetch the no. of items available. This is used to add or substract from the original number and update the no. of items available.
            string fetchItemAvailable = @"SELECT QUANTITY_PURCHASED,
                                                    QUANTITY_SOLD,
                                                    QUANTITY_AVAILABLE
                                            FROM ITEM_AVAILABILITY
                                            WHERE ITEM_ID = @ITEM_ID
                                                AND STORE_ID = @STORE_ID";

            //Query to edit the item category attached to the item
            string updateItemCatgory = @"UPDATE ITEM_TO_ITEM_CATEGORY
                                                SET ITEM_CATEGORY_ID = @ITEM_CATEGORY_ID,
                                                    MODIFIED_TIME = @MODIFIED_TIME
                                                WHERE ITEM_ID = @ITEM_ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(fetchItemAvailable, con))
                {
                    cmd.Parameters.AddWithValue("@ITEM_ID", storeItemRequest.StoreItemId);
                    cmd.Parameters.AddWithValue("@STORE_ID", storeItemRequest.StoreId);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if(dr.Read())
                        {
                            itemAvailableInformation.QuantityAvailable = Convert.ToInt32(dr["QUANTITY_AVAILABLE"]);
                            itemAvailableInformation.QuantityPurchased = Convert.ToInt32(dr["QUANTITY_PURCHASED"]);
                            itemAvailableInformation.QuantitySold = Convert.ToInt32(dr["QUANTITY_SOLD"]);

                            //Soumee - 29-March-2018 - Adding Loggers
                            Logger.Write(logIdentifier, "Fetching the no. of items available from the Item_Availability table", DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_STORE_ITEMS);
                        }                        
                    }
                }

                using (SqlCommand cmd = new SqlCommand(editItems, con))
                {
                    cmd.Parameters.AddWithValue("@CODE", storeItemRequest.StoreItemCode);
                    cmd.Parameters.AddWithValue("@NAME", storeItemRequest.StoreItemName);
                    cmd.Parameters.AddWithValue("@BATCH_NO", storeItemRequest.StoreItemBatchNo);
                    cmd.Parameters.AddWithValue("@QUANTITY", storeItemRequest.StoreItemQuantity);
                    cmd.Parameters.AddWithValue("@UNIT_PRICE", storeItemRequest.StoreItemPrice);
                    cmd.Parameters.AddWithValue("@TAX", storeItemRequest.StoreItemTax);
                    cmd.Parameters.AddWithValue("@SELLABLE", storeItemRequest.StoreItemSellable);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ID", storeItemRequest.StoreItemId);

                    rowsAffected = (int)cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers
                    Logger.Write(logIdentifier, "Editing the item details. Updating the items table", DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_STORE_ITEMS);
                }

                using (SqlCommand cmd = new SqlCommand(updateItemCatgory, con))
                {
                    cmd.Parameters.AddWithValue("@ITEM_CATEGORY_ID", storeItemRequest.StoreItemCatgeoryId);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ITEM_ID", storeItemRequest.StoreItemId);

                    cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers
                    Logger.Write(logIdentifier, "Updatring the item category attached to the item", DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_STORE_ITEMS);
                }

                if (itemAvailableInformation.QuantityPurchased != storeItemRequest.StoreItemQuantity)
                {
                    itemAvailableInformation.QuantityPurchased = itemAvailableInformation.QuantityPurchased + storeItemRequest.StoreItemQuantity;
                    itemAvailableInformation.QuantityAvailable = itemAvailableInformation.QuantityPurchased;

                    using (SqlCommand cmd = new SqlCommand(updateItemAvailability, con))
                    {
                        cmd.Parameters.AddWithValue("@ITEM_ID", storeItemRequest.StoreItemId);
                        cmd.Parameters.AddWithValue("@STORE_ID", storeItemRequest.StoreId);
                        cmd.Parameters.AddWithValue("@QUANTITY_PURCHASED", itemAvailableInformation.QuantityPurchased);
                        cmd.Parameters.AddWithValue("@QUANTITY_AVAILABLE", itemAvailableInformation.QuantityAvailable);
                        cmd.Parameters.AddWithValue("@MODIFIED_ON", DateTime.Now);

                        cmd.ExecuteNonQuery();

                        //Soumee - 29-March-2018 - Adding Loggers
                        Logger.Write(logIdentifier, "updating the quantity of items available in the items_availability table", DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_STORE_ITEMS);
                    }
                }
            }

            if(rowsAffected != 0)
            {
                response.IsOk = true;
                response.Message = "Item details updated.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeItemRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_STORE_ITEMS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_STORE_ITEMS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                response.IsOk = false;
                response.Message = "Unable to update the item details. Lets's try again.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeItemRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_STORE_ITEMS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_STORE_ITEMS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(response);
        }

        /// <summary>
        /// This api is used to delete the item
        /// </summary>
        /// Created By: Soumee
        /// Date: 16th March, 2018
        /// Issue No.: td-975
        /// Issue Description: Inhouse | API | Inventory | Delete Store Items
        /// Input Json:
        /// {
        /// "StoreItemId":3,
        /// }
        /// <param name="storeItemRequest">StoreItemId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("DeleteStoreItems")]
        public IHttpActionResult DeleteStoreItems(Models.Items.DeleteStoreItems.Request.StoreItemRequest storeItemRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Items.DeleteStoreItems";
            Logger.Write(logIdentifier, storeItemRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__DELETE_STORE_ITEMS);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.ITEMS__DELETE_STORE_ITEMS);
            //-Soumee - 29-March-2018 - Adding Loggers

            Response response = new Response();

            int rowsAffected = 0;

            //Query to delete the items. We will update the active, Deleted column of the items table.
            string deleteItem = @"UPDATE ITEMS
                                    SET ACTIVE = 0,
                                        DELETED = 1,
                                        MODIFIED_TIME = @MODIFIED_TIME
                                    WHERE ID=@ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(deleteItem, con))
                {
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ID", storeItemRequest.StoreItemId);

                    rowsAffected = (int)cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "Updating the active, Deleted column of the items table", DateTime.Now.Ticks, LoggerConstants.ITEMS__DELETE_STORE_ITEMS);
                }
            }
            if (rowsAffected > 0)
            {
                response.IsOk = true;
                response.Message = "The item has been successfully deleted";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeItemRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__DELETE_STORE_ITEMS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__DELETE_STORE_ITEMS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                response.Message = "Unable to delete the item. Let's try again";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, storeItemRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__DELETE_STORE_ITEMS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__DELETE_STORE_ITEMS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(response);
        }

        /// <summary>
        /// This api is used to edit the Item categories
        /// </summary>
        /// Created By: Soumee
        /// Date: 22 March, 2018
        /// Issue No.: td-994
        /// Issue Description: Inhouse | Api | Inventory | Edit Item Category
        /// Input Json:
        /// {
        /// "ItemCategoryId":1,
        /// "ItemCategoryName":"Cafeteria",
        /// "ItemCategoryCode":"CFTR",
        /// }
        /// <param name="editItemCategoryInput">ItemCategoryId, ItemCategoryName, ItemCategoryCode</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("EditItemCategory")]
        public IHttpActionResult EditItemCategory(Models.Items.EditItemCategory.Request.EditItemCategoryInput editItemCategoryInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Items.EditItemCategory";
            Logger.Write(logIdentifier, editItemCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_ITEM_CATEGORY);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_ITEM_CATEGORY);
            //-Soumee - 29-March-2018 - Adding Loggers

            Response response = new Response();

            //+Soumee - 29-March-2018 - Checking for code duplicacy
            Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput duplicateCodeCheckInput = new Models.Stores.CheckForDuplicateCode.Request.DuplicateCodeCheckInput();

            duplicateCodeCheckInput.Code = editItemCategoryInput.ItemCategoryCode;
            duplicateCodeCheckInput.TableName = "Item_Categories";

            IMS.Controllers.Services.IItemsServices itemsServices = new ItemsService();
            response.IsOk = itemsServices.CheckForDuplicateCode(duplicateCodeCheckInput).IsOk;

            if (response.IsOk == false)
            {
                response.Message = "This Code already exists. Please try a different code.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, editItemCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_ITEM_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_ITEM_CATEGORY);
                //-Soumee - 29-March-2018 - Adding Loggers

                return Ok(response);
            }
            //-Soumee - 29-March-2018 - Checking for code duplicacy

            int rowsAffected = 0;

            //Query to edit the item category
            string editItemCategory = @"UPDATE ITEM_CATEGORIES
                                                SET NAME = @NAME,
                                                    CODE = @CODE,
                                                    MODIFIED_TIME = @MODIFIED_TIME
                                                WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(editItemCategory, con))
                {
                    cmd.Parameters.AddWithValue("@NAME", editItemCategoryInput.ItemCategoryName);
                    cmd.Parameters.AddWithValue("@CODE", editItemCategoryInput.ItemCategoryCode);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ID", editItemCategoryInput.ItemCategoryId);

                    rowsAffected = (int)cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "editing a item category", DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_ITEM_CATEGORY);
                }
            }

            if(rowsAffected > 0)
            {
                response.IsOk = true;
                response.Message = "Item category details updated successfully.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, editItemCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_ITEM_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_ITEM_CATEGORY);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                response.IsOk = false;
                response.Message = "Unable to update the item category details. Please try again.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, editItemCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_ITEM_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__EDIT_ITEM_CATEGORY);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(response);
        }

        /// <summary>
        /// This api is used to delete item categories
        /// </summary>
        /// Created By: Soumee
        /// Date: 22 March, 2018
        /// Issue No.: td-995
        /// Issue Description: Inhouse | Api | Inventory | Delete Item Category
        /// Input Json: 
        /// {
        /// "ItemCategoryId":1,
        /// }
        /// <param name="deleteItemCategoryInput">ItemCategoryId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("DeleteItemCategory")]
        public IHttpActionResult DeleteItemCategory(Models.Items.DeleteItemCategory.Request.DeleteItemCategoryInput deleteItemCategoryInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Items.DeleteItemCategory";
            Logger.Write(logIdentifier, deleteItemCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__DELETE_ITEM_CATEGORY);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.ITEMS__DELETE_ITEM_CATEGORY);
            //-Soumee - 29-March-2018 - Adding Loggers

            Response response = new Response();

            int rowsAffected = 0;

            //Query to update the active,DEleted column of the item_Categories table
            string deleteItemCategory = @"UPDATE ITEM_CATEGORIES
                                                SET ACTIVE = 0,
                                                    DELETED = 1,
                                                    MODIFIED_TIME = GETDATE()
                                                WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(deleteItemCategory, con))
                {
                    cmd.Parameters.AddWithValue("@ID", deleteItemCategoryInput.ItemCategoryId);

                    rowsAffected = (int)cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "updating the active,DEleted column of the item_Categories table", DateTime.Now.Ticks, LoggerConstants.ITEMS__DELETE_ITEM_CATEGORY);
                }
            }

            if(rowsAffected > 0)
            {
                response.IsOk = true;
                response.Message = "Successfully deleted the item category";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, deleteItemCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__DELETE_ITEM_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__DELETE_ITEM_CATEGORY);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                response.Message = "Failed to delete the item category. Please try again.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, deleteItemCategoryInput.ToString(), DateTime.Now.Ticks, LoggerConstants.ITEMS__DELETE_ITEM_CATEGORY);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.ITEMS__DELETE_ITEM_CATEGORY);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(response);
        }

    }
}

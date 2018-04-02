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
using IMS.Helpers.Constants;
using IMS.Helpers.Logger;

namespace IMS.Controllers
{
    [RoutePrefix("api/Transaction")]
    public class TransactionController : ApiController
    {
        /// <summary>
        /// This api is used to create purchase order for each item
        /// </summary>
        /// Created By: Soumee
        /// Date: 16th March, 2018
        /// Issue No.: td-986
        /// Issue Description: Inhouse | Api | Inventory | Create Purchase Order
        /// Input JSON:
        /// {
        /// "StoreId":1,
        /// "SupplierTypeId":1,
        /// "SupplierDetailsId":1,
        /// "DeliveryStatusId":1,
        /// "PurchaseOrderReference":"Avinash",
        /// "ItemDetails":
        /// [
        /// {
        /// "User":"Admin",
        /// "ItemId":1,
        /// "ItemUnitPrice":25000,
        /// "ItemQuantity":3,
        /// "DiscountPercentage":2.5,
        /// "TaxPercentage":12.5
        /// },
        /// {
        /// "User":"Admin",
        /// "ItemId":2,
        /// "ItemUnitPrice":3,
        /// "ItemQuantity":100,
        /// "DiscountPercentage":0,
        /// "TaxPercentage":0
        /// }]
        /// }
        /// <param name="purchaseOrderRequest">User, &StoreId, &SupplierTypeId, &SupplierDetailsId, &DeliveryStatusId, &PurchaseOrderReference, ItemId, ItemUnitPrice, ItemQuantity, DiscountPercentage, TaxPercentage</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("CreatePurchaseOrder")]
        public IHttpActionResult CreatePurchaseOrder(Models.Transaction.CreatePurchaseOrder.Request.PurchaseOrderRequest purchaseOrderRequest)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Transaction.CreatePurchaseOrder";
            Logger.Write(logIdentifier, purchaseOrderRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_PURCHASE_ORDER);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_PURCHASE_ORDER);
            //-Soumee - 29-March-2018 - Adding Loggers

            Response<Models.Transaction.CreatePurchaseOrder.Response.PurchaseOrderResponse> purchaseOrderResponse = new Response<Models.Transaction.CreatePurchaseOrder.Response.PurchaseOrderResponse>();
            Models.Transaction.CreatePurchaseOrder.Response.PurchaseOrderResponse purchaseOrder = new Models.Transaction.CreatePurchaseOrder.Response.PurchaseOrderResponse();

            int rowsAffected = 0;

            //Query to create new purchase order
            string createPurchaseOrder = @"INSERT INTO PURCHASE_ORDERS
                                                        VALUES(@STORE_ID,
                                                               @SUPPLIER_TYPE_ID,
                                                               @SUPPLIER_DETAILS_ID,
                                                               @DELIVERY_STATUS_ID,
                                                               @REFERENCE,
                                                               @ACTIVE,
                                                               @DELETED,
                                                               @CREATED_TIME,
                                                               @MODIFIED_TIME,
                                                               @CREATED_BY,
                                                               @MODIFIED_BY);

                                                        SELECT SCOPE_IDENTITY()";

            //1 purchase order can have multiple items. Hence query to map the purchase order with the items in that purchase order
            string purchaseOrderMappingQuery = @"INSERT INTO PURCHASE_ORDER_TO_ITEMS
                                                        VALUES";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(createPurchaseOrder, con))
                {
                    cmd.Parameters.AddWithValue("@STORE_ID", purchaseOrderRequest.StoreId);
                    cmd.Parameters.AddWithValue("@SUPPLIER_TYPE_ID", purchaseOrderRequest.SupplierTypeId);
                    cmd.Parameters.AddWithValue("@SUPPLIER_DETAILS_ID", purchaseOrderRequest.SupplierDetailsId);
                    cmd.Parameters.AddWithValue("@DELIVERY_STATUS_ID", purchaseOrderRequest.DeliveryStatusId);
                    cmd.Parameters.AddWithValue("@REFERENCE", purchaseOrderRequest.PurchaseOrderReference);
                    cmd.Parameters.AddWithValue("@ACTIVE", 1);
                    cmd.Parameters.AddWithValue("@DELETED", 0);
                    cmd.Parameters.AddWithValue("@CREATED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CREATED_BY", 0);
                    cmd.Parameters.AddWithValue("@MODIFIED_BY", 0);

                    purchaseOrder.PurchaseOrderNo = int.Parse(cmd.ExecuteScalar().ToString());

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "Creating new purchase order and fetching the purchaseOderId", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_PURCHASE_ORDER);
                }

                //Generating query for multiple itrems mapping to a single purchase order
                foreach (Models.Transaction.CreatePurchaseOrder.Core.POItemsInput itemDetails in purchaseOrderRequest.ItemDetails)
                {
                    purchaseOrderMappingQuery += "(" + purchaseOrder.PurchaseOrderNo + ",";
                    purchaseOrderMappingQuery += " '" + itemDetails.User + "',";
                    purchaseOrderMappingQuery += " " + itemDetails.ItemId + ",";
                    purchaseOrderMappingQuery += " " + itemDetails.ItemUnitPrice + ",";
                    purchaseOrderMappingQuery += " " + itemDetails.ItemQuantity + ",";
                    purchaseOrderMappingQuery += " " + itemDetails.DiscountPercentage + ",";
                    purchaseOrderMappingQuery += " " + itemDetails.TaxPercentage + ",";
                    purchaseOrderMappingQuery += " 1, 0, getdate(), getdate(), 0, 0),";
                }
                purchaseOrderMappingQuery = purchaseOrderMappingQuery.RemoveTrailingCharacters(1);

                using (SqlCommand cmd = new SqlCommand(purchaseOrderMappingQuery, con))
                {
                    rowsAffected = (int)cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "Mapping multiple items with the purchase order", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_PURCHASE_ORDER);
                }
            }

            if(purchaseOrder.PurchaseOrderNo > 0)
            {
                purchaseOrderResponse.IsOk = true;
                purchaseOrderResponse.Message = "Purchase Order successfully created";
                purchaseOrderResponse.ResponseObject = purchaseOrder;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, purchaseOrderRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_PURCHASE_ORDER);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_PURCHASE_ORDER);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                purchaseOrderResponse.Message = "Oops! Unable to create purchase order";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, purchaseOrderRequest.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_PURCHASE_ORDER);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_PURCHASE_ORDER);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(purchaseOrderResponse);
        }

        /// <summary>
        /// This api is used to fetch all the purchase order details. If PurchaseOrderNo = 0, all purchase order details are fetched 
        /// </summary>
        /// Created By: Soumee
        /// Date: 16th March, 2018
        /// Issue No.: td-987
        /// Issue Description: Inhouse | Api | Inventory | Fetch Purchase Order
        /// Input JSON:
        /// {
        /// "PurchaseOrderNo":0,
        /// }
        /// <param name="purchaseOrderInput">PurchaseOrderNo</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("FetchPurchaseOrder")]
        public IHttpActionResult FetchPurchaseOrder(Models.Transaction.FetchPurchaseOrder.Request.PurchaseOrderInput purchaseOrderInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Transaction.FetchPurchaseOrder";
            Logger.Write(logIdentifier, purchaseOrderInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_PURCHASE_ORDER);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_PURCHASE_ORDER);
            //-Soumee - 29-March-2018 - Adding Loggers

            List<Models.Transaction.FetchPurchaseOrder.Response.PurchaseOrderOutput> purchaseOrderOutputList = new List<Models.Transaction.FetchPurchaseOrder.Response.PurchaseOrderOutput>();
            Response<List<Models.Transaction.FetchPurchaseOrder.Response.PurchaseOrderOutput>> purchaseOrderOutputListResponse = new Response<List<Models.Transaction.FetchPurchaseOrder.Response.PurchaseOrderOutput>>();

            //Query to fetch purchase order details. Each purchase order can have multiple items, addititonal charges and discounts.
            string fetchPurchaseOrderQuery = @"SELECT DISTINCT(POI.ITEM_ID) AS ItemId,
                                                                 PO.ID,
                                                                 POI.USER_NAME AS USERNAME,
                                                                 POI.ID AS PURCHASE_ORDER_TO_ITEMS_ID,
                                                                 S.NAME AS StoreName,
                                                                 S.ID AS StoreId,
                                                                 ST.NAME AS SupplierTypeName,
                                                                 SD.NAME AS SupplierDetailsName,
                                                                 DS.NAME AS DeliveryStatus,
                                                                 PO.REFERENCE,
                                                                 I.NAME as ItemName,
                                                                 POI.UNIT_PRICE,
                                                                 POI.QUANTITY,
                                                                 POI.DISCOUNT_PERCENTAGE,
                                                                 POI.TAX_PERCENTAGE
                                                          FROM PURCHASE_ORDERS PO
                                                          INNER JOIN PURCHASE_ORDER_TO_ITEMS POI ON POI.PURCHASE_ORDER_ID = PO.ID
                                                          INNER JOIN STORES S ON S.ID = PO.STORE_ID
                                                          INNER JOIN SUPPLIER_TYPES ST ON ST.ID = PO.SUPPLIER_TYPE_ID
                                                          INNER JOIN SUPPLIER_DETAILS SD ON SD.ID = PO.SUPPLIER_DETAILS_ID
                                                          INNER JOIN DELIVERY_STATUS DS ON DS.ID = PO.DELIVERY_STATUS_ID
                                                          INNER JOIN ITEMS I ON I.ID = POI.ITEM_ID WHERE PO.ACTIVE = 1
                                                          AND PO.DELETED = 0"; 

            //If only 1 purchase order needs to be fetched by id
            if(purchaseOrderInput.PurchaseOrderNo != 0)
            {
                fetchPurchaseOrderQuery += " AND PO.ID = @ID";
            }

            //This is for the searching parameters as per the UI
            if(purchaseOrderInput.InvoiceStatus != "" && purchaseOrderInput.InvoiceStatus != null)
            {
                fetchPurchaseOrderQuery = @"SELECT DISTINCT(POI.ITEM_ID) AS ItemId,
                                                                 PO.ID,
                                                                 POI.USER_NAME AS USERNAME,
                                                                 POI.ID AS PURCHASE_ORDER_TO_ITEMS_ID,
                                                                 S.NAME AS StoreName,
                                                                 S.ID AS StoreId,
                                                                 ST.NAME AS SupplierTypeName,
                                                                 SD.NAME AS SupplierDetailsName,
                                                                 DS.NAME AS DeliveryStatus,
                                                                 PO.REFERENCE,
                                                                 I.NAME as ItemName,
                                                                 POI.UNIT_PRICE,
                                                                 POI.QUANTITY,
                                                                 POI.DISCOUNT_PERCENTAGE,
                                                                 POI.TAX_PERCENTAGE
                                                          FROM PURCHASE_ORDERS PO
                                                          INNER JOIN PURCHASE_ORDER_TO_ITEMS POI ON POI.PURCHASE_ORDER_ID = PO.ID
                                                          INNER JOIN STORES S ON S.ID = PO.STORE_ID
                                                          INNER JOIN SUPPLIER_TYPES ST ON ST.ID = PO.SUPPLIER_TYPE_ID
                                                          INNER JOIN SUPPLIER_DETAILS SD ON SD.ID = PO.SUPPLIER_DETAILS_ID
                                                          INNER JOIN DELIVERY_STATUS DS ON DS.ID = PO.DELIVERY_STATUS_ID
                                                          INNER JOIN ITEMS I ON I.ID = POI.ITEM_ID
                                                          INNER JOIN BILLING_DETAILS BD ON BD.STORE_ID = S.ID
                                                          INNER JOIN INVOICE_DETAILS IND ON IND.ID = BD.INVOICE_ID WHERE PO.ACTIVE = 1
                                                          AND PO.DELETED = 0
                                                          AND IND.PAID_STATUS = @PAID_STATUS";
            }

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(fetchPurchaseOrderQuery, con))
                {
                    if (purchaseOrderInput.PurchaseOrderNo != 0)
                    {
                        cmd.Parameters.AddWithValue("@ID", purchaseOrderInput.PurchaseOrderNo);
                    }
                    if (purchaseOrderInput.InvoiceStatus != "" && purchaseOrderInput.InvoiceStatus != null)
                    {
                        cmd.Parameters.AddWithValue("@PAID_STATUS", purchaseOrderInput.InvoiceStatus);
                    }

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while(dr.Read())
                        {
                            Models.Transaction.FetchPurchaseOrder.Response.PurchaseOrderOutput purchaseOrderOutput = new Models.Transaction.FetchPurchaseOrder.Response.PurchaseOrderOutput();

                            purchaseOrderOutput.PurchaseOrderNo = Convert.ToInt32(dr["ID"]);
                            purchaseOrderOutput.PurchaseOrderUser = Convert.ToString(dr["USERNAME"]);
                            purchaseOrderOutput.StoreName = Convert.ToString(dr["StoreName"]);
                            purchaseOrderOutput.StoreId = Convert.ToInt32(dr["StoreId"]);
                            purchaseOrderOutput.SupplierTypeName = Convert.ToString(dr["SupplierTypeName"]);
                            purchaseOrderOutput.SupplierDetailsName = Convert.ToString(dr["SupplierDetailsName"]);
                            purchaseOrderOutput.DeliveryStatus = Convert.ToString(dr["DeliveryStatus"]);
                            purchaseOrderOutput.PurchaseOrderReference = Convert.ToString(dr["Reference"]);
                            purchaseOrderOutput.ItemName = Convert.ToString(dr["ItemName"]);
                            purchaseOrderOutput.ItemId = Convert.ToInt32(dr["ItemId"]);
                            purchaseOrderOutput.ItemUnitPrice = Convert.ToDouble(dr["UNIT_PRICE"]);
                            purchaseOrderOutput.ItemQuantity = Convert.ToInt32(dr["QUANTITY"]);
                            purchaseOrderOutput.DiscountPercentage = Convert.ToDouble(dr["DISCOUNT_PERCENTAGE"]);
                            purchaseOrderOutput.TaxPercentage = Convert.ToDouble(dr["TAX_PERCENTAGE"]);
                            purchaseOrderOutput.PurchaseOrderToItemsId = Convert.ToInt32(dr["PURCHASE_ORDER_TO_ITEMS_ID"]);

                            purchaseOrderOutputList.Add(purchaseOrderOutput);

                            //Soumee - 29-March-2018 - Adding Loggers        
                            Logger.Write(logIdentifier, "fetching purchase order details inside the reader", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_PURCHASE_ORDER);
                        }
                    }
                }
            }

            if(purchaseOrderOutputList.Count != 0)
            {
                purchaseOrderOutputListResponse.IsOk = true;
                purchaseOrderOutputListResponse.Message = "Purchase Order details are";
                purchaseOrderOutputListResponse.ResponseObject = purchaseOrderOutputList;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, purchaseOrderInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_PURCHASE_ORDER);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_PURCHASE_ORDER);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                purchaseOrderOutputListResponse.Message = "Unable to fetch the details of the Purchase Orders";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, purchaseOrderInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_PURCHASE_ORDER);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_PURCHASE_ORDER);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(purchaseOrderOutputListResponse);
        }

        /// <summary>
        /// This api is used to fetch all the delivery status for purchase order form fill up details
        /// </summary>
        /// Created By: Soumee
        /// Date: 21 March, 2018
        /// Issue No.: td-988
        /// Issue Description: Inhouse | Api | Inventory | Fetch All Delivery Status
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("FetchAllDeliveryStatus")]
        public IHttpActionResult FetchAllDeliveryStatus()
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Transaction.FetchAllDeliveryStatus";
            Logger.Write(logIdentifier, "No input parameters", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_ALL_DELIVERY_STATUS);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_ALL_DELIVERY_STATUS);
            //-Soumee - 29-March-2018 - Adding Loggers

            List<Models.Transaction.FetchAllDeliveryStatus.Response.DeliveryStatusOutput> deliverySatusOutputList = new List<Models.Transaction.FetchAllDeliveryStatus.Response.DeliveryStatusOutput>();
            Response<List<Models.Transaction.FetchAllDeliveryStatus.Response.DeliveryStatusOutput>> deliveryStatusOutputListResponse = new Response<List<Models.Transaction.FetchAllDeliveryStatus.Response.DeliveryStatusOutput>>();

            //Query to fetch all the delievery status to show it in the drop down as per the UI
            string fetchAllDeliveryStatus = @"SELECT NAME
                                                FROM DELIVERY_STATUS
                                                WHERE ACTIVE = 1
                                                  AND
                                                  DELETED = 0";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(fetchAllDeliveryStatus, con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while(dr.Read())
                        {
                            Models.Transaction.FetchAllDeliveryStatus.Response.DeliveryStatusOutput deliveryStatusOutput = new Models.Transaction.FetchAllDeliveryStatus.Response.DeliveryStatusOutput();

                            deliveryStatusOutput.DeliveryStatus = Convert.ToString(dr["NAME"]);

                            deliverySatusOutputList.Add(deliveryStatusOutput);

                            //Soumee - 29-March-2018 - Adding Loggers        
                            Logger.Write(logIdentifier, "Fetching all the delivery status inside the reader", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_ALL_DELIVERY_STATUS);
                        }
                    }
                }
            }

            if(deliverySatusOutputList.Count != 0)
            {
                deliveryStatusOutputListResponse.IsOk = true;
                deliveryStatusOutputListResponse.Message = "The delivery status are";
                deliveryStatusOutputListResponse.ResponseObject = deliverySatusOutputList;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, "No parameters to pass", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_ALL_DELIVERY_STATUS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_ALL_DELIVERY_STATUS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                deliveryStatusOutputListResponse.Message = "No delivery status exists. Please entry some.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, "No parameters to pass", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_ALL_DELIVERY_STATUS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_ALL_DELIVERY_STATUS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }

            return Ok(deliveryStatusOutputListResponse);
        }

        /// <summary>
        /// This api is used to fetch the GRN details. If GRNNumber = 0, all GRN details are fetched
        /// </summary>
        /// Created By: Soumee
        /// Date: 21 March, 2018
        /// Issue No.: td-990
        /// Issue Description: Inhouse | Api | Inventory | Fetch GRN Details
        /// Input JSON:
        /// {
        /// "GRNNumber":1,
        /// }
        /// <param name="grnDetailsInput">GRNNumber</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("FetchGRNDetails")]
        public IHttpActionResult FetchGRNDetails(Models.Transaction.FetchGRNDetails.Request.GRNDetailsInput grnDetailsInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Transaction.FetchGRNDetails";
            Logger.Write(logIdentifier, grnDetailsInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_GRN_DETAILS);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_GRN_DETAILS);
            //-Soumee - 29-March-2018 - Adding Loggers

            List<Models.Transaction.FetchGRNDetails.Response.GRNDetailsOutput> grnDetailsOutputList = new List<Models.Transaction.FetchGRNDetails.Response.GRNDetailsOutput>();
            Response<List<Models.Transaction.FetchGRNDetails.Response.GRNDetailsOutput>> grnDetailsOutputListResponse = new Response<List<Models.Transaction.FetchGRNDetails.Response.GRNDetailsOutput>>();

            //Query to fetch the GRN details. Every Supplier needs to have a new GRN, i.e., one GRN can have multiple Items from a single supplier.
            string fetchGRNDetails = @"SELECT distinct(POI.ITEM_ID),
                                               GRN.RECEIVED_BILL_NUMBER,
                                               GRN.RECEIVED_BILL_DATE,
                                               GRN.GRN_DATE,
                                               GRN.ID AS GRN_ID,
                                               POI.PURCHASE_ORDER_ID,
                                               POI.QUANTITY,
                                               POI.UNIT_PRICE,
                                               POI.DISCOUNT_PERCENTAGE,
                                               POI.TAX_PERCENTAGE,
                                               S.NAME AS STORE_NAME,
                                               SD.NAME AS SUPPLIER_NAME
                                        FROM GOODS_RECEIPT_NOTES GRN
                                        INNER JOIN PURCHASE_ORDERS PO ON PO.ID = GRN.PURCHASE_ORDER_ID
                                        INNER JOIN PURCHASE_ORDER_TO_ITEMS POI ON POI.PURCHASE_ORDER_ID = PO.ID
                                        INNER JOIN STORES S ON S.ID = PO.STORE_ID
                                        INNER JOIN SUPPLIER_DETAILS SD ON SD.ID = PO.SUPPLIER_DETAILS_ID
                                        WHERE GRN.ACTIVE = 1
                                          AND GRN.DELETED = 0";

            //If a specific GRN id is passed to fetch only 1 GRN details
            if(grnDetailsInput.GRNNumber != 0)
            {
                fetchGRNDetails += " AND ID = @ID";
            }

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(fetchGRNDetails, con))
                {
                    if (grnDetailsInput.GRNNumber != 0)
                    {
                        cmd.Parameters.AddWithValue("@ID", grnDetailsInput.GRNNumber);
                    }

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while(dr.Read())
                        {
                            Models.Transaction.FetchGRNDetails.Response.GRNDetailsOutput grnDetailsOutput = new Models.Transaction.FetchGRNDetails.Response.GRNDetailsOutput();

                            grnDetailsOutput.GRNNumber = Convert.ToInt32(dr["GRN_ID"]);                           
                            grnDetailsOutput.ReceivedBillNumber = Convert.ToString(dr["RECEIVED_BILL_NUMBER"]);
                            grnDetailsOutput.ReceivedBillDate = Convert.ToDateTime(dr["RECEIVED_BILL_DATE"]);
                            grnDetailsOutput.GRNDate = Convert.ToDateTime(dr["GRN_DATE"]);
                            grnDetailsOutput.ItemId = Convert.ToInt32(dr["ITEM_ID"]);
                            grnDetailsOutput.ReceivedQuantity = Convert.ToInt32(dr["QUANTITY"]);
                            grnDetailsOutput.ItemUnitPrice = Convert.ToDouble(dr["UNIT_PRICE"]);
                            grnDetailsOutput.ItemTaxPercentage = Convert.ToDouble(dr["TAX_PERCENTAGE"]);
                            grnDetailsOutput.ItemDiscountPercentage = Convert.ToDouble(dr["DISCOUNT_PERCENTAGE"]);
                            grnDetailsOutput.StoreName = Convert.ToString(dr["STORE_NAME"]);
                            grnDetailsOutput.SupplierName = Convert.ToString(dr["SUPPLIER_NAME"]);

                            grnDetailsOutputList.Add(grnDetailsOutput);


                            //Soumee - 29-March-2018 - Adding Loggers        
                            Logger.Write(logIdentifier, "Fetching the GRN details inside the reader", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_GRN_DETAILS);

                        }
                    }
                }
            }
            if(grnDetailsOutputList.Count != 0)
            {
                grnDetailsOutputListResponse.IsOk = true;
                grnDetailsOutputListResponse.Message = "GRN details are";
                grnDetailsOutputListResponse.ResponseObject = grnDetailsOutputList;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, grnDetailsInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_GRN_DETAILS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_GRN_DETAILS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                grnDetailsOutputListResponse.Message = "No GRN details available to fetch";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, grnDetailsInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_GRN_DETAILS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_GRN_DETAILS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(grnDetailsOutputListResponse);
        }

        /// <summary>
        /// This api is used to fetch the invoices made
        /// </summary>
        /// Created By: Soumee
        /// Date: 21 March, 2018
        /// Issue No.:
        /// Issue Description:
        /// Input JSON:
        /// {
        /// "InvoiceNumber":0,
        /// }
        /// <param name="invoiceInput">InvoiceNumber</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("FetchInvoiceDetails")]
        public IHttpActionResult FetchInvoiceDetails(Models.Transaction.FetchInvoiceDetails.Request.InvoiceInput invoiceInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Transaction.FetchInvoiceDetails";
            Logger.Write(logIdentifier, invoiceInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_INVOICE_DETAILS);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_INVOICE_DETAILS);
            //-Soumee - 29-March-2018 - Adding Loggers

            List<Models.Transaction.FetchInvoiceDetails.Response.InvoiceOutput> invoiceOutputList = new List<Models.Transaction.FetchInvoiceDetails.Response.InvoiceOutput>();
            List<Models.Transaction.FetchInvoiceDetails.Response.InvoiceOutput> invoiceOutputListForReader = new List<Models.Transaction.FetchInvoiceDetails.Response.InvoiceOutput>();
            Response<List<Models.Transaction.FetchInvoiceDetails.Response.InvoiceOutput>> invoiceOutputListResponse = new Response<List<Models.Transaction.FetchInvoiceDetails.Response.InvoiceOutput>>();

            string fetchInvoices = "";

            //Query to fetch all the invoices
            if(invoiceInput.InvoiceNumber == 0)
            {
                fetchInvoices = @"SELECT ID,
                                    USER_ID,
                                    ADDRESS,
                                    MOBILE,
                                    RECORD_DATE,
                                    TAX,
                                    NET_TOTAL,
                                    PAID_STATUS
                            FROM INVOICE_DETAILS
                            WHERE ACTIVE = 1
                                AND DELETED = 0";

            }

            //Query to fetch a specific invoice details for searching
            if (invoiceInput.InvoiceNumber != 0)
            {
                fetchInvoices = @"SELECT ID,
                                            USER_ID,
                                            ADDRESS,
                                            MOBILE,
                                            RECORD_DATE,
                                            TAX,
                                            NET_TOTAL,
                                            PAID_STATUS
                                    FROM INVOICE_DETAILS
                                         WHERE ID = @ID
                                            AND ACTIVE = 1
                                                AND DELETED = 0";
            }
            
            //Query to fetch invoice details based on a specific storeId for searching
            if (invoiceInput.StoreId != 0)
            {
                fetchInvoices = @"SELECT DISTINCT(IND.ID),
                                               IND.USER_ID,
                                               IND.ADDRESS,
                                               IND.MOBILE,
                                               IND.RECORD_DATE,
                                               IND.TAX,
                                               IND.NET_TOTAL,
                                               IND.PAID_STATUS
                                        FROM INVOICE_DETAILS IND
                                        INNER JOIN BILLING_DETAILS BD ON BD.INVOICE_ID = IND.ID
                                        INNER JOIN STORES S ON S.ID = BD.STORE_ID
                                             WHERE IND.ACTIVE = 1
                                                 AND IND.DELETED = 0
                                                 AND S.ID = @SID";

            }

            //Query to fetch the invoice details based on both StoreId and Invoice id, for searching
            if (invoiceInput.InvoiceNumber != 0 && invoiceInput.StoreId != 0)
            {
                fetchInvoices = @"SELECT DISTINCT(IND.ID),
                                           IND.USER_ID,
                                           IND.ADDRESS,
                                           IND.MOBILE,
                                           IND.RECORD_DATE,
                                           IND.TAX,
                                           IND.NET_TOTAL,
                                           IND.PAID_STATUS
                                    FROM INVOICE_DETAILS IND
                                    INNER JOIN BILLING_DETAILS BD ON BD.INVOICE_ID = IND.ID
                                    INNER JOIN STORES S ON S.ID = BD.STORE_ID
                                    WHERE IND.ACTIVE = 1
                                      AND IND.DELETED = 0
                                      AND IND.ID = @ID
                                      AND S.ID = @SID";
            }

            //Query to fethc item details based on invoice id
            string fetchItemDetails = @"SELECT ITEM_ID,
                                                ITEM_QUANTITY,
                                                RATE
                                        FROM BILLING_DETAILS
                                        WHERE INVOICE_ID = @INVOICE_DETAILS";

            //Query to fetch additional charges based on invoice id
            string fetchAdditionalChargesDetails = @"SELECT NAME,
                                                            AMOUNT
                                                    FROM INVOICE_ADDITIONAL_CHARGES
                                                    WHERE INVOICE_ID = @INVOICE_DETAILS";

            //Query to fetch disounts based on invoice id
            string fetchDiscounts = @"SELECT NAME,
                                            AMOUNT
                                    FROM INVOICE_DISCOUNTS
                                    WHERE INVOICE_ID = @INVOICE_ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                
                using (SqlCommand cmd = new SqlCommand(fetchInvoices, con))
                {                    
                    if (invoiceInput.InvoiceNumber != 0)
                    {
                        cmd.Parameters.AddWithValue("@ID", invoiceInput.InvoiceNumber);
                    }
                    if(invoiceInput.StoreId != 0)
                    {
                        cmd.Parameters.AddWithValue("@SID", invoiceInput.StoreId);
                    }
                    
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                      
                        while (dr.Read())
                        {
                            Models.Transaction.FetchInvoiceDetails.Response.InvoiceOutput invoiceOutputForReader = new Models.Transaction.FetchInvoiceDetails.Response.InvoiceOutput();
                        
                            invoiceOutputForReader.UserId = Convert.ToInt32(dr["USER_ID"]);
                            invoiceOutputForReader.Address = Convert.ToString(dr["ADDRESS"]);
                            invoiceOutputForReader.Mobile = Convert.ToString(dr["MOBILE"]);
                            invoiceOutputForReader.InvoiceDate = Convert.ToDateTime(dr["RECORD_DATE"]);
                            invoiceOutputForReader.TotalTax = Convert.ToDouble(dr["TAX"]);
                            invoiceOutputForReader.InvoiceTotal = Convert.ToDouble(dr["NET_TOTAL"]);
                            invoiceOutputForReader.InvoiceStatus = Convert.ToString(dr["PAID_STATUS"]);
                            invoiceOutputForReader.InvoiceId = Convert.ToInt32(dr["ID"]);

                            invoiceOutputListForReader.Add(invoiceOutputForReader);

                            //Soumee - 29-March-2018 - Adding Loggers        
                            Logger.Write(logIdentifier, "Fetching invoice details inside a reader", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_INVOICE_DETAILS);
                        }
                    }
                }

                foreach(Models.Transaction.FetchInvoiceDetails.Response.InvoiceOutput invoiceDetails in invoiceOutputListForReader)
                {
                    Models.Transaction.FetchInvoiceDetails.Response.InvoiceOutput invoiceOutput = new Models.Transaction.FetchInvoiceDetails.Response.InvoiceOutput();
                    using (SqlCommand cmd = new SqlCommand(fetchItemDetails, con))
                    {
                        cmd.Parameters.AddWithValue("@INVOICE_DETAILS", invoiceDetails.InvoiceId);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            List<Models.Transaction.FetchInvoiceDetails.Core.ItemDetailsOutput> itemList = new List<Models.Transaction.FetchInvoiceDetails.Core.ItemDetailsOutput>();
                            while (dr.Read())
                            {
                                Models.Transaction.FetchInvoiceDetails.Core.ItemDetailsOutput itemOutput = new Models.Transaction.FetchInvoiceDetails.Core.ItemDetailsOutput();

                                itemOutput.ItemId = Convert.ToInt32(dr["ITEM_ID"]);
                                itemOutput.ItemQuantity = Convert.ToInt32(dr["ITEM_QUANTITY"]);
                                itemOutput.ItemRate = Convert.ToDouble(dr["RATE"]);

                                itemList.Add(itemOutput);

                                //Soumee - 29-March-2018 - Adding Loggers        
                                Logger.Write(logIdentifier, "Fetching multiple item details for an invoice id", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_INVOICE_DETAILS);
                            }
                            invoiceOutput.ItemDetailsOutput = itemList;
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand(fetchAdditionalChargesDetails, con))
                    {
                        cmd.Parameters.AddWithValue("@INVOICE_DETAILS", invoiceDetails.InvoiceId);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            List<Models.Transaction.FetchInvoiceDetails.Core.AdditionalChargesOutput> additinalList = new List<Models.Transaction.FetchInvoiceDetails.Core.AdditionalChargesOutput>();
                            while(dr.Read())
                            {
                                Models.Transaction.FetchInvoiceDetails.Core.AdditionalChargesOutput additionalChargesOutput = new Models.Transaction.FetchInvoiceDetails.Core.AdditionalChargesOutput();

                                additionalChargesOutput.ChargeName = Convert.ToString(dr["NAME"]);
                                additionalChargesOutput.ChargeAmount = Convert.ToDouble(dr["AMOUNT"]);

                                additinalList.Add(additionalChargesOutput);

                                //Soumee - 29-March-2018 - Adding Loggers        
                                Logger.Write(logIdentifier, "Fetching multiple additional charges for an invoice id", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_INVOICE_DETAILS);
                            }
                            invoiceOutput.AdditionalCharges = additinalList;
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand(fetchDiscounts, con))
                    {
                        cmd.Parameters.AddWithValue("@INVOICE_ID", invoiceDetails.InvoiceId);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            List<Models.Transaction.FetchInvoiceDetails.Core.DiscountOutputs> discountList = new List<Models.Transaction.FetchInvoiceDetails.Core.DiscountOutputs>();

                            while(dr.Read())
                            {
                                Models.Transaction.FetchInvoiceDetails.Core.DiscountOutputs discountOutputs = new Models.Transaction.FetchInvoiceDetails.Core.DiscountOutputs();

                                discountOutputs.DiscountDetails = Convert.ToString(dr["NAME"]);
                                discountOutputs.DiscountAmount = Convert.ToDouble(dr["AMOUNT"]);

                                discountList.Add(discountOutputs);

                                //Soumee - 29-March-2018 - Adding Loggers        
                                Logger.Write(logIdentifier, "Fetching multiple disounts for an invoice id", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_INVOICE_DETAILS);
                            }
                            invoiceOutput.DiscountCharges = discountList;
                        }
                    }
                    invoiceOutput.UserId = invoiceDetails.UserId;
                    invoiceOutput.Address = invoiceDetails.Address;
                    invoiceOutput.Mobile = invoiceDetails.Mobile;
                    invoiceOutput.InvoiceDate = invoiceDetails.InvoiceDate;
                    invoiceOutput.TotalTax = invoiceDetails.TotalTax;
                    invoiceOutput.InvoiceTotal = invoiceDetails.InvoiceTotal;
                    invoiceOutput.InvoiceStatus = invoiceDetails.InvoiceStatus;
                    invoiceOutput.InvoiceId = invoiceDetails.InvoiceId;

                    invoiceOutputList.Add(invoiceOutput);

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "assigning the values to the list", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__FETCH_INVOICE_DETAILS);
                }

                if (invoiceOutputList.Count != 0)
                {
                    invoiceOutputListResponse.IsOk = true;
                    invoiceOutputListResponse.Message = "Invoice List Count = " + invoiceOutputList.Count;
                    invoiceOutputListResponse.ResponseObject = invoiceOutputList;

                    //+Soumee - 29-March-2018 - Adding Loggers
                    Logger.Write(logIdentifier, invoiceInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);
                    Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);
                    //-Soumee - 29-March-2018 - Adding Loggers
                }
                else
                {
                    invoiceOutputListResponse.Message = "No invoice details to fetch.";

                    //+Soumee - 29-March-2018 - Adding Loggers
                    Logger.Write(logIdentifier, invoiceInput.ToString(), DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);
                    Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.STORES__EDIT_STORE);
                    //-Soumee - 29-March-2018 - Adding Loggers
                }
                return Ok(invoiceOutputListResponse);
                
            }
        }

        /// <summary>
        /// This api is used to delete GRN details
        /// </summary>
        /// Created By: Soumee
        /// Date: 22 March, 2018
        /// Issue No.:
        /// Issue Description:
        /// Input JSON:
        /// {
        /// "GRNDetailsId":1,
        /// }
        /// <param name="deleteGRNDetailsInput">GRNDetailsId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("DeleteGRNDetails")]
        public IHttpActionResult DeleteGRNDetails(Models.Transaction.DeleteGRNDetails.Request.DeleteGRNDetailsInput deleteGRNDetailsInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Transaction.DeleteGRNDetails";
            Logger.Write(logIdentifier, deleteGRNDetailsInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__DELETE_GRN_DETAILS);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__DELETE_GRN_DETAILS);
            //-Soumee - 29-March-2018 - Adding Loggers

            Response response = new Response();

            int rowsaffected = 0;

            //Query to delete GRN details
            string deleteGRNDetails = @"UPDATE GOODS_RECEIPT_NOTES
                                                    SET ACTIVE = 0,
                                                        DELETED = 1,
                                                        MODIFIED_TIME = GETDATE()
                                                    WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(deleteGRNDetails, con))
                {
                    cmd.Parameters.AddWithValue("@ID", deleteGRNDetailsInput.GRNDetailsId);

                    rowsaffected = (int)cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "Deleted the GRN", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__DELETE_GRN_DETAILS);
                }
            }

            if(rowsaffected > 0)
            {
                response.IsOk = true;
                response.Message = "GRN deleted successfully.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, deleteGRNDetailsInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__DELETE_GRN_DETAILS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__DELETE_GRN_DETAILS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                response.Message = "Unable to delete the GRN. Please try again.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, deleteGRNDetailsInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__DELETE_GRN_DETAILS);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__DELETE_GRN_DETAILS);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(response);
        }

        /// <summary>
        /// This api is used to create a new GRN
        /// </summary>
        /// Created By: Soumee
        /// Date: 21 March, 2018
        /// Issue No.: td-989
        /// Issue Description: Inhouse | Api | Inventory | Create GRN
        /// Input JSON:
        /// {
        /// "PurchaseOrderId":1,
        /// "ReceivedBillNumber":"SM101",
        /// "GRNDate":"2018-03-28",
        /// "ReceivedBillDate":"2018-03-28",
        /// "OtherCharges":250,
        /// "GRNItemInputs":
        /// [
        /// {
        /// "PurchaseOrderToItemsId":1,
        /// "ItemId":1,
        /// "ItemQuantity":3,
        /// "ItemUnitPrice":24000,
        /// "ItemDiscount":0,
        /// "ItemTax":0,
        /// "ItemExpiryDate":"2018-08-28",
        /// "StoreId":1,
        /// },
        /// {
        /// "PurchaseOrderToItemsId":2,
        /// "ItemId":2,
        /// "ItemQuantity":100,
        /// "ItemUnitPrice":3,
        /// "ItemDiscount":0,
        /// "ItemTax":0,
        /// "ItemExpiryDate":"2018-08-28",
        /// "StoreId":1,
        /// }]
        /// }
        /// <param name="grnInput">ReceivedQuantity, ReceivedBillNumber, GRNDate, ReceivedBillDate, OtherCharges, ItemId, StoreId</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("CreateGRN")]
        public IHttpActionResult CreateGRN(Models.Transaction.CreateGRN.Request.GRNInput grnInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Transaction.CreateGRN";
            Logger.Write(logIdentifier, grnInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_GRN);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_GRN);
            //-Soumee - 29-March-2018 - Adding Loggers

            Models.Transaction.CreateGRN.Response.GRNOutput grnOutput = new Models.Transaction.CreateGRN.Response.GRNOutput();
            Response<Models.Transaction.CreateGRN.Response.GRNOutput> grnOutputResponse = new Response<Models.Transaction.CreateGRN.Response.GRNOutput>();
            Models.Transaction.CreateGRN.Core.UpdateItemAvailability updateItemAvailability = new Models.Transaction.CreateGRN.Core.UpdateItemAvailability();

            //Query to insert multiple items in a single GRN
            string GRNItemDetails = @"INSERT INTO GRN_ITEM_DETAILS
                                            VALUES";

            //Query to create new GRN
            string createGRN = @"INSERT INTO GOODS_RECEIPT_NOTES
                                        VALUES(@PURCHASE_ORDER_ID,
                                               @RECEIVED_BILL_NUMBER,
                                               @GRN_DATE,
                                               @RECEIVED_BILL_DATE,
                                               @OTHER_CHARGES,
                                               @ACTIVE,
                                               @DELETED,
                                               @CREATED_TIME,
                                               @MODIFIED_TIME,
                                               @CREATED_BY,
                                               @MODIFIED_BY);

                                        SELECT SCOPE_IDENTITY()";

            //After issuing GRN, i.e.,  after receiving items for a store, updating the items available in the store
            string updateItemAvailable = @"UPDATE ITEM_AVAILABILITY
                                        SET QUANTITY_PURCHASED = @QUANTITY_PURCHASED,
                                            MODIFIED_ON = @MODIFIED_ON,
                                            QUANTITY_AVAILABLE = @QUANTITY_AVAILABLE
                                        WHERE ITEM_ID = @ITEM_ID
                                                AND STORE_ID = @STORE_ID";

            //Fetching the number of items the store already has
            string fetchItemAvailable = @"SELECT QUANTITY_PURCHASED,
                                                    QUANTITY_SOLD,
                                                    QUANTITY_AVAILABLE
                                            FROM ITEM_AVAILABILITY
                                            WHERE ITEM_ID = @ITEM_ID
                                                AND STORE_ID = @STORE_ID";

            //Query to update the items availability information
            string updateItem = @"UPDATE ITEMS
                                SET QUANTITY = @QUANTITY
                                WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(createGRN, con))
                {
                    cmd.Parameters.AddWithValue("@PURCHASE_ORDER_ID", grnInput.PurchaseOrderId);
                    cmd.Parameters.AddWithValue("@RECEIVED_BILL_NUMBER", grnInput.ReceivedBillNumber);
                    cmd.Parameters.AddWithValue("@GRN_DATE", grnInput.GRNDate);
                    cmd.Parameters.AddWithValue("@RECEIVED_BILL_DATE", grnInput.ReceivedBillDate);
                    cmd.Parameters.AddWithValue("@OTHER_CHARGES", grnInput.OtherCharges);
                    cmd.Parameters.AddWithValue("@ACTIVE", 1);
                    cmd.Parameters.AddWithValue("@DELETED", 0);
                    cmd.Parameters.AddWithValue("@CREATED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CREATED_BY", 0);
                    cmd.Parameters.AddWithValue("@MODIFIED_BY", 0);

                    grnOutput.GRNNumber = int.Parse(cmd.ExecuteScalar().ToString()); //Generating new GRN number

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "Generating new GRN number", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_GRN);
                }
                                
                foreach (Models.Transaction.CreateGRN.Core.GRNItemInput itemDetails in grnInput.GRNItemInputs)
                {
                    //Generating the query for multiple items for a GRN
                    GRNItemDetails += "(" + itemDetails.PurchaseOrderToItemsId + ",";
                    GRNItemDetails += " " + itemDetails.ItemId + ",";
                    GRNItemDetails += " " + itemDetails.ItemQuantity + ",";
                    GRNItemDetails += " " + itemDetails.ItemUnitPrice + ",";
                    GRNItemDetails += " " + itemDetails.ItemDiscount + ",";
                    GRNItemDetails += " " + itemDetails.ItemTax + ",";
                    GRNItemDetails += " '" + itemDetails.ItemExpiryDate + "'),";

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "Generating the query for multiple items for a GRN", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_GRN);

                    //Fetching the no. of items already available
                    using (SqlCommand cmd = new SqlCommand(fetchItemAvailable, con))
                    {
                        cmd.Parameters.AddWithValue("@ITEM_ID", itemDetails.ItemId);
                        cmd.Parameters.AddWithValue("@STORE_ID", itemDetails.StoreId);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                updateItemAvailability.QuantityAvailable = Convert.ToInt32(dr["QUANTITY_AVAILABLE"]);
                                updateItemAvailability.QuantityPurchased = Convert.ToInt32(dr["QUANTITY_PURCHASED"]);
                                updateItemAvailability.QuantitySold = Convert.ToInt32(dr["QUANTITY_SOLD"]);

                                //Soumee - 29-March-2018 - Adding Loggers        
                                Logger.Write(logIdentifier, "Fetching the no. of items already available in the store", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_GRN);
                            }
                        }
                    }

                    //total Quantity Purchased = previously purchased quantity + newly purchased quantity
                    updateItemAvailability.QuantityPurchased = updateItemAvailability.QuantityPurchased + itemDetails.ItemQuantity;

                    //Quantity available = total quantity purchased
                    updateItemAvailability.QuantityAvailable = updateItemAvailability.QuantityPurchased;

                    using (SqlCommand cmd = new SqlCommand(updateItemAvailable, con))
                    {
                        cmd.Parameters.AddWithValue("@ITEM_ID", itemDetails.ItemId);
                        cmd.Parameters.AddWithValue("@STORE_ID", itemDetails.StoreId);
                        cmd.Parameters.AddWithValue("@QUANTITY_PURCHASED", updateItemAvailability.QuantityPurchased);
                        cmd.Parameters.AddWithValue("@QUANTITY_AVAILABLE", updateItemAvailability.QuantityAvailable);
                        cmd.Parameters.AddWithValue("@MODIFIED_ON", DateTime.Now);

                        cmd.ExecuteNonQuery();

                        //Soumee - 29-March-2018 - Adding Loggers        
                        Logger.Write(logIdentifier, "updating the no. of items availability in the store", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_GRN);
                    }

                    using (SqlCommand cmd = new SqlCommand(updateItem, con))
                    {
                        cmd.Parameters.AddWithValue("@QUANTITY", updateItemAvailability.QuantityAvailable);
                        cmd.Parameters.AddWithValue("@ID", itemDetails.ItemId);

                        cmd.ExecuteNonQuery();

                        //Soumee - 29-March-2018 - Adding Loggers        
                        Logger.Write(logIdentifier, "updating the no. of items in the items table", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_GRN);
                    }
                }
                GRNItemDetails = GRNItemDetails.RemoveTrailingCharacters(1);

                using (SqlCommand cmd = new SqlCommand(GRNItemDetails, con))
                {
                    cmd.ExecuteNonQuery();

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "Inserting multiple items for a GRN", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_GRN);
                }                
            }            

            if (grnOutput.GRNNumber != 0)
            {
                grnOutputResponse.IsOk = true;
                grnOutputResponse.Message = "The GRN details have been saved successfully";
                grnOutputResponse.ResponseObject = grnOutput;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, grnInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_GRN);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_GRN);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                grnOutputResponse.Message = "Unable to save the GRN details. Please try again.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, grnInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_GRN);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_GRN);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(grnOutputResponse);
        }

        /// <summary>
        /// This api is used to create New Invoice for items
        /// </summary>
        /// Created By: Soumee
        /// Date: 21 March, 2018
        /// Issue No.: td-1000
        /// Issue Description: Inhouse | Api | Inventory | Create New Invoice
        /// Input JSON:
        /// {
        /// "UserId":12,
        /// "Address":"BBD bagh, Kolkata",
        /// "Mobile":"8745213698",
        /// "InvoiceDate":"2018-03-14",
        /// "TotalTax":2,
        /// "ItemTotalPrice":2400,
        /// "InvoiceTotal":2500,
        /// "InvoiceStatus":"Paid",
        /// "ItemDetails":
        /// [{
        /// "ItemId":1,
        /// "ItemQuantity":10,
        /// "ItemRate":2.5,
        /// "StoreId":1,
        /// },
        /// {
        /// "ItemId":2,
        /// "ItemQuantity":20,
        /// "ItemRate":3,
        /// "StoreId":1,
        /// }],
        /// "AdditionalCharges":
        /// [{
        /// "ChargeName":"Transportation",
        /// "ChargeAmount":1500,
        /// },
        /// {
        /// "ChargeName":"Delivery",
        /// "ChargeAmount":250,
        /// }],
        /// "DiscountCharges":
        /// [{
        /// "DiscountDetails":"Flat Off",
        /// "DiscountAmount":10,
        /// },
        /// {
        /// "DiscountDetails":"Successive Discount",
        /// "DiscountAmount":5,
        /// }],
        /// }
        /// <param name="newInvoiceInput"></param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("CreateNewInvoice")]
        public IHttpActionResult CreateNewInvoice(Models.Transaction.CreateNewInvoice.Request.NewInvoiceInput newInvoiceInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Transaction.CreateNewInvoice";
            Logger.Write(logIdentifier, newInvoiceInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_NEW_INVOICE);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_NEW_INVOICE);
            //-Soumee - 29-March-2018 - Adding Loggers

            Models.Transaction.CreateNewInvoice.Core.ItemAvailabilityUpdate itemAvailabilityUpdate = new Models.Transaction.CreateNewInvoice.Core.ItemAvailabilityUpdate();
            Models.Transaction.CreateNewInvoice.Response.NewInvoiceOutput newInvoiceOutput = new Models.Transaction.CreateNewInvoice.Response.NewInvoiceOutput();
            Response<Models.Transaction.CreateNewInvoice.Response.NewInvoiceOutput> newInvoiceOutputResponse = new Response<Models.Transaction.CreateNewInvoice.Response.NewInvoiceOutput>();

            int rowsAffected = 0;

            //Query to generate new invoice details
            string createNewInvoice = @"INSERT INTO INVOICE_DETAILS
                                                VALUES(@USER_ID,
                                                       @ADDRESS,
                                                       @MOBILE,
                                                       @RECORD_DATE,
                                                       @GROSS_TOTAL,
                                                       @TAX,
                                                       @NET_TOTAL,
                                                       @PAID_STATUS,
                                                       @ACTIVE,
                                                       @DELETED,
                                                       @CREATED_TIME,
                                                       @MODIFIED_TIME,
                                                       @CREATED_BY,
                                                       @MODIFIED_BY);

                                                SELECT SCOPE_IDENTITY()";

            //Query to insert discount details for an invoice
            string insertDiscountCharges = @"INSERT INTO INVOICE_DISCOUNTS
                                                    VALUES  ";

            //Query to insert invoice to billing_details mapping
            string createBillingDetails = @"INSERT INTO BILLING_DETAILS
                                                    VALUES ";

            //Query to insert additional charges
            string insertAdditionalCharges = @"INSERT INTO INVOICE_ADDITIONAL_CHARGES
                                                            VALUES ";

            //Query to update the items available
            string updateItemAvailable = @"UPDATE ITEM_AVAILABILITY
                                        SET QUANTITY_SOLD = @QUANTITY_SOLD,
                                            MODIFIED_ON = @MODIFIED_ON,
                                            QUANTITY_AVAILABLE = @QUANTITY_AVAILABLE
                                        WHERE ITEM_ID = @ITEM_ID
                                                AND STORE_ID = @STORE_ID";

            //Query to fetch the items available
            string fetchItemAvailable = @"SELECT QUANTITY_PURCHASED,
                                                    QUANTITY_SOLD,
                                                    QUANTITY_AVAILABLE
                                            FROM ITEM_AVAILABILITY
                                            WHERE ITEM_ID = @ITEM_ID
                                                AND STORE_ID = @STORE_ID";
            
            //Query to update the items available i the items table
            string updateItem = @"UPDATE ITEMS
                                SET QUANTITY = @QUANTITY
                                WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(createNewInvoice, con))
                {
                    cmd.Parameters.AddWithValue("@USER_ID", newInvoiceInput.UserId);
                    cmd.Parameters.AddWithValue("@ADDRESS", newInvoiceInput.Address);
                    cmd.Parameters.AddWithValue("@MOBILE", newInvoiceInput.Mobile);
                    cmd.Parameters.AddWithValue("@RECORD_DATE", newInvoiceInput.InvoiceDate);
                    cmd.Parameters.AddWithValue("@GROSS_TOTAL", newInvoiceInput.ItemTotalPrice);
                    cmd.Parameters.AddWithValue("@TAX", newInvoiceInput.TotalTax);
                    cmd.Parameters.AddWithValue("@NET_TOTAL", newInvoiceInput.ItemTotalPrice);
                    cmd.Parameters.AddWithValue("@PAID_STATUS", newInvoiceInput.InvoiceStatus);
                    cmd.Parameters.AddWithValue("@ACTIVE", 1);
                    cmd.Parameters.AddWithValue("@DELETED", 0);
                    cmd.Parameters.AddWithValue("@CREATED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@MODIFIED_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CREATED_BY", 0);
                    cmd.Parameters.AddWithValue("@MODIFIED_BY", 0);

                    newInvoiceOutput.InvoiceNumber = int.Parse(cmd.ExecuteScalar().ToString()); //Generating new invoice id

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "Generating new invoice id", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_NEW_INVOICE);
                }

                if (newInvoiceInput.ItemDetails != null)
                {
                    foreach (Models.Transaction.CreateNewInvoice.Core.ItemDetails item in newInvoiceInput.ItemDetails)
                    {
                        createBillingDetails += " (" + newInvoiceOutput.InvoiceNumber + ",";
                        createBillingDetails += " " + item.StoreId + ",";
                        createBillingDetails += " " + item.ItemId + ",";
                        createBillingDetails += " " + item.ItemQuantity + ",";
                        createBillingDetails += " " + item.ItemRate + ",";
                        createBillingDetails += "0, 0, 1, 0, getdate(), getdate(), 0, 0),";

                        //Soumee - 29-March-2018 - Adding Loggers        
                        Logger.Write(logIdentifier, "Generating Query to create billing details", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_NEW_INVOICE);

                        using (SqlCommand cmd = new SqlCommand(fetchItemAvailable, con))
                        {
                            cmd.Parameters.AddWithValue("@ITEM_ID", item.ItemId);
                            cmd.Parameters.AddWithValue("@STORE_ID", item.StoreId);

                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                if (dr.Read())
                                {
                                    itemAvailabilityUpdate.QuantityAvailable = Convert.ToInt32(dr["QUANTITY_AVAILABLE"]);
                                    itemAvailabilityUpdate.QuantityPurchased = Convert.ToInt32(dr["QUANTITY_PURCHASED"]);
                                    itemAvailabilityUpdate.QuantitySold = Convert.ToInt32(dr["QUANTITY_SOLD"]);

                                    //Soumee - 29-March-2018 - Adding Loggers        
                                    Logger.Write(logIdentifier, "Fetching the quantity of items available", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_NEW_INVOICE);
                                }
                            }
                        }

                        //total Quantity sold = previous sold quantity + newly sold quantity
                        itemAvailabilityUpdate.QuantitySold = itemAvailabilityUpdate.QuantitySold + item.ItemQuantity;

                        //Quantity available = previously available quantity - quantity sold
                        itemAvailabilityUpdate.QuantityAvailable = itemAvailabilityUpdate.QuantityAvailable - itemAvailabilityUpdate.QuantitySold;

                        using (SqlCommand cmd = new SqlCommand(updateItemAvailable, con))
                        {
                            cmd.Parameters.AddWithValue("@ITEM_ID", item.ItemId);
                            cmd.Parameters.AddWithValue("@STORE_ID", item.StoreId);
                            cmd.Parameters.AddWithValue("@QUANTITY_SOLD", itemAvailabilityUpdate.QuantitySold);
                            cmd.Parameters.AddWithValue("@QUANTITY_AVAILABLE", itemAvailabilityUpdate.QuantityAvailable);
                            cmd.Parameters.AddWithValue("@MODIFIED_ON", DateTime.Now);

                            cmd.ExecuteNonQuery();

                            //Soumee - 29-March-2018 - Adding Loggers        
                            Logger.Write(logIdentifier, "Updating the quantity of items available", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_NEW_INVOICE);
                        }

                        using (SqlCommand cmd = new SqlCommand(updateItem, con))
                        {
                            cmd.Parameters.AddWithValue("@QUANTITY", itemAvailabilityUpdate.QuantityAvailable);
                            cmd.Parameters.AddWithValue("@ID", item.ItemId);

                            cmd.ExecuteNonQuery();

                            //Soumee - 29-March-2018 - Adding Loggers        
                            Logger.Write(logIdentifier, "Updating the quantity of items available in the Items table", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_NEW_INVOICE);
                        }
                    }
                    createBillingDetails = createBillingDetails.RemoveTrailingCharacters(1);

                    using (SqlCommand cmd = new SqlCommand(createBillingDetails, con))
                    {
                        rowsAffected = (int)cmd.ExecuteNonQuery();

                        //Soumee - 29-March-2018 - Adding Loggers        
                        Logger.Write(logIdentifier, "Creating billing details", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_NEW_INVOICE);
                    }
                }

                if (newInvoiceInput.AdditionalCharges != null)
                {
                    foreach (Models.Transaction.CreateNewInvoice.Core.AdditionalCharges additionalCharge in newInvoiceInput.AdditionalCharges)
                    {
                        insertAdditionalCharges += " (" + newInvoiceOutput.InvoiceNumber + ",";
                        insertAdditionalCharges += " '" + additionalCharge.ChargeName + "',";
                        insertAdditionalCharges += " " + additionalCharge.ChargeAmount + ",";
                        insertAdditionalCharges += " 1, 0, getdate(), getdate(), 0, 0),";
                    }
                    insertAdditionalCharges = insertAdditionalCharges.RemoveTrailingCharacters(1);

                    using (SqlCommand cmd = new SqlCommand(insertAdditionalCharges, con))
                    {
                        rowsAffected += (int)cmd.ExecuteNonQuery();

                        //Soumee - 29-March-2018 - Adding Loggers        
                        Logger.Write(logIdentifier, "Inserting additional charges", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_NEW_INVOICE);
                    }
                }

                if (newInvoiceInput.DiscountCharges != null)
                {
                    foreach (Models.Transaction.CreateNewInvoice.Core.Discounts discount in newInvoiceInput.DiscountCharges)
                    {
                        insertDiscountCharges += " (" + newInvoiceOutput.InvoiceNumber + ",";
                        insertDiscountCharges += " '" + discount.DiscountDetails + "',";
                        insertDiscountCharges += " " + discount.DiscountAmount + ",";
                        insertDiscountCharges += " 1, 0, getdate(), getdate(), 0, 0),";
                    }
                    insertDiscountCharges = insertDiscountCharges.RemoveTrailingCharacters(1);

                    using (SqlCommand cmd = new SqlCommand(insertDiscountCharges, con))
                    {
                        rowsAffected += (int)cmd.ExecuteNonQuery();

                        //Soumee - 29-March-2018 - Adding Loggers        
                        Logger.Write(logIdentifier, "Inserting disounts", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_NEW_INVOICE);
                    }
                }
            }

            if (rowsAffected > 0)
            {
                newInvoiceOutputResponse.IsOk = true;
                newInvoiceOutputResponse.Message = "Invoice generated";
                newInvoiceOutputResponse.ResponseObject = newInvoiceOutput;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, newInvoiceInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_NEW_INVOICE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_NEW_INVOICE);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                newInvoiceOutputResponse.Message = "Unable to generate the Invoice.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, newInvoiceInput.ToString(), DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_NEW_INVOICE);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.TRANSACTION__CREATE_NEW_INVOICE);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(newInvoiceOutputResponse);
        }
    }
}

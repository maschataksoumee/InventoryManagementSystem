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
    [RoutePrefix("api/Reports")]
    public class ReportsController : ApiController
    {
        /// <summary>
        /// This api is used for invoice report generation
        /// </summary>
        /// Created By: Soumee
        /// Date: 22 March, 2018
        /// Issue No.: td-1004
        /// Issue Description: Inhouse | Api | Inventory | Report generation
        /// Input JSON:
        /// {
        /// "StoreId":1,
        /// "FromDate":"2018-03-12",
        /// "ToDate":"2018-03-24",
        /// "PaidStatus":"paid",
        /// }
        /// <param name="invoiceReportInput">StoreId, FromDate, ToDate, PaidStatus</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("InvoiceReport")]
        public IHttpActionResult InvoiceReport(Models.Reports.InvoiceReport.Request.InvoiceReportInput invoiceReportInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Reports.InvoiceReport";
            Logger.Write(logIdentifier, invoiceReportInput.ToString(), DateTime.Now.Ticks, LoggerConstants.REPORTS__INVOICE_REPORT);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.REPORTS__INVOICE_REPORT);
            //-Soumee - 29-March-2018 - Adding Loggers

            Models.Reports.InvoiceReport.Response.InvoiceReportOutput invoiceReportOutput = new Models.Reports.InvoiceReport.Response.InvoiceReportOutput();
            List<Models.Reports.InvoiceReport.Response.InvoiceReportOutput> invoiceReportOutputList = new List<Models.Reports.InvoiceReport.Response.InvoiceReportOutput>();
            Response<List<Models.Reports.InvoiceReport.Response.InvoiceReportOutput>> invoiceReportOutputListResponse = new Response<List<Models.Reports.InvoiceReport.Response.InvoiceReportOutput>>();

            string invoiceReport = "";

            //Query to execute when StoreId, FromDate and ToDate passed
            if (invoiceReportInput.PaidStatus == "All")
            {
                invoiceReport += @"SELECT distinct(IND.ID),
                                         IND.USER_ID,
                                         IND.ADDRESS,
                                         IND.MOBILE,
                                         IND.RECORD_DATE,
                                         IND.TAX,
                                         IND.NET_TOTAL,
                                         IND.PAID_STATUS
                                    FROM INVOICE_DETAILS IND
                                    INNER JOIN BILLING_DETAILS BD ON BD.INVOICE_ID = IND.ID
                                    WHERE BD.STORE_ID = @STORE_ID
                                      AND RECORD_DATE BETWEEN @FROMDATE AND @TODATE";
            }
            else

            //Query to execute when all the parameters are passed
            {
                invoiceReport += @"SELECT distinct(IND.ID),
                                         IND.USER_ID,
                                         IND.ADDRESS,
                                         IND.MOBILE,
                                         IND.RECORD_DATE,
                                         IND.TAX,
                                         IND.NET_TOTAL,
                                         IND.PAID_STATUS
                                    FROM INVOICE_DETAILS IND
                                    INNER JOIN BILLING_DETAILS BD ON BD.INVOICE_ID = IND.ID
                                    WHERE BD.STORE_ID = @STORE_ID
                                    AND IND.PAID_STATUS = @PAID_STATUS
                                    AND RECORD_DATE BETWEEN @FROMDATE AND @TODATE";
            }

            //Query to fetch item details for an invoice id
            string fetchItemDetails = @"SELECT ITEM_ID,
                                                ITEM_QUANTITY,
                                                RATE,
                                                STORE_ID
                                        FROM BILLING_DETAILS
                                        WHERE INVOICE_ID = @INVOICE_DETAILS";

            //Query to fetch additional charges for an invoice id
            string fetchAdditionalChargesDetails = @"SELECT NAME,
                                                            AMOUNT
                                                    FROM INVOICE_ADDITIONAL_CHARGES
                                                    WHERE INVOICE_ID = @INVOICE_DETAILS";

            //Query to fetch disounts for an invoice id
            string fetchDiscounts = @"SELECT NAME,
                                            AMOUNT
                                    FROM INVOICE_DISCOUNTS
                                    WHERE INVOICE_ID = @INVOICE_ID";

            //Query to fetch the sales count for each day
            string fetchSalesCount = @"SELECT COUNT(ID) AS SALES_COUNT,
                                                CONVERT(DATE, RECORD_DATE) AS SALES_DATE
                                                FROM INVOICE_DETAILS
                                                WHERE RECORD_DATE BETWEEN @FROMDATE AND @TODATE
                                                GROUP BY RECORD_DATE";

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(fetchSalesCount, con))
                {
                    cmd.Parameters.AddWithValue("@FROMDATE", invoiceReportInput.FromDate);
                    cmd.Parameters.AddWithValue("@TODATE", invoiceReportInput.ToDate);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Models.Reports.InvoiceReport.Core.SalesCountOutput salesCountOutput = new Models.Reports.InvoiceReport.Core.SalesCountOutput();

                            salesCountOutput.SalesCount = Convert.ToInt32(dr["SALES_COUNT"]);
                            salesCountOutput.SalesDate = Convert.ToDateTime(dr["SALES_DATE"]);

                            invoiceReportOutput.SalesCountOutput.Add(salesCountOutput);

                            //Soumee - 29-March-2018 - Adding Loggers        
                            Logger.Write(logIdentifier, "Fetching the sales count for each day", DateTime.Now.Ticks, LoggerConstants.REPORTS__INVOICE_REPORT);
                        }
                    }
                }
                

                using (SqlCommand cmd = new SqlCommand(invoiceReport, con))
                {
                   
                    cmd.Parameters.AddWithValue("@STORE_ID", invoiceReportInput.StoreId);
                    cmd.Parameters.AddWithValue("@FROMDATE", invoiceReportInput.FromDate);
                    cmd.Parameters.AddWithValue("@TODATE", invoiceReportInput.ToDate);

                    if (invoiceReportInput.PaidStatus != "All")
                    {
                        cmd.Parameters.AddWithValue("@PAID_STATUS", invoiceReportInput.PaidStatus);
                    }
                        
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            invoiceReportOutput.UserId = Convert.ToInt32(dr["USER_ID"]); ;
                            invoiceReportOutput.Address = Convert.ToString(dr["ADDRESS"]);
                            invoiceReportOutput.Mobile = Convert.ToString(dr["MOBILE"]);
                            invoiceReportOutput.InvoiceDate = Convert.ToDateTime(dr["RECORD_DATE"]);
                            invoiceReportOutput.TotalTax = Convert.ToDouble(dr["TAX"]);
                            invoiceReportOutput.InvoiceTotal = Convert.ToDouble(dr["NET_TOTAL"]);
                            invoiceReportOutput.InvoiceStatus = Convert.ToString(dr["PAID_STATUS"]);
                            invoiceReportOutput.InvoiceId = Convert.ToInt32(dr["ID"]);

                            invoiceReportOutputList.Add(invoiceReportOutput);

                            //Soumee - 29-March-2018 - Adding Loggers        
                            Logger.Write(logIdentifier, "Fetching invoice details", DateTime.Now.Ticks, LoggerConstants.REPORTS__INVOICE_REPORT);
                        }
                    }
                }

                foreach (Models.Reports.InvoiceReport.Response.InvoiceReportOutput invoiceReportsDetails in invoiceReportOutputList)
                {
                    using (SqlCommand cmd = new SqlCommand(fetchItemDetails, con))
                    {
                        cmd.Parameters.AddWithValue("@INVOICE_DETAILS", invoiceReportsDetails.InvoiceId);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Models.Reports.InvoiceReport.Core.ItemDetailsResponse itemOutput = new Models.Reports.InvoiceReport.Core.ItemDetailsResponse();

                                itemOutput.ItemId = Convert.ToInt32(dr["ITEM_ID"]);
                                itemOutput.ItemQuantity = Convert.ToInt32(dr["ITEM_QUANTITY"]);
                                itemOutput.ItemRate = Convert.ToDouble(dr["RATE"]);
                                itemOutput.StoreId = Convert.ToInt32(dr["STORE_ID"]);

                                invoiceReportOutput.ItemDetailsOutput.Add(itemOutput);

                                //Soumee - 29-March-2018 - Adding Loggers        
                                Logger.Write(logIdentifier, "Fetching item details for an invoice", DateTime.Now.Ticks, LoggerConstants.REPORTS__INVOICE_REPORT);
                            }
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand(fetchAdditionalChargesDetails, con))
                    {
                        cmd.Parameters.AddWithValue("@INVOICE_DETAILS", invoiceReportsDetails.InvoiceId);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Models.Reports.InvoiceReport.Core.AdditionalChargesOutput additionalChargesOutput = new Models.Reports.InvoiceReport.Core.AdditionalChargesOutput();

                                additionalChargesOutput.ChargeName = Convert.ToString(dr["NAME"]);
                                additionalChargesOutput.ChargeAmount = Convert.ToDouble(dr["AMOUNT"]);

                                invoiceReportOutput.AdditionalCharges.Add(additionalChargesOutput);

                                //Soumee - 29-March-2018 - Adding Loggers        
                                Logger.Write(logIdentifier, "Fetching additional charges for an invoice", DateTime.Now.Ticks, LoggerConstants.REPORTS__INVOICE_REPORT);
                            }
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand(fetchDiscounts, con))
                    {
                        cmd.Parameters.AddWithValue("@INVOICE_ID", invoiceReportsDetails.InvoiceId);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Models.Reports.InvoiceReport.Core.DiscountResponse discountOutputs = new Models.Reports.InvoiceReport.Core.DiscountResponse();

                                discountOutputs.DiscountDetails = Convert.ToString(dr["NAME"]);
                                discountOutputs.DiscountAmount = Convert.ToDouble(dr["AMOUNT"]);

                                invoiceReportOutput.DiscountCharges.Add(discountOutputs);

                                //Soumee - 29-March-2018 - Adding Loggers        
                                Logger.Write(logIdentifier, "Fetching all disounts for an invoice", DateTime.Now.Ticks, LoggerConstants.REPORTS__INVOICE_REPORT);
                            }
                        }
                    }
                }
            }

            if(invoiceReportOutputList.Count > 0)
            {
                invoiceReportOutputListResponse.IsOk = true;
                invoiceReportOutputListResponse.Message = "Invoice Reports are";
                invoiceReportOutputListResponse.ResponseObject = invoiceReportOutputList;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, invoiceReportInput.ToString(), DateTime.Now.Ticks, LoggerConstants.REPORTS__INVOICE_REPORT);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.REPORTS__INVOICE_REPORT);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                invoiceReportOutputListResponse.Message = "No records exists here.";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, invoiceReportInput.ToString(), DateTime.Now.Ticks, LoggerConstants.REPORTS__INVOICE_REPORT);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.REPORTS__INVOICE_REPORT);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(invoiceReportOutputListResponse);
        }

        /// <summary>
        /// This api is used to generate item wise report. If invoiceId = 0 is send, it will fetch all the reports.
        /// </summary>
        /// Created By: Soumee
        /// Date: 26 March, 2018
        /// Issue No.: td-1005
        /// Issue Description: Inhouse | Api | Inventory | Item wise report generation
        /// Input JSON:
        /// {
        /// "StoreId":1,
        /// "ItemCategoryId":3,
        /// "InvoiceStatus":"Unpaid",
        /// "FromDate":"2018-03-10",
        /// "ToDate":"2018-03-24",
        /// }
        /// <param name="itemWiseReportInput">StoreId, ItemCategoryId, InvoiceStatus, FromDate, ToDate</param>
        /// <returns>An IHttpActionResult is returned</returns>
        [HttpPost]
        [Route("ItemWiseReportGeneration")]
        public IHttpActionResult ItemWiseReportGeneration(Models.Reports.ItemWiseReportGeneration.Request.ItemWiseReportInput itemWiseReportInput)
        {
            string CS = ConfigurationManager.ConnectionStrings["CS"].ConnectionString;

            //+Soumee - 29-March-2018 - Adding Loggers
            long logIdentifier = DateTime.Now.Ticks;

            string classAndMethodName = "Reports.CreatePurchaseOrder";
            Logger.Write(logIdentifier, itemWiseReportInput.ToString(), DateTime.Now.Ticks, LoggerConstants.REPORTS__ITEM_WISE_REPORT_GENERATION);
            Logger.Write(logIdentifier, classAndMethodName + " START", DateTime.Now.Ticks, LoggerConstants.REPORTS__ITEM_WISE_REPORT_GENERATION);
            //-Soumee - 29-March-2018 - Adding Loggers

            List<Models.Reports.ItemWiseReportGeneration.Response.ItemWiseReportOutput> itemWiseReportOutputList = new List<Models.Reports.ItemWiseReportGeneration.Response.ItemWiseReportOutput>();
            List<Models.Reports.ItemWiseReportGeneration.Response.ItemWiseReportOutput> tempItemWiseReportOutputList = new List<Models.Reports.ItemWiseReportGeneration.Response.ItemWiseReportOutput>();
            Response<List<Models.Reports.ItemWiseReportGeneration.Response.ItemWiseReportOutput>> itemWiseReportOutputListResponse = new Response<List<Models.Reports.ItemWiseReportGeneration.Response.ItemWiseReportOutput>>();

            //Query to fetch all items
            string fetchItemsByInvoiceId = @"SELECT DISTINCT(S.ID),
                                                       I.NAME,
                                                       IAV.QUANTITY_AVAILABLE,
                                                       IAV.QUANTITY_SOLD,
                                                       (IAV.QUANTITY_SOLD * I.UNIT_PRICE) AS AMOUNT_COLLECTED,
                                                       I.TAX
                                                FROM STORES S
                                                INNER JOIN ITEM_TO_STORE ITS ON ITS.STORE_ID = S.ID
                                                INNER JOIN ITEMS I ON I.ID = ITS.ITEM_ID
                                                INNER JOIN ITEM_TO_ITEM_CATEGORY ITIC ON ITIC.ITEM_ID = ITS.ITEM_ID
                                                INNER JOIN ITEM_CATEGORIES IC ON IC.ID = ITIC.ITEM_CATEGORY_ID
                                                INNER JOIN ITEM_AVAILABILITY IAV ON IAV.ITEM_ID = I.ID
                                                INNER JOIN BILLING_DETAILS BD ON BD.STORE_ID = S.ID
                                                INNER JOIN INVOICE_DETAILS IND ON IND.ID = BD.INVOICE_ID
                                                WHERE I.ACTIVE = 1
                                                  AND I.DELETED = 0";
            //AND IND.ID = @INVOICE_ID";

            string fetchInvoiceId = "";

            //Query to fetch invoice id
            fetchInvoiceId = @"SELECT ID,
                                    USER_ID,
                                    ADDRESS,
                                    MOBILE,
                                    RECORD_DATE,
                                    GROSS_TOTAL,
                                    TAX,
                                    NET_TOTAL,
                                    PAID_STATUS
                            FROM INVOICE_DETAILS
                            WHERE ACTIVE = 1
                                AND DELETED = 0";

            //Query for searching as per UI
            if (itemWiseReportInput.StoreId != 0 && itemWiseReportInput.ItemCategoryId != 0 && itemWiseReportInput.InvoiceStatus != "")
            {
                fetchInvoiceId = @"SELECT DISTINCT(IND.ID),
                                           IND.USER_ID,
                                           IND.ADDRESS,
                                           IND.MOBILE,
                                           IND.RECORD_DATE,
                                           IND.GROSS_TOTAL,
                                           IND.TAX,
                                           IND.NET_TOTAL,
                                           IND.PAID_STATUS
                                    FROM INVOICE_DETAILS IND
                                    INNER JOIN BILLING_DETAILS BD ON BD.INVOICE_ID = IND.ID
                                    INNER JOIN ITEM_TO_ITEM_CATEGORY ITIC ON ITIC.ITEM_ID = BD.ITEM_ID
                                        WHERE IND.ACTIVE = 1 AND
                                              IND.DELETED = 0 AND";

                if (itemWiseReportInput.StoreId > 0)
                {
                    fetchInvoiceId += " BD.STORE_ID = @STORE_ID AND";
                }

                if (itemWiseReportInput.ItemCategoryId > 0)
                {
                    fetchInvoiceId += " ITIC.ITEM_CATEGORY_ID = @ITEM_CATEGORY_ID AND";
                }

                if (!itemWiseReportInput.FromDate.Equals(DBNull.Value) && !itemWiseReportInput.ToDate.Equals(DBNull.Value))
                {
                    fetchInvoiceId += " IND.RECORD_DATE BETWEEN @FROM_DATE AND @TO_DATE AND";
                }

                if (itemWiseReportInput.InvoiceStatus != "")
                {
                    fetchInvoiceId += " IND.PAID_STATUS = @PAID_STATUS AND";
                }

                fetchInvoiceId = fetchInvoiceId.RemoveTrailingCharacters(3);
            }

            using (SqlConnection con = new SqlConnection(CS))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(fetchInvoiceId, con))
                {
                    if (itemWiseReportInput.StoreId != 0 && itemWiseReportInput.ItemCategoryId != 0 && itemWiseReportInput.InvoiceStatus != "")
                    {
                        if (itemWiseReportInput.StoreId > 0)
                        {
                            cmd.Parameters.AddWithValue("@STORE_ID", itemWiseReportInput.StoreId);
                        }

                        if (itemWiseReportInput.ItemCategoryId > 0)
                        {
                            cmd.Parameters.AddWithValue("@ITEM_CATEGORY_ID", itemWiseReportInput.ItemCategoryId);
                        }

                        if (!itemWiseReportInput.FromDate.Equals(DBNull.Value) && !itemWiseReportInput.ToDate.Equals(DBNull.Value))
                        {
                            cmd.Parameters.AddWithValue("@FROM_DATE", itemWiseReportInput.FromDate);
                            cmd.Parameters.AddWithValue("@TO_DATE", itemWiseReportInput.ToDate);
                        }

                        if (itemWiseReportInput.InvoiceStatus != "")
                        {
                            cmd.Parameters.AddWithValue("@PAID_STATUS", itemWiseReportInput.InvoiceStatus);
                        }
                    }

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {                        
                        while (dr.Read())
                        {
                            Models.Reports.ItemWiseReportGeneration.Response.ItemWiseReportOutput itemWiseReportOutput = new Models.Reports.ItemWiseReportGeneration.Response.ItemWiseReportOutput();

                            itemWiseReportOutput.UserId = Convert.ToInt32(dr["USER_ID"]);
                            itemWiseReportOutput.Address = Convert.ToString(dr["ADDRESS"]);
                            itemWiseReportOutput.InvoiceId = Convert.ToInt32(dr["ID"]);
                            itemWiseReportOutput.Mobile = Convert.ToString(dr["MOBILE"]);
                            itemWiseReportOutput.RecordDate = Convert.ToDateTime(dr["RECORD_DATE"]);
                            itemWiseReportOutput.GrossTotal = Convert.ToDouble(dr["GROSS_TOTAL"]);
                            itemWiseReportOutput.Tax = Convert.ToDouble(dr["TAX"]);
                            itemWiseReportOutput.NetTotal = Convert.ToDouble(dr["NET_TOTAL"]);
                            itemWiseReportOutput.PaidStatus = Convert.ToString(dr["PAID_STATUS"]);

                            tempItemWiseReportOutputList.Add(itemWiseReportOutput);

                            //Soumee - 29-March-2018 - Adding Loggers        
                            Logger.Write(logIdentifier, "fetching invoice details inside the reader", DateTime.Now.Ticks, LoggerConstants.REPORTS__ITEM_WISE_REPORT_GENERATION);
                        }

                        if (tempItemWiseReportOutputList.Count > 0)
                        {
                            fetchItemsByInvoiceId += " AND IND.ID = @INVOICE_ID";
                        }
                    }
                }

                foreach (Models.Reports.ItemWiseReportGeneration.Response.ItemWiseReportOutput itemReportList in tempItemWiseReportOutputList)
                {
                    Models.Reports.ItemWiseReportGeneration.Response.ItemWiseReportOutput itemReportOutput = new Models.Reports.ItemWiseReportGeneration.Response.ItemWiseReportOutput();

                    using (SqlCommand cmd = new SqlCommand(fetchItemsByInvoiceId, con))
                    {
                        if (itemReportList.InvoiceId > 0)
                        {
                            cmd.Parameters.AddWithValue("@INVOICE_ID", itemReportList.InvoiceId);
                        }

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            List<Models.Reports.ItemWiseReportGeneration.Core.ItemDetailsReportOutput> itemDetailsReportOutputList = new List<Models.Reports.ItemWiseReportGeneration.Core.ItemDetailsReportOutput>();

                            while (dr.Read())
                            {
                                double multipliedAmount = 0;
                                Models.Reports.ItemWiseReportGeneration.Core.ItemDetailsReportOutput itemDetailsReportOutput = new Models.Reports.ItemWiseReportGeneration.Core.ItemDetailsReportOutput();

                                itemDetailsReportOutput.StoreId = Convert.ToInt32(dr["ID"]);
                                itemDetailsReportOutput.ItemName = Convert.ToString(dr["NAME"]);
                                itemDetailsReportOutput.AvailableQuantity = Convert.ToInt32(dr["QUANTITY_AVAILABLE"]);
                                itemDetailsReportOutput.SoldQuantity = Convert.ToInt32(dr["QUANTITY_SOLD"]);
                                multipliedAmount = Convert.ToDouble(dr["AMOUNT_COLLECTED"]);
                                itemDetailsReportOutput.ItemTax = Convert.ToDouble(dr["TAX"]);
                                itemDetailsReportOutput.AmountCollected = multipliedAmount + (multipliedAmount * itemDetailsReportOutput.ItemTax);

                                itemDetailsReportOutputList.Add(itemDetailsReportOutput);

                                //Soumee - 29-March-2018 - Adding Loggers        
                                Logger.Write(logIdentifier, "fetching items for an invoice inside the reader", DateTime.Now.Ticks, LoggerConstants.REPORTS__ITEM_WISE_REPORT_GENERATION);
                            }
                            itemReportOutput.itemDetailsReportOutputs = itemDetailsReportOutputList;
                        }
                    }
                    itemReportOutput.UserId = itemReportList.UserId;
                    itemReportOutput.Address = itemReportList.Address;
                    itemReportOutput.InvoiceId = itemReportList.InvoiceId;
                    itemReportOutput.Mobile = itemReportList.Mobile;
                    itemReportOutput.RecordDate = itemReportList.RecordDate;
                    itemReportOutput.GrossTotal = itemReportList.GrossTotal;
                    itemReportOutput.Tax = itemReportList.Tax;
                    itemReportOutput.NetTotal = itemReportList.NetTotal;
                    itemReportOutput.PaidStatus = itemReportList.PaidStatus;

                    itemWiseReportOutputList.Add(itemReportOutput);

                    //Soumee - 29-March-2018 - Adding Loggers        
                    Logger.Write(logIdentifier, "adding the items fetched to a list", DateTime.Now.Ticks, LoggerConstants.REPORTS__ITEM_WISE_REPORT_GENERATION);
                }
            }

            if(itemWiseReportOutputList.Count > 0)
            {
                itemWiseReportOutputListResponse.IsOk = true;
                itemWiseReportOutputListResponse.Message = "Item wise report";
                itemWiseReportOutputListResponse.ResponseObject = itemWiseReportOutputList;

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, itemWiseReportInput.ToString(), DateTime.Now.Ticks, LoggerConstants.REPORTS__ITEM_WISE_REPORT_GENERATION);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.REPORTS__ITEM_WISE_REPORT_GENERATION);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            else
            {
                itemWiseReportOutputListResponse.Message = "No item wise reports to fetch";

                //+Soumee - 29-March-2018 - Adding Loggers
                Logger.Write(logIdentifier, itemWiseReportInput.ToString(), DateTime.Now.Ticks, LoggerConstants.REPORTS__ITEM_WISE_REPORT_GENERATION);
                Logger.Write(logIdentifier, classAndMethodName + " END", DateTime.Now.Ticks, LoggerConstants.REPORTS__ITEM_WISE_REPORT_GENERATION);
                //-Soumee - 29-March-2018 - Adding Loggers
            }
            return Ok(itemWiseReportOutputListResponse);
        }
    }
}

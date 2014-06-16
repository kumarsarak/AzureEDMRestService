using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Http.Description;
using EDMApiWebRole.Models;
using System.Diagnostics;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace EDMApiWebRole.Controllers
{
    
    public class APInvoiceController : ApiController
    {
        private APAppDBContext db = new APAppDBContext();

        
        /*public HttpResponseMessage GetAPInvoice(string recordnumber)
        {
            try
            {
                if (!String.IsNullOrEmpty(recordnumber) && db.APInvoices.Single(x => x.Record_Number == recordnumber) != null)
                {
                    
                   var apinvoicerecord = db.APInvoices.Single(x => x.Record_Number == recordnumber);
                    return Request.CreateResponse<APInvoice>(HttpStatusCode.OK, apinvoicerecord);
                }
                else
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                

            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }


        }*/

        [System.Web.Http.ActionName("Metadata")]
        public HttpResponseMessage PostAPInvoice(string recordnumber, string invoicedate, string invoicenumber, string vendornumber, string vendorname, string ponumber, string invoicetypecd)
        {
            try
            {
                if (!String.IsNullOrEmpty(recordnumber) && !String.IsNullOrEmpty(invoicenumber))
                {
                    if (db.APInvoices.Count(x => x.Record_Number == recordnumber) > 0) { return Request.CreateResponse(HttpStatusCode.Conflict); }
                    var apinvoicerecord = new APInvoice { Record_Number = recordnumber, Invoice_Date = Convert.ToDateTime(invoicedate), Invoice_Number = invoicenumber, Vendor_Number = vendornumber, Vendor_Name = vendorname, PO_Number = ponumber, Invoice_Type_cd = invoicetypecd };
                    db.APInvoices.Add(apinvoicerecord);
                    db.SaveChanges();
                    
                    return Request.CreateResponse<APInvoice>(HttpStatusCode.Created, apinvoicerecord);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }

            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
            
            
        }

        [System.Web.Http.ActionName("Metadata")]
        public HttpResponseMessage DeleteAPInvoice(string recordnumber)
        {
            try
            {
                if (!String.IsNullOrEmpty(recordnumber) && db.APInvoices.Single(x => x.Record_Number == recordnumber) != null)
                {
                    db.APInvoices.Remove(db.APInvoices.Single(x => x.Record_Number == recordnumber));
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    if (String.IsNullOrEmpty(recordnumber))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        /* [System.Web.Http.ActionName("UpdateMetadata")]
       public HttpResponseMessage PutAPInvoice(string recordnumber, string invoicedate, string invoicenumber, string vendornumber, string vendorname, string ponumber, string invoicetypecd)
        {
            try
            {
                if (!String.IsNullOrEmpty(recordnumber))
                {
                    var apinvoicerecord = db.APInvoices.Single(x => x.Record_Number == recordnumber);
                    if (apinvoicerecord != null)
                    {

                        apinvoicerecord.Invoice_Date = (!String.IsNullOrEmpty(invoicedate)) ? Convert.ToDateTime(invoicedate) : apinvoicerecord.Invoice_Date;
                        apinvoicerecord.Invoice_Number = (!String.IsNullOrEmpty(invoicenumber)) ? invoicenumber : apinvoicerecord.Invoice_Number;
                        apinvoicerecord.Vendor_Number = (!String.IsNullOrEmpty(vendornumber)) ? vendornumber : apinvoicerecord.Vendor_Number;
                        apinvoicerecord.Vendor_Name = (!String.IsNullOrEmpty(vendorname)) ? vendorname : apinvoicerecord.Vendor_Name;
                        apinvoicerecord.PO_Number = (!String.IsNullOrEmpty(ponumber)) ? ponumber : apinvoicerecord.PO_Number;
                        apinvoicerecord.Invoice_Type_cd = (!String.IsNullOrEmpty(invoicetypecd)) ? invoicetypecd : apinvoicerecord.Invoice_Type_cd;
                    }
                    DeleteMetadata(recordnumber);
                    db.APInvoices.Add(apinvoicerecord);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                }
                 
            }
            catch (Exception ex)
            {
                DeleteMetadata(recordnumber);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }*/

        [System.Web.Http.ActionName("Image")]
        public HttpResponseMessage PostAPInvoiceImage(string recordnumber, string filepath)
        {
            try
            {
                if (!String.IsNullOrEmpty(recordnumber) && (!String.IsNullOrEmpty(filepath)))
                {
                    if (GetBlockBlob(recordnumber).Exists())
                    {
                        return Request.CreateResponse(HttpStatusCode.Conflict);
                    }

                    // Retrieve reference to a blob named "blobName".
                    CloudBlockBlob blockBlob = GetBlockBlob(recordnumber);

                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(filepath);

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (filepath.ToLower().EndsWith(".pdf"))
                        {
                            blockBlob.Properties.ContentType = "application/pdf";
                        }
                        else if (filepath.ToLower().EndsWith(".tif"))
                        {
                            blockBlob.Properties.ContentType = "image/tiff";
                        }
                        else if (filepath.ToLower().EndsWith(".png"))
                        {
                            blockBlob.Properties.ContentType = "image/png";
                        }
                        else if (filepath.ToLower().EndsWith(".jpg"))
                        {
                            blockBlob.Properties.ContentType = "image/jpeg";
                        }

                        blockBlob.UploadFromStream(response.GetResponseStream());
                    }

                    return Request.CreateResponse(HttpStatusCode.Created);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }

            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

        }


        [System.Web.Http.ActionName("Image")]
        public HttpResponseMessage DeleteImage(string recordnumber)
        {
            try
            {
                if (!String.IsNullOrEmpty(recordnumber) && GetBlockBlob(recordnumber).Exists())
                {
                    CloudBlockBlob blockBlob = GetBlockBlob(recordnumber);
                    blockBlob.Delete();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    if (String.IsNullOrEmpty(recordnumber))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

        }

        private CloudBlockBlob GetBlockBlob(string recordnumber)
        {
            // Retrieve AP storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.ConnectionStrings["APStorageConnectionString"].ConnectionString);
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            String containerName = ConfigurationManager.ConnectionStrings["APContainerConnectionString"].ConnectionString;

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            return container.GetBlockBlobReference(recordnumber);
        }
    }
}

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
using System.Web.Mvc.Async;
using System.Threading.Tasks;

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
                if (!String.IsNullOrEmpty(recordnumber))
                {
                    var apinvoices = from m in db.APInvoices where (m.Record_Number == recordnumber) select m;
                    if (apinvoices.Count() > 0)
                    {
                        db.APInvoices.Remove(db.APInvoices.Single(x => x.Record_Number == recordnumber));
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                    }
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
        public async Task<HttpResponseMessage> PostAPInvoiceImage(string recordnumber, string fileformat)
        {
            try
            {
                if (!String.IsNullOrEmpty(recordnumber))
                {
                HttpContent requestContent = Request.Content;
                using (var streamimage = requestContent.ReadAsStreamAsync())
                {
                    int maxSize = 64 * 1024 * 1024; // 64 MB
                    if (streamimage.Result.Length >= maxSize)
                    {
                        UploadBigImage(requestContent.ReadAsByteArrayAsync().Result, recordnumber, fileformat);
                    }

                    else
                    {
                        await UploadImage(streamimage.Result, recordnumber, fileformat); 
                    }
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

        private async Task UploadImage(System.IO.Stream streamimage, string recordnumber, string fileformat)
        {
            try
            {
                CloudBlockBlob blockBlob = GetBlockBlob(recordnumber);
                blockBlob.Properties.ContentType = GetBlockBlobContentType(fileformat);
                await blockBlob.UploadFromStreamAsync(streamimage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UploadBigImage(byte[] data, string recordnumber, string fileformat)
        {
            try
            {
                CloudBlockBlob blockBlob = GetBlockBlob(recordnumber);
                blockBlob.Properties.ContentType = GetBlockBlobContentType(fileformat);
                int id = 0;
                int byteslength = data.Length;
                int bytesread = 0;
                int index = 0;
                List<string> blocklist = new List<string>();
                int numBytesPerChunk = 250 * 1024; //250KB per block

                do
                {
                    byte[] buffer = new byte[numBytesPerChunk];
                    int limit = index + numBytesPerChunk;
                    for (int loops = 0; index < limit; index++)
                    {
                        buffer[loops] = data[index];
                        loops++;
                    }
                    bytesread = index;
                    string blockIdBase64 = Convert.ToBase64String(System.BitConverter.GetBytes(id));

                    blockBlob.PutBlock(blockIdBase64, new System.IO.MemoryStream(buffer, true), null);
                    blocklist.Add(blockIdBase64);
                    id++;
                } while (byteslength - bytesread > numBytesPerChunk);


                int final = byteslength - bytesread;
                byte[] finalbuffer = new byte[final];
                for (int loops = 0; index < byteslength; index++)
                {
                    finalbuffer[loops] = data[index];
                    loops++;
                }
                string blockId = Convert.ToBase64String(System.BitConverter.GetBytes(id));
                blockBlob.PutBlock(blockId, new System.IO.MemoryStream(finalbuffer, true), null);
                blocklist.Add(blockId);

                blockBlob.PutBlockList(blocklist);



            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private String GetBlockBlobContentType(string fileformat)
        {
            if (fileformat.ToLower().Contains("pdf"))
            {
                return  "application/pdf";
            }
            else if (fileformat.ToLower().Contains("tif"))
            {
                return "image/tiff";
            }
            else if (fileformat.ToLower().Contains("png"))
            {
                return "image/png";
            }
            else if (fileformat.ToLower().Contains("jpg"))
            {
                return "image/jpeg";
            }
            return null;
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
                if (ex.Message.Contains("Microsoft.Data.Services.Client"))
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                else
                {
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);
                }
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
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

        [System.Web.Http.HttpPost]
        public String Create(string recordnumber, string invoicedate, string invoicenumber, string vendornumber, string vendorname, string ponumber, string invoicetypecd, string filelocation, string invoiceimagename)
        {
            String returnvalue;
            bool insertsuccess = false;
            try
            {
                if (!String.IsNullOrEmpty(recordnumber) && !String.IsNullOrEmpty(invoicenumber))
                {
                    var apinvoicerecord = new APInvoice { Record_Number = recordnumber, Invoice_Date = Convert.ToDateTime(invoicedate), Invoice_Number = invoicenumber, Vendor_Number = vendornumber, Vendor_Name = vendorname, PO_Number = ponumber, Invoice_Type_cd = invoicetypecd };
                    db.APInvoices.Add(apinvoicerecord);
                    db.SaveChanges();
                    insertsuccess = true;
                }
                returnvalue = "AP MetaData succesfully Added;";

            }
            catch (Exception ex)
            {
                DeleteMetadata(recordnumber);
                returnvalue = "AP Metadata not saved due to exceptions";
                return returnvalue;
            }

            returnvalue = returnvalue + InsertInvoiceIntoBlob(recordnumber, filelocation, invoiceimagename, insertsuccess);
            return returnvalue;
        }

        [System.Web.Http.HttpPost]
        public String Update(string recordnumber, string invoicedate, string invoicenumber, string vendornumber, string vendorname, string ponumber, string invoicetypecd, string filelocation, string invoiceimagename)
        {
            String returnvalue;
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
                    returnvalue = "AP MetaData succesfully Updated";
                }
                else
                {
                    returnvalue = "AP MetaData not Updated as the recordnumber is empty";

                }
                 
            }
            catch (Exception ex)
            {
                DeleteMetadata(recordnumber);
                returnvalue = "AP Metadata not updated due to exceptions";
                return returnvalue;
            }

            return returnvalue;

        }

        private String InsertInvoiceIntoBlob(string recordnumber, string filelocation, string invoiceimagename, bool insertsuccess)
        {
            try
            {
                
                if (insertsuccess)
                {
                    // Retrieve reference to a blob named "blobName".
                    CloudBlockBlob blockBlob = GetBlockBlob(recordnumber);
                    // Create or overwrite the "blobName" blob with contents from a local file.
                    using (var fileStream = System.IO.File.OpenRead(filelocation))
                    {
                        // Set content type accordingly
                        if (invoiceimagename.ToLower().EndsWith(".pdf"))
                        {
                            blockBlob.Properties.ContentType = "application/pdf";
                        }
                        else if (invoiceimagename.ToLower().EndsWith(".tif"))
                        {
                            blockBlob.Properties.ContentType = "image/tiff";
                        }
                        //Upload the image
                        blockBlob.UploadFromStream(fileStream);
                    }

                    return "AP Invoice Image successfully saved";
                }
                else
                {
                    return "AP Invoice Image not Saved due to errors in Metadata";
                }
            }
            catch (Exception ex)
            {
                DeleteMetadata(recordnumber);
                DeleteImage(recordnumber);
                return "AP Invoice Image and Metadata not saved due to exceptions";
            }
            
        }

        private void DeleteMetadata(string recordnumber)
        {
            if (!String.IsNullOrEmpty(db.APInvoices.Single(x => x.Record_Number == recordnumber).Record_Number))
            {
                db.APInvoices.Remove(db.APInvoices.Single(x => x.Record_Number == recordnumber));
                db.SaveChanges();
            }

        }

        private void DeleteImage(string recordnumber)
        {
            CloudBlockBlob blockBlob = GetBlockBlob(recordnumber);
            blockBlob.Delete();

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

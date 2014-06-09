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
    public class ARCustRemitController : ApiController
    {
        private ARAppDBContext db = new ARAppDBContext();

        [System.Web.Http.HttpPost]
        public String Create(string recordnumber, string checkdepositdate, string checkserialnum, string checktransitnum, string checkaccountnum, string lockbox, string hasactionitem, string lawsoncustomer, string filelocation, string checkimagename)
        {
            String returnvalue;
            bool insertsuccess = false;
            try
            {
                if (!String.IsNullOrEmpty(recordnumber) && !String.IsNullOrEmpty(checkaccountnum))
                {
                    var arcustomerremitrecord = new ARCustRemit { Record_Number = recordnumber, Chk_Deposit_Dt = Convert.ToDateTime(checkdepositdate), Chk_Serial_Num = checkserialnum, Chk_Transit_Num = checktransitnum, Chk_Account_Num = checkaccountnum, Lockbox = lockbox, Has_Action_Item = hasactionitem, Lawson_Customer = lawsoncustomer };
                    db.ARCustRemits.Add(arcustomerremitrecord);
                    db.SaveChanges();
                    insertsuccess = true;
                }
                returnvalue = "AR MetaData succesfully Added;";

            }
            catch (Exception ex)
            {
                DeleteMetadata(recordnumber);
                returnvalue = "AR Metadata not saved due to exceptions";
                return returnvalue;
            }

            returnvalue = returnvalue + InsertCheckIntoBlob(recordnumber, filelocation, checkimagename, insertsuccess);
            return returnvalue;
        }

        [System.Web.Http.HttpPost]
        public String Update(string recordnumber, string checkdepositdate, string checkserialnum, string checktransitnum, string checkaccountnum, string lockbox, string hasactionitem, string lawsoncustomer)
        {
            String returnvalue;
            try
            {
                if (!String.IsNullOrEmpty(recordnumber))
                {
                    var arcustomerremitrecord = db.ARCustRemits.Single(x => x.Record_Number == recordnumber);
                    if (arcustomerremitrecord != null)
                    {

                        arcustomerremitrecord.Chk_Deposit_Dt = (!String.IsNullOrEmpty(checkdepositdate)) ? Convert.ToDateTime(checkdepositdate) : arcustomerremitrecord.Chk_Deposit_Dt;
                        arcustomerremitrecord.Chk_Serial_Num = (!String.IsNullOrEmpty(checkserialnum)) ? checkserialnum : arcustomerremitrecord.Chk_Serial_Num;
                        arcustomerremitrecord.Chk_Transit_Num = (!String.IsNullOrEmpty(checktransitnum)) ? checktransitnum : arcustomerremitrecord.Chk_Transit_Num;
                        arcustomerremitrecord.Chk_Account_Num = (!String.IsNullOrEmpty(checkaccountnum)) ? checkaccountnum : arcustomerremitrecord.Chk_Account_Num;
                        arcustomerremitrecord.Lockbox = (!String.IsNullOrEmpty(lockbox)) ? lockbox : arcustomerremitrecord.Lockbox;
                        arcustomerremitrecord.Has_Action_Item = (!String.IsNullOrEmpty(hasactionitem)) ? hasactionitem : arcustomerremitrecord.Has_Action_Item;
                        arcustomerremitrecord.Lawson_Customer = (!String.IsNullOrEmpty(lawsoncustomer)) ? lawsoncustomer : arcustomerremitrecord.Lawson_Customer;
                    }
                    DeleteMetadata(recordnumber);
                    db.ARCustRemits.Add(arcustomerremitrecord);
                    db.SaveChanges();
                    returnvalue = "AR MetaData succesfully Updated";
                }
                else
                {
                    returnvalue = "AR MetaData not Updated as the recordnumber is empty";

                }

            }
            catch (Exception ex)
            {
                DeleteMetadata(recordnumber);
                returnvalue = "AR Metadata not updated due to exceptions";
                return returnvalue;
            }

            return returnvalue;

        }

        private String InsertCheckIntoBlob(string recordnumber, string filelocation, string checkimagename, bool insertsuccess)
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
                        if (checkimagename.ToLower().EndsWith(".pdf"))
                        {
                            blockBlob.Properties.ContentType = "application/pdf";
                        }
                        else if (checkimagename.ToLower().EndsWith(".tif"))
                        {
                            blockBlob.Properties.ContentType = "image/tiff";
                        }
                        //Upload the image
                        blockBlob.UploadFromStream(fileStream);
                    }

                    return "AR Customer Remit Image successfully saved";
                }
                else
                {
                    return "AR Customer Remit Image not Saved due to errors in Metadata";
                }
            }
            catch (Exception ex)
            {
                DeleteMetadata(recordnumber);
                DeleteImage(recordnumber);
                return "AR Customer Remit Image and Metadata not saved due to exceptions";
            }

        }

        private void DeleteMetadata(string recordnumber)
        {
            if (!String.IsNullOrEmpty(db.ARCustRemits.Single(x => x.Record_Number == recordnumber).Record_Number))
            {
                db.ARCustRemits.Remove(db.ARCustRemits.Single(x => x.Record_Number == recordnumber));
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
                ConfigurationManager.ConnectionStrings["ARStorageConnectionString"].ConnectionString);
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            String containerName = ConfigurationManager.ConnectionStrings["ARContainerConnectionString"].ConnectionString;

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            return container.GetBlockBlobReference(recordnumber);
        }
    }
}

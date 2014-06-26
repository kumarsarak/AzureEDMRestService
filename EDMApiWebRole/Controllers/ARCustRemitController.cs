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

        [System.Web.Http.ActionName("Metadata")]
        public HttpResponseMessage PostARCustRemit(string recordnumber, string checkdepositdate, string checkserialnum, string checktransitnum, string checkaccountnum, string lockbox, string hasactionitem, string lawsoncustomer)
        {
            
            try
            {
                if (!String.IsNullOrEmpty(recordnumber) && !String.IsNullOrEmpty(checkaccountnum))
                {
                    if (db.ARCustRemits.Count(x => x.Record_Number == recordnumber) > 0) { return Request.CreateResponse(HttpStatusCode.Conflict); }
                    var arcustomerremitrecord = new ARCustRemit { Record_Number = recordnumber, Chk_Deposit_Dt = Convert.ToDateTime(checkdepositdate), Chk_Serial_Num = checkserialnum, Chk_Transit_Num = checktransitnum, Chk_Account_Num = checkaccountnum, Lockbox = lockbox, Has_Action_Item = hasactionitem, Lawson_Customer = lawsoncustomer };
                    db.ARCustRemits.Add(arcustomerremitrecord);
                    db.SaveChanges();
                    return Request.CreateResponse<ARCustRemit>(HttpStatusCode.Created, arcustomerremitrecord);
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
        public HttpResponseMessage DeleteARCustRemit(string recordnumber)
        {
            try
            {
                if (!String.IsNullOrEmpty(recordnumber) && db.ARCustRemits.Single(x => x.Record_Number == recordnumber) != null)
                {
                    db.ARCustRemits.Remove(db.ARCustRemits.Single(x => x.Record_Number == recordnumber));
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

        /*[System.Web.Http.ActionName("UpdateMetadata")]
        public HttpResponseMessage PutARCustRemit(string recordnumber, string checkdepositdate, string checkserialnum, string checktransitnum, string checkaccountnum, string lockbox, string hasactionitem, string lawsoncustomer)
        {
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
        public HttpResponseMessage PostARCustRemitImage(string recordnumber, string fileformat)
        {
            try
            {
                if (!String.IsNullOrEmpty(recordnumber))
                {
                    /*if (GetBlockBlob(recordnumber).Exists())
                    {
                        return Request.CreateResponse(HttpStatusCode.Conflict);
                    }*/

                    // Retrieve reference to a blob named "blobName".
                    CloudBlockBlob blockBlob = GetBlockBlob(recordnumber);
                    HttpContent requestContent = Request.Content;
                    var streamimage = requestContent.ReadAsStreamAsync().Result;


                    if (fileformat.ToLower().Contains("pdf"))
                    {
                        blockBlob.Properties.ContentType = "application/pdf";
                    }
                    else if (fileformat.ToLower().Contains("tif"))
                    {
                        blockBlob.Properties.ContentType = "image/tiff";
                    }
                    else if (fileformat.ToLower().Contains("png"))
                    {
                        blockBlob.Properties.ContentType = "image/png";
                    }
                    else if (fileformat.ToLower().Contains("jpg"))
                    {
                        blockBlob.Properties.ContentType = "image/jpeg";
                    }


                    blockBlob.UploadFromStream(streamimage);
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

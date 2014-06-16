using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using EDMApiWebRole.Models;

namespace EDMApiWebRole
{
 public class TokenValidationAttribute : ActionFilterAttribute
 {
      private string key = "token";
      public override void OnActionExecuting(HttpActionContext actionContext)
      {

           string token;
           try
           {
            token = actionContext.Request.Headers.GetValues("Authorization-Token").First();
           }
           catch (Exception ex)
           {
            actionContext.Response   = 
              new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) 
              { 
                Content = new StringContent("Missing Authorization-Token") 
              };
            return;
           }

           try
           {
               if (AuthorizeToken.GetToken(key) == RSAClass.Decrypt(token))
               {
                   base.OnActionExecuting(actionContext);
               }
               else
               {
                   actionContext.Response =
                      new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                      {
                          Content = new StringContent("Unauthorized User")
                      };
                   return;
               }
           }
           catch (Exception ex)
           {

            actionContext.Response =
              new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
              {
               Content = new StringContent("Unauthorized User")
              };
            return;
          }
     }
  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EDMApiWebRole.Models;

namespace EDMApiWebRole
{
    public static class AuthorizeToken
    {
        public static string GetToken(string parameter)
        {
            using (AuthorizeDBContext db = new AuthorizeDBContext())
            {
                var returnvalue = String.Empty;

                if (!String.IsNullOrEmpty(parameter))
                {
                    returnvalue = db.Authorizes.Single(x => x.Key == parameter).Value;
                }

                return returnvalue;

            }
        }
    }
}
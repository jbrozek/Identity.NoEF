using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace Identity.Web.Controllers
{
    public class BaseController : Controller
    {
        public Guid UserId
        {
            get
            {
                if (User.Identity.IsAuthenticated)
                    return Guid.Parse(User.Identity.GetUserId());

                return Guid.Empty;
            }
        }
    }
}
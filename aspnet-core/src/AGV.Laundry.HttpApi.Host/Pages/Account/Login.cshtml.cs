using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Volo.Abp.Account.Web.Pages.Account;
using Microsoft.Extensions.Options;

namespace AGV.Laundry.Pages.Account
{
    [IgnoreAntiforgeryToken(Order = 2000)]
    public class CustomLoginModel : LoginModel
    {
        private readonly IConfiguration _configuration;

        public CustomLoginModel(
        Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider schemeProvider,
        Microsoft.Extensions.Options.IOptions<Volo.Abp.Account.Web.AbpAccountOptions> accountOptions,
        Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Identity.IdentityOptions> identityOptions,
        IConfiguration configuration)
            : base(schemeProvider, accountOptions, identityOptions)
        {
            _configuration = configuration;
        }
    }
}

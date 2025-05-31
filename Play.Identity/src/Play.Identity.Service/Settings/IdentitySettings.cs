using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Identity.Service.Settings
{
    public class IdentitySettings
    {
        public string AdminUserEmail { get; init; }
        public string AdminUserPassword { get; init; }

        public decimal StartingGil { get; init; }
        public string PathBase { get; init; }
        public string CertificateCrtFilePath { get; init; }
        public string CertificateKeyFilePath { get; init; }
    }
}
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace NuGetGallery
{
    public interface ICertificateService
    {
        /// <summary>
        /// Add a certificate to the database if the certificate does not already exist.
        /// </summary>
        /// <param name="file">The certificate file.</param>
        /// <returns>A certificate entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="file" /> is <c>null</c>.</exception>
        Task<Certificate> AddCertificateAsync(HttpPostedFileBase file);

        Task ActivateCertificateAsync(string thumbprint, User account);

        Task DeactivateCertificateAsync(string thumbprint, User account);

        IEnumerable<Certificate> GetActiveCertificates(User account);
    }
}
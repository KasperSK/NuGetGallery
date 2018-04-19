// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGetGallery
{
    public class CertificatesListViewModel
    {
        public IEnumerable<ListCertificateItemViewModel> Certificates { get; }

        public CertificatesListViewModel(IEnumerable<Certificate> certificates, User user)
        {
            if (certificates == null)
            {
                throw new ArgumentNullException(nameof(certificates));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            Certificates = Enumerable.Empty<ListCertificateItemViewModel>();
        }
    }
}
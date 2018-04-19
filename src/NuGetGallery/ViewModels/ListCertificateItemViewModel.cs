// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGetGallery
{
    public sealed class ListCertificateItemViewModel
    {
        public string Sha1Thumbprint { get; }
        public bool CanDeactivate { get; }
        public string DeactivateUrl { get; }

        public ListCertificateItemViewModel(Certificate certificate, string deactivateUrl)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

            Sha1Thumbprint = certificate.Sha1Thumbprint;
            CanDeactivate = !string.IsNullOrEmpty(deactivateUrl);
            DeactivateUrl = deactivateUrl;
        }
    }
}
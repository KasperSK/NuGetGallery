// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace NuGetGallery.ViewModels
{
    public class ListCertificateItemViewModelFacts
    {
        [Fact]
        public void Constructor_WhenCertificateIsNull_Throws()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ListCertificateItemViewModel(certificate: null, deactivateUrl: "a"));

            Assert.Equal("certificate", exception.ParamName);
        }

        [Theory]
        [InlineData("a", "b")]
        [InlineData("c", null)]
        [InlineData("d", "")]
        public void Constructor_InitializesProperties(string sha1Thumbprint, string deactivateUrl)
        {
            var certificate = new Certificate() { Sha1Thumbprint = sha1Thumbprint };
            var viewModel = new ListCertificateItemViewModel(certificate, deactivateUrl);

            Assert.Equal(sha1Thumbprint, viewModel.Sha1Thumbprint);
            Assert.Equal(deactivateUrl, viewModel.DeactivateUrl);
            Assert.Equal(!string.IsNullOrEmpty(deactivateUrl), viewModel.CanDeactivate);
        }
    }
}
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace NuGetGallery.ViewModels
{
    public class SignerViewModelFacts
    {
        [Theory]
        [InlineData(3, "a", "b")]
        [InlineData(null, null, null)]
        public void Constructor_InitializesProperties(int? id, string username, string displayText)
        {
            var viewModel = new SignerViewModel(id, username, displayText);

            Assert.Equal(id, viewModel.Id);
            Assert.Equal(username, viewModel.Username);
            Assert.Equal(displayText, viewModel.DisplayText);
        }
    }
}
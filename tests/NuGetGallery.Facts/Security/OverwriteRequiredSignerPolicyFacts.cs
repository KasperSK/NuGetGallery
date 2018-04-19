// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NuGetGallery.Security
{
    public class OverwriteRequiredSignerPolicyFacts
    {
        [Fact]
        public void PolicyName_IsTypeName()
        {
            Assert.Equal(nameof(OverwriteRequiredSignerPolicy), OverwriteRequiredSignerPolicy.PolicyName);
        }

        [Fact]
        public void Constructor_InitializesProperties()
        {
            var policy = new OverwriteRequiredSignerPolicy();

            Assert.Equal(nameof(OverwriteRequiredSignerPolicy), policy.Name);
            Assert.Equal(nameof(OverwriteRequiredSignerPolicy), policy.SubscriptionName);
            Assert.Equal(SecurityPolicyAction.OverwriteRequiredSigner, policy.Action);
            Assert.Equal(1, policy.Policies.Count());
            Assert.Equal(nameof(OverwriteRequiredSignerPolicy), policy.Policies.Single().Name);
            Assert.Equal(nameof(OverwriteRequiredSignerPolicy), policy.Policies.Single().Subscription);
        }

        [Fact]
        public void Evaluate_Throws()
        {
            var policy = new OverwriteRequiredSignerPolicy();

            Assert.Throws<NotImplementedException>(() => policy.Evaluate(context: null));
        }

        [Fact]
        public void OnSubscribeAsync_ReturnsCompletedTask()
        {
            var policy = new OverwriteRequiredSignerPolicy();

            var task = policy.OnSubscribeAsync(context: null);

            Assert.Same(Task.CompletedTask, task);
        }

        [Fact]
        public void OnUnsubscribeAsync_ReturnsCompletedTask()
        {
            var policy = new OverwriteRequiredSignerPolicy();

            var task = policy.OnSubscribeAsync(context: null);

            Assert.Same(Task.CompletedTask, task);
        }
    }
}
﻿using NUnit.Framework;
using System;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;

namespace WebApi.OutputCache.V2.Tests
{
    /// <summary>
    /// Base class for implementing tests for the generation of cache keys (meaning: implementations of the <see cref="ICacheKeyGenerator"/>
    /// </summary>
    public abstract class CacheKeyGenerationTestsBase<TCacheKeyGenerator> where TCacheKeyGenerator : ICacheKeyGenerator
    {
        private const string Argument1Key = "filterExpression1";
        private const string Argument1Value = "val";
        private const string Argument2WithFromUriAttributeKey = "filterExpression2";
        private static readonly ClassArgument Argument2WithFromUriAttributeValue = new ClassArgument() { Value1 = "val1", Value2 = 7 };
        protected HttpActionContext context;
        protected MediaTypeHeaderValue mediaType;
        protected Uri requestUri;
        protected TCacheKeyGenerator cacheKeyGenerator;
        protected string BaseCacheKey;

        private class ClassArgument
        {
            public string Value1 { get; set; }

            public int Value2 { get; set; }
        }

        [SetUp]
        public virtual void Setup()
        {
            requestUri = new Uri("http://localhost:8080/cacheKeyGeneration?filter=val");
            var controllerType = typeof(TestControllers.CacheKeyGenerationController);
            var actionMethodInfo = controllerType.GetMethod("Get");
            var controllerDescriptor = new HttpControllerDescriptor() { ControllerType = controllerType };
            var actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, actionMethodInfo);
            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, requestUri.AbsoluteUri);

            context = new HttpActionContext(
                new HttpControllerContext() { ControllerDescriptor = controllerDescriptor, Request = request },
                actionDescriptor
            );
            mediaType = new MediaTypeHeaderValue("application/json");

            BaseCacheKey = new CacheOutputConfiguration(null).MakeBaseCachekey((TestControllers.CacheKeyGenerationController c) => c.Get(String.Empty));
            cacheKeyGenerator = BuildCacheKeyGenerator();
        }

        protected abstract TCacheKeyGenerator BuildCacheKeyGenerator();

        protected virtual void AssertCacheKeysBasicFormat(string cacheKey)
        {
            Assert.IsNotNull(cacheKey);
            StringAssert.StartsWith(BaseCacheKey, cacheKey, "Key does not start with BaseKey");
            StringAssert.EndsWith(mediaType.ToString(), cacheKey, "Key does not end with MediaType");
        }

        protected void AddActionArgumentsToContext()
        {
            context.ActionArguments.Add(Argument1Key, Argument1Value);
            context.ActionArguments.Add(Argument2WithFromUriAttributeKey, Argument2WithFromUriAttributeValue);
        }

        protected string FormatActionArgumentsForKeyAssertion()
        {
            return String.Format("{0}={1}", Argument1Key, Argument1Value);
        }
    }
}

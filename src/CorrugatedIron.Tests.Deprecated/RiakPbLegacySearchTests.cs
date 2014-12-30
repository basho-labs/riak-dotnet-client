﻿// Copyright (c) 2010 - OJ Reeves & Jeremiah Peschka
// 
// This file is provided to you under the Apache License,
// Version 2.0 (the "License"); you may not use this file
// except in compliance with the License.  You may obtain
// a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using CorrugatedIron.Extensions;
using CorrugatedIron.Models;
using CorrugatedIron.Models.Search;
using CorrugatedIron.Tests.Extensions;
using CorrugatedIron.Tests.Live.Extensions;
using CorrugatedIron.Tests.Live.LiveRiakConnectionTests;
using CorrugatedIron.Util;
using NUnit.Framework;

namespace CorrugatedIron.Tests.Live.Deprecated
{
    [TestFixture]
    public class WhenQueryingRiakLegacySearchViaPbc : LiveRiakConnectionTestBase
    {
        private const string Bucket = "riak_search_bucket";
        private const string RiakSearchKey = "a.hacker";
        private const string RiakSearchKey2 = "a.public";
        private const string RiakSearchDoc = "{\"name\":\"Alyssa P. Hacker\", \"bio\":\"I'm an engineer, making awesome things.\", \"favorites\":{\"book\":\"The Moon is a Harsh Mistress\",\"album\":\"Magical Mystery Tour\", }}";
        private const string RiakSearchDoc2 = "{\"name\":\"Alan Q. Public\", \"bio\":\"I'm an exciting awesome mathematician\", \"favorites\":{\"book\":\"Prelude to Mathematics\",\"album\":\"The Fame Monster\"}}";

        [TestFixtureSetUp]
        public new void SetUp()
        {
            base.SetUp();

            var props = Client.GetBucketProperties(Bucket).Value;
            props.SetLegacySearch(true);
            Client.SetBucketProperties(Bucket, props);

            PrepSearchData();
        }

        private void PrepSearchData()
        {
            Func<RiakResult<RiakObject>> put1 = () => Client.Put(new RiakObject(Bucket, RiakSearchKey, RiakSearchDoc.ToRiakString(),
                                                      RiakConstants.ContentTypes.ApplicationJson, RiakConstants.CharSets.Utf8));

            var put1Result = put1.WaitUntil();

            Func<RiakResult<RiakObject>> put2 = () => Client.Put(new RiakObject(Bucket, RiakSearchKey2, RiakSearchDoc2.ToRiakString(),
                                                      RiakConstants.ContentTypes.ApplicationJson, RiakConstants.CharSets.Utf8));

            var put2Result = put2.WaitUntil();

            put1Result.IsSuccess.ShouldBeTrue(put1Result.ErrorMessage);
            put2Result.IsSuccess.ShouldBeTrue(put2Result.ErrorMessage);
        }

        [Test]
        public void SearchingWithSimpleFluentQueryWorksCorrectly()
        {
            var req = new RiakSearchRequest
            {
                Query = new RiakFluentSearch("riak_search_bucket", "name").Search("Alyssa").Build()
            };

            var result = RunSolrQuery(req).WaitUntil(AnyMatchIsFound);
            result.IsSuccess.ShouldBeTrue(result.ErrorMessage);
            result.Value.NumFound.ShouldEqual(1u);
            result.Value.Documents.Count.ShouldEqual(1);
            result.Value.Documents[0].Fields.Count.ShouldEqual(5);
            result.Value.Documents[0].Id.ShouldEqual("a.hacker");
        }

        [Test]
        public void SearchingWithWildcardFluentQueryWorksCorrectly()
        {
            var req = new RiakSearchRequest
            {
                Query = new RiakFluentSearch("riak_search_bucket", "name").Search(Token.StartsWith("Al")).Build()
            };


            var result = RunSolrQuery(req).WaitUntil(TwoMatchesFound);
            result.IsSuccess.ShouldBeTrue(result.ErrorMessage);
            result.Value.NumFound.ShouldEqual(2u);
            result.Value.Documents.Count.ShouldEqual(2);
        }

        [Test]
        public void SearchingWithMoreComplexFluentQueryWorksCorrectly()
        {
            var req = new RiakSearchRequest
            {
                Query = new RiakFluentSearch("riak_search_bucket", "bio")
            };

            req.Query.Search("awesome")
                .And("an")
                .And("mathematician", t => t.Or("favorites_ablum", "Fame"));

            var result = RunSolrQuery(req).WaitUntil(AnyMatchIsFound);
            result.IsSuccess.ShouldBeTrue(result.ErrorMessage);
            result.Value.NumFound.ShouldEqual(1u);
            result.Value.Documents.Count.ShouldEqual(1);
            result.Value.Documents[0].Fields.Count.ShouldEqual(5);
            result.Value.Documents[0].Id.ShouldEqual("a.public");
        }

        [Test]
        public void SettingFieldListReturnsOnlyFieldsSpecified()
        {
            var req = new RiakSearchRequest
            {
                Query = new RiakFluentSearch("riak_search_bucket", "bio"),
                PreSort = PreSort.Key,
                DefaultOperation = DefaultOperation.Or,
                ReturnFields = new List<string>
                {
                    "bio", "favorites_album"
                }
            };

            req.Query.Search("awesome")
                .And("an")
                .And("mathematician", t => t.Or("favorites_ablum", "Fame"));

            var result = RunSolrQuery(req).WaitUntil(AnyMatchIsFound);

            result.IsSuccess.ShouldBeTrue(result.ErrorMessage);
            result.Value.NumFound.ShouldEqual(1u);
            result.Value.Documents.Count.ShouldEqual(1);
            // "id" field is always returned
            result.Value.Documents[0].Fields.Count.ShouldEqual(3);
            result.Value.Documents[0].Id.ShouldNotBeNull();
        }

        private Func<RiakResult<RiakSearchResult>> RunSolrQuery(RiakSearchRequest req)
        {
            Func<RiakResult<RiakSearchResult>> runSolrQuery =
                () => Client.Search(req);
            return runSolrQuery;
        }

        private static Func<RiakResult<RiakSearchResult>, bool> AnyMatchIsFound
        {
            get
            {
                Func<RiakResult<RiakSearchResult>, bool> matchIsFound =
                    result => result.IsSuccess &&
                              result.Value != null &&
                              result.Value.NumFound > 0;
                return matchIsFound;
            }
        }

        private static Func<RiakResult<RiakSearchResult>, bool> TwoMatchesFound
        {
            get
            {
                Func<RiakResult<RiakSearchResult>, bool> twoMatchesFound =
                    result => result.IsSuccess &&
                              result.Value != null &&
                              result.Value.NumFound == 2;
                return twoMatchesFound;
            }
        }
    }
}

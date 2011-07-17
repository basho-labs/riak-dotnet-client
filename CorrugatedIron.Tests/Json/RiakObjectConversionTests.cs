﻿// Copyright (c) 2011 - OJ Reeves & Jeremiah Peschka
//
// This file is provided to you under the Apache License,
// Version 2.0 (the "License"); you may not use this file
// except in compliance with the License.  You may obtain
// a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using CorrugatedIron.Extensions;
using CorrugatedIron.Models;
using CorrugatedIron.Tests.Extensions;
using CorrugatedIron.Util;
using NUnit.Framework;

namespace CorrugatedIron.Tests.Json.RiakObjectConversionTests
{
    [TestFixture]
    public class WhenStoringDataIntoRiakObjectsAsJson
    {
        [Test]
        public void ObjectsAreConvertedProperly()
        {
            var testPerson = new Person
            {
                DateOfBirth = new DateTime(1978, 12, 5, 0, 0, 0, DateTimeKind.Utc),
                Email = "oj@buffered.io",
                Name = new Name
                {
                    FirstName = "OJ",
                    Surname = "Reeves"
                },
                PhoneNumbers = new List<PhoneNumber>
                {
                    new PhoneNumber
                    {
                        Number = "12345678",
                        NumberType = PhoneNumberType.Home
                    }
                }
            };
            var obj = new RiakObject("bucket", "key");
            obj.SetValue(testPerson);
            obj.Value.ShouldNotBeNull();
            obj.ContentType.ShouldEqual(RiakConstants.ContentTypes.ApplicationJson);

            var json = obj.Value.FromRiakString();
            json.ShouldEqual("{\"Name\":{\"FirstName\":\"OJ\",\"Surname\":\"Reeves\"},\"PhoneNumbers\":[{\"Number\":\"12345678\",\"NumberType\":1}],\"DateOfBirth\":\"\\/Date(281664000000)\\/\",\"Email\":\"oj@buffered.io\"}");

            var deserialisedPerson = obj.GetValue<Person>();
            deserialisedPerson.ShouldEqual(testPerson);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void NonJsonObjectsCantBeDeserialisedFromJson()
        {
            var obj = new RiakObject("bucket", "key", "{\"Name\":{\"FirstName\":\"OJ\",\"Surname\":\"Reeves\"},\"PhoneNumbers\":[{\"Number\":\"12345678\",\"NumberType\":1}],\"DateOfBirth\":\"\\/Date(281664000000)\\/\",\"Email\":\"oj@buffered.io\"}", RiakConstants.ContentTypes.TextPlain);
            obj.GetValue<Person>();
        }

        [Test]
        [Ignore("Only run this if you're interested in some perf stats for Json conversio" +
            "n")]
        public void JsonConversionTimerTest()
        {
            var testPerson = new Person
            {
                DateOfBirth = new DateTime(1978, 12, 5, 0, 0, 0, DateTimeKind.Utc),
                Email = "oj@buffered.io",
                Name = new Name
                {
                    FirstName = "OJ",
                    Surname = "Reeves"
                },
                PhoneNumbers = new List<PhoneNumber>
                {
                    new PhoneNumber
                    {
                        Number = "12345678",
                        NumberType = PhoneNumberType.Home
                    }
                }
            };
            var obj = new RiakObject("bucket", "key");

            var sw = new Stopwatch();
            sw.Start();
            const int iterations = 1000000;

            for (var i = 0; i < iterations; ++i)
            {
                obj.SetValue(testPerson);
            }
            sw.Stop();
            Console.WriteLine("Serialisation took a total of {0} - {1} per iteration", sw.Elapsed, new TimeSpan(sw.ElapsedTicks / iterations));

            sw.Reset();
            sw.Start();

            for (var i = 0; i < iterations; ++i)
            {
                var result = obj.GetValue<Person>();
            }
            sw.Stop();
            Console.WriteLine("De" +
                "serialisation took a total of {0} - {1} per iteration", sw.Elapsed, new TimeSpan(sw.ElapsedTicks / iterations));
        }
    }
}

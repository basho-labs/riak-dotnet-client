// Copyright (c) 2011 - OJ Reeves & Jeremiah Peschka
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
using CorrugatedIron.Extensions;
using Newtonsoft.Json;

namespace CorrugatedIron.Models.MapReduce.Inputs
{
    public class RiakBinIndexRangeInput : RiakIndexInput
    {
        [Obsolete("Use IndexId.BucketName instead. This will be revoved in the next version.")]
        public string Bucket
        {
            get
            {
                return IndexId != null ? IndexId.BucketName : null;
            }
        }

        [Obsolete("Use IndexId.IndexName instead. This will be revoved in the next version.")]
        public string Index
        {
            get
            {
                return IndexId != null ? IndexId.IndexName : null;
            }
        }

        public string Start { get; set; }
        public string End { get; set; }

        [Obsolete("Use the constructor that accepts a RiakIndexId instead. This will be revoved in the next version.")]
        public RiakBinIndexRangeInput(string bucket, string index, string start, string end)
            : this(new RiakIndexId(bucket, index), start, end)
        {
        }

        public RiakBinIndexRangeInput(RiakIndexId indexId, string start, string end)
        {
            IndexId = new RiakIndexId(indexId.BucketType, indexId.BucketName, indexId.IndexName.ToBinaryKey());
            Start = start;
            End = end;
        }

        public override JsonWriter WriteJson(JsonWriter writer)
        {
            WriteIndexHeaderJson(writer);

            writer.WritePropertyName("start");
            writer.WriteValue(Start);

            writer.WritePropertyName("end");
            writer.WriteValue(End);

            writer.WriteEndObject();

            return writer;
        }
    }
}


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

using CorrugatedIron.Util;

namespace CorrugatedIron.Models
{
    public class RiakIndexId
    {
        public string BucketType { get; private set; }
        public string BucketName { get; private set; }
        public string IndexName { get; private set; }

        public RiakIndexId()
        {
        }

        public RiakIndexId(string bucketName, string indexName) : this (null, bucketName, indexName) { }

        public RiakIndexId(string bucketType, string bucketName, string indexName)
        {
            BucketType = bucketType;
            BucketName = bucketName;
            IndexName = indexName;
        } 

        public RiakIndexId SetBucketType(string bucketType) 
        {
            BucketType = bucketType;

            return this;
        }

        public RiakIndexId SetBucketName(string bucketName)
        {
            BucketName = bucketName;

            return this;
        }

        public RiakIndexId SetIndexName(string indexName)
        {
            IndexName = indexName;

            return this;
        }
    }
}


﻿namespace RiakClient.Commands.TS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Messages;
    using Util;

    /// <summary>
    /// Fetches timeseries data from Riak
    /// </summary>
    public class Put : Command<PutOptions, PutResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Put"/> class.
        /// </summary>
        /// <param name="options">Options for this operation. See <see cref="PutOptions"/></param>
        public Put(PutOptions options)
            : base(options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            if (EnumerableUtil.IsNullOrEmpty(options.Rows))
            {
                throw new ArgumentNullException("Rows", "Rows can not be null or empty");
            }
        }

        public override MessageCode ExpectedCode
        {
            get { return MessageCode.TsPutResp; }
        }

        public override RpbReq ConstructPbRequest()
        {
            var req = new TsPutReq();

            req.table = CommandOptions.Table;

            if (EnumerableUtil.NotNullOrEmpty(CommandOptions.Columns))
            {
                req.columns.AddRange(CommandOptions.Columns.Select(c => c.ToTsColumn()));
            }

            req.rows.AddRange(CommandOptions.Rows.Select(r => r.ToTsRow()));

            return req;
        }

        public override void OnSuccess(RpbResp response)
        {
            if (response == null)
            {
                Response = new PutResponse(false);
            }
            else
            {
                Response = new PutResponse();
            }
        }

        /// <inheritdoc />
        public class Builder
            : TimeseriesCommandBuilder<Builder, Put, PutOptions>
        {
            private IEnumerable<Column> columns;
            private IEnumerable<Row> rows;

            public Builder WithColumns(IEnumerable<Column> columns)
            {
                if (EnumerableUtil.IsNullOrEmpty(columns))
                {
                    throw new ArgumentNullException("columns", "columns are required");
                }

                this.columns = columns;
                return this;
            }

            public Builder WithRows(IEnumerable<Row> rows)
            {
                if (EnumerableUtil.IsNullOrEmpty(rows))
                {
                    throw new ArgumentNullException("rows", "rows are required");
                }

                this.rows = rows;
                return this;
            }

            protected override void PopulateOptions(PutOptions options)
            {
                options.Columns = columns;
                options.Rows = rows;
            }
        }
    }
}

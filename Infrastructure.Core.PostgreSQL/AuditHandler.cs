//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using Audit.Core;
//using Audit.PostgreSql.Configuration;

//using SoftwaredeveloperDotAt.Infrastructure.Core.Audit;

//namespace SoftwaredeveloperDotAt.Infrastructure.Core.PostgreSQL
//{
//    public class PostgeSQLAuditHandler : IAuditHandler
//    {
//        /*
//         * CREATE TABLE audit.event
//(
//    id bigserial NOT NULL,
//    inserted_date timestamp without time zone NOT NULL DEFAULT now(),
//    updated_date timestamp without time zone NOT NULL DEFAULT now(),
//    data jsonb NOT NULL,
//    event_type varchar(500),
//    user varchar(50) NULL,
//    CONSTRAINT event_pkey PRIMARY KEY (id)
//)
//WITH (
//    OIDS = FALSE
//)
//TABLESPACE pg_default;
//         */
//        public void RegisterProvider()
//        {
//            Configuration.Setup()
//                .UsePostgreSql(config => config
//                    .ConnectionString("User ID=postgres;Password=admin;Server=localhost;Port=5432;Database=MTGM;")
//                    .TableName("event")
//                    .Schema("audit")
//                    .IdColumnName("id")
//                    .DataColumn("data", DataType.JSONB)
//                    .LastUpdatedColumnName("updated_date")
//                    .CustomColumn("event_type", ev => ev.EventType)
//                    .CustomColumn("user", ev => ev.Environment.UserName));
//        }
//    }
//}

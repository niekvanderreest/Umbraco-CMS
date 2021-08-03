﻿using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_16_0
{
    public class AddDomainSortOrder : MigrationBase
    {
        public AddDomainSortOrder(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            if (!ColumnExists(Constants.DatabaseSchema.Tables.Domain, "sortOrder"))
            {
                AddColumn<DomainDto>("sortOrder", out var sqls);

                // Keep exising sort order by setting it to the id
                var updateSortOrder = Sql().Update<DomainDto>().Append("SET sortOrder = id");
                Execute.Sql(updateSortOrder).Do();

                // Set sort order of wildcard domains to -1
                var updateWildcardSortOrder = Sql().Update<DomainDto>(d => d.Set(f => f.SortOrder, -1)).Where("LEN(domainName) = 0 OR CHARINDEX('*', domainName) = 1");
                Execute.Sql(updateWildcardSortOrder).Do();

                // Alter the column (make it non-nullable)
                foreach (var sql in sqls) Database.Execute(sql);
            }
        }
    }
}

// Draft isolation tests below the application (AC5, ADR 0003). Owner: Prashant Sharma.
// Destination: jobwright-app, tests/Jobwright.Data.IntegrationTests/. Real PostgreSQL 17 (Testcontainers),
// migrations applied as the owner role; queries run as the application role, exactly as the API connects.
// The queries deliberately omit any tenant filter: the database alone must keep tenants apart.
// The tenant is set with set_config('jobwright.tenant_id', <id>, true) at transaction start (ADR 0003).

using Npgsql;
using Xunit;

namespace Jobwright.Data.IntegrationTests;

public sealed class RowLevelSecurityTests(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    // Every tenant-owned table of increment 1 (S12). A new tenant-owned table must be added here,
    // and TenantOwnedTablesAreAllCovered fails until it is.
    public static TheoryData<string> TenantOwnedTables => new() { "memberships", "tenant_settings", "invitations" };

    [Theory(DisplayName = "AC5 an unfiltered read returns only the current tenant's rows")]
    [MemberData(nameof(TenantOwnedTables))]
    public async Task Unfiltered_read_returns_only_current_tenant(string table)
    {
        await using var connection = await db.OpenAsApplicationRole();
        await using var tx = await connection.BeginTransactionAsync();
        await db.SetCurrentTenant(connection, tx, Seed.Apex.TenantId);

        var tenants = await db.DistinctTenantIds(connection, tx, $"SELECT tenant_id FROM {table}");

        Assert.Equal(new[] { Seed.Apex.TenantId }, tenants);
    }

    [Theory(DisplayName = "AC5 with no current tenant set, no rows are returned")]
    [MemberData(nameof(TenantOwnedTables))]
    public async Task No_current_tenant_returns_no_rows(string table)
    {
        await using var connection = await db.OpenAsApplicationRole();
        await using var tx = await connection.BeginTransactionAsync();

        var count = await db.Count(connection, tx, $"SELECT count(*) FROM {table}");

        Assert.Equal(0, count);
    }

    [Theory(DisplayName = "AC5 a write into another tenant is refused by the database")]
    [MemberData(nameof(TenantOwnedTables))]
    public async Task Write_into_other_tenant_is_refused(string table)
    {
        await using var connection = await db.OpenAsApplicationRole();
        await using var tx = await connection.BeginTransactionAsync();
        await db.SetCurrentTenant(connection, tx, Seed.Apex.TenantId);

        var error = await Assert.ThrowsAsync<PostgresException>(
            () => db.InsertMinimalRow(connection, tx, table, tenantId: Seed.Brightline.TenantId));

        Assert.Equal("42501", error.SqlState); // insufficient_privilege: row-level security policy violation
    }

    [Fact(DisplayName = "AC4 a unique value used by another tenant can be used again, revealing nothing")]
    public async Task Unique_constraints_are_per_tenant()
    {
        await using var connection = await db.OpenAsApplicationRole();
        await using var tx = await connection.BeginTransactionAsync();
        await db.SetCurrentTenant(connection, tx, Seed.Apex.TenantId);

        // Brightline already has an open invitation for this email; a constraint that ignored tenant_id would refuse it.
        await db.InsertInvitation(connection, tx, Seed.Apex.TenantId, email: Seed.Brightline.InvitedEmail);

        Assert.Equal(1, await db.Count(connection, tx,
            "SELECT count(*) FROM invitations WHERE email = @email", ("email", Seed.Brightline.InvitedEmail)));
    }

    [Fact(DisplayName = "AC5 the tenant setting does not survive the transaction on a pooled connection")]
    public async Task Tenant_setting_does_not_leak_across_pooled_transactions()
    {
        await using (var first = await db.OpenAsApplicationRole())
        await using (var tx = await first.BeginTransactionAsync())
        {
            await db.SetCurrentTenant(first, tx, Seed.Apex.TenantId);
            await tx.CommitAsync();
        }

        await using var second = await db.OpenAsApplicationRole(); // same pool, likely the same physical connection
        await using var tx2 = await second.BeginTransactionAsync();
        // The setting is now defined but empty on this session: the policy must treat it as "no tenant"
        // (no rows), not fail to cast it (design note, Challenged C1).
        var count = await db.Count(second, tx2, "SELECT count(*) FROM memberships");

        Assert.Equal(0, count);
    }

    [Fact(DisplayName = "AC5 every table with a tenant_id column has row-level security forced")]
    public async Task TenantOwnedTablesAreAllCovered()
    {
        await using var connection = await db.OpenAsOwnerRole();

        var unprotected = await db.TablesWithTenantIdWithoutForcedRls(connection);
        var untested = (await db.TablesWithTenantId(connection)).Except(TenantOwnedTables.Select(r => (string)r[0]));

        Assert.Empty(unprotected);
        Assert.Empty(untested);
    }

    [Fact(DisplayName = "AC5 the application role can neither bypass nor own row-level security")]
    public async Task Application_role_cannot_bypass_rls()
    {
        await using var connection = await db.OpenAsOwnerRole();

        var role = await db.RoleAttributes(connection, "jobwright_app");

        Assert.False(role.BypassRls);
        Assert.False(role.IsSuperuser);
        Assert.False(role.OwnsAnyTenantOwnedTable);
    }
}

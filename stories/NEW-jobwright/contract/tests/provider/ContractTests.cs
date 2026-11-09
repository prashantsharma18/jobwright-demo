// Draft provider tests for contract v1 (NEW-jobwright). Owner: Prashant Sharma.
// Destination: jobwright-app, tests/Jobwright.Api.ContractTests/. Written before any API code (contract first).
// They cannot run at the workspace root. They run red in the first step of the slice that builds the API,
// once the test host and fixtures exist, so each fails on its assertion and not on a missing type.
// Fixtures: two tenants seeded with the tenant command (AC29): Apex (one member per role) and Brightline (one admin),
// in a real PostgreSQL 17 (Testcontainers) with the migrations applied; tokens minted by a test signing key that the
// test host trusts in place of the identity provider (the only replaced edge).

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Xunit;

namespace Jobwright.Api.ContractTests;

public sealed class ContractDocumentTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    [Fact(DisplayName = "contract v1: the API's own OpenAPI document equals the committed contract")]
    public async Task Generated_document_equals_committed_contract()
    {
        var generated = await api.Anonymous.GetStringAsync("/openapi/v1.json");
        var committed = ContractFile.LoadNormalised("contract/openapi.yaml");

        Assert.Equal(committed, ContractFile.Normalise(generated));
    }
}

public sealed class TenantIsolationContractTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    [Fact(DisplayName = "AC4 another tenant's member is answered exactly like a member that does not exist")]
    public async Task Other_tenants_member_is_not_found_like_a_missing_one()
    {
        var apexAdmin = api.ClientFor(Seed.Apex.Admin);

        var otherTenants = await apexAdmin.GetAsync($"/v1/members/{Seed.Brightline.Admin.MemberId}");
        var missing = await apexAdmin.GetAsync($"/v1/members/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, otherTenants.StatusCode);
        Assert.Equal(missing.StatusCode, otherTenants.StatusCode);
        Assert.Equal(missing.Content.Headers.ContentType?.MediaType, otherTenants.Content.Headers.ContentType?.MediaType);
        Assert.Equal(
            await Problem.WithoutInstanceAndTrace(missing),
            await Problem.WithoutInstanceAndTrace(otherTenants));
    }

    [Fact(DisplayName = "AC4 listing and searching members returns only the current tenant's")]
    public async Task Listing_and_searching_return_only_current_tenant()
    {
        var apexAdmin = api.ClientFor(Seed.Apex.Admin);

        var all = await apexAdmin.GetFromJsonAsync<MemberPage>("/v1/members?pageSize=100");
        var search = await apexAdmin.GetFromJsonAsync<MemberPage>(
            $"/v1/members?search={Uri.EscapeDataString(Seed.Brightline.Admin.Name)}");

        Assert.Equal(Seed.Apex.MemberIds.Order(), all!.Data.Select(m => m.MemberId).Order());
        Assert.Empty(search!.Data);
    }
}

public sealed class RoleContractTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    [Theory(DisplayName = "AC6 a dispatcher or technician is refused the members list as forbidden")]
    [InlineData("dispatcher")]
    [InlineData("technician")]
    public async Task Non_admin_is_forbidden(string role)
    {
        var client = api.ClientFor(Seed.Apex.ByRole(role));

        var response = await client.GetAsync("/v1/members");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("https://jobwright.example/problems/forbidden", (await Problem.Read(response)).Type);
    }

    [Fact(DisplayName = "E2 a person removed from the tenant is refused on the next request as membership-revoked")]
    public async Task Removed_member_gets_membership_revoked()
    {
        var person = await api.SeedExtraMember(Seed.Apex.Tenant, role: "dispatcher");
        var client = api.ClientFor(person);
        await api.RemoveMembership(person);

        var response = await client.GetAsync("/v1/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("https://jobwright.example/problems/membership-revoked", (await Problem.Read(response)).Type);
    }
}

public sealed class ProblemFormatContractTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    [Theory(DisplayName = "AC13 every error is a problem response, whatever the request accepts")]
    [InlineData("text/html")]
    [InlineData("application/xml")]
    [InlineData("*/*")]
    public async Task Errors_are_problem_json_whatever_is_accepted(string accept)
    {
        var client = api.ClientFor(Seed.Apex.Admin);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v1/members?pageSize=0");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(accept));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await Problem.Read(response);
        Assert.True(problem.Errors.ContainsKey("pageSize"), "the refusal must name the field");
    }

    [Fact(DisplayName = "AC13 a failure inside the server reveals no internals")]
    public async Task Server_failure_reveals_no_internals()
    {
        var client = api.ClientFor(Seed.Apex.Admin);
        using var _ = api.FailNextDatabaseCall();

        var response = await client.GetAsync("/v1/tenant/settings");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        foreach (var leak in new[] { " at ", "SELECT", "Npgsql", "Exception", "/src/", ".cs:" })
            Assert.DoesNotContain(leak, body, StringComparison.OrdinalIgnoreCase);
    }
}

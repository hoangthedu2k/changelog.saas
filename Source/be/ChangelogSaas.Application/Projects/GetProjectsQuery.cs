using System;
using System.Collections.Generic;
using System.Text;

namespace ChangelogSaas.Application.Projects
{
    public record GetProjectsQueryResult(Guid Id, string Name, string Slug, string? AccentColor, string? CustomDomain);
    public class GetProjectsQueryHandler()
    {

    }
}

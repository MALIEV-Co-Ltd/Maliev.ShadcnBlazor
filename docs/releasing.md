# Releasing

Releases are published from GitHub Releases through NuGet Trusted Publishing.
No long-lived NuGet API key is stored in repository secrets.

## Maintainer checklist

1. Update `VersionPrefix` and `CHANGELOG.md` in a reviewed pull request.
2. Confirm CI, CodeQL, dependency review, package validation, and browser tests
   are green on the release commit.
3. Create a Git tag named `vMAJOR.MINOR.PATCH` from that commit.
4. Publish the matching GitHub Release.
5. The release workflow validates that the tag and package version match,
   obtains a short-lived NuGet credential through GitHub OIDC, and pushes the
   `.nupkg` and `.snupkg` to NuGet.org. The Pages workflow is also triggered by
   the published release and builds the Showcase from that exact tag, keeping
   the public docs synchronized with the package.
6. Verify the package page, repository metadata, symbols, install command, and
   the deployed Pages commit.

The NuGet.org Trusted Publishing policy must name this repository, the release
workflow file, and the protected `nuget` GitHub environment. Publishing fails
closed when that trust relationship is absent or does not match.

Do not republish an existing version. Correct the issue and release a new
SemVer version.

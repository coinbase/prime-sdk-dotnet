# Coinbase Prime .NET SDK — Deploy

Canonical repository: [coinbase/prime-sdk-dotnet](https://github.com/coinbase/prime-sdk-dotnet).

This project publishes to [NuGet.org](https://www.nuget.org/packages/CoinbaseSdk.Prime) via GitHub Actions and [NuGet OIDC trusted publishing](https://devblogs.microsoft.com/nuget/introducing-nuget-trusted-publishing/).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later (CI uses .NET 10; the package targets `net8.0`)
- NuGet.org account with publish access to `CoinbaseSdk.Prime`
- GitHub `release` environment configured with `NUGET_USER` and OIDC trust for this repository

## Publish with GitHub Actions

Creating a [GitHub Release](https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository#creating-a-release) runs [`.github/workflows/build.yml`](.github/workflows/build.yml). You can also run the workflow manually from **Actions → build → Run workflow**, providing the git ref to publish (for example a release tag).

The workflow checks out the ref, packs `CoinbaseSdk.Prime.csproj`, authenticates to NuGet via OIDC, and pushes the package.

### Trigger manually from the CLI

Use the [GitHub CLI](https://cli.github.com/) (`gh auth login` if needed). From a clone of this repo:

```bash
gh workflow run build -f ref=v0.7.0
```

From another directory, pass the repository explicitly:

```bash
gh workflow run build --repo coinbase/prime-sdk-dotnet -f ref=v0.7.0
```

Watch the latest run or list recent build runs:

```bash
gh run watch
gh run list --workflow=build
```

### Release environment secrets

Configure these under **Settings → Environments → release → Environment secrets** (the publish job uses the `release` environment):

| Secret | Description |
|--------|-------------|
| `NUGET_USER` | NuGet.org username or organization account used for trusted publishing |

Also configure NuGet.org **trusted publishing** for `coinbase/prime-sdk-dotnet` (OIDC) so the workflow can obtain a short-lived API key.

### Release checklist

1. Bump `<Version>` in `src/CoinbaseSdk/Prime/CoinbaseSdk.Prime.csproj` on `main`.
2. Update `CHANGELOG.md`.
3. Create and push an annotated tag: `git tag v0.7.0 && git push origin v0.7.0`
4. Create a GitHub Release for that tag (event type **published** triggers the workflow).
5. Confirm the workflow succeeded and the package appears on [NuGet](https://www.nuget.org/packages/CoinbaseSdk.Prime).

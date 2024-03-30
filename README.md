![GitHub License](https://img.shields.io/github/license/grapio/grapio-server)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues-pr/grapio/grapio-server)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues/grapio/grapio-server)
![GitHub Repo stars](https://img.shields.io/github/stars/grapio/grapio-server)
![GitHub watchers](https://img.shields.io/github/watchers/grapio/grapio-server)
![GitHub forks](https://img.shields.io/github/forks/grapio/grapio-server)

# grapio-server

## What is the Grapio Server?
Grapio Server is the server component of the [Grapio Provider for .NET](https://github.com/grapio/grapio-openfeature-provider-dotnet) that stores and manages feature flags. 

## Workings

A crude type detector is used to determine the type of value that is stored in the database. It currently detects the following types:
1. Boolean
2. Integer
3. Double
4. String,
5. Structured (JSON, XML and YAML)

Note: The type detector assumes that all YAML documents starts with `---`.

## Configuration
A typical Grapio configuration section is shown from the `appsettings.json` file. 

```
"Grapio": {
    "ConnectionString": "DataSource=grapio.db;Mode=ReadWriteCreate;",
    "ControlServiceHost": "*:3280",
    "ProviderServiceHost": "*:3278"
}
```

## Contributing
To get started, have a look at the [CONTRIBUTING](https://github.com/grapio/grapio-openfeature-provider-dotnet/blob/main/CONTRIBUTING.md) guide.

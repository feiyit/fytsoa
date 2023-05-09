FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY . /app
ENTRYPOINT ["dotnet", "FytSoa.ApiService.dll"]
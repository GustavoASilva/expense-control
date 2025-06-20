FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update && apt-get install -y curl

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ExpenseControl.Api/ExpenseControl.Api.csproj ExpenseControl.Api/
RUN dotnet restore ExpenseControl.Api/ExpenseControl.Api.csproj
COPY ExpenseControl.Api/. ExpenseControl.Api/
WORKDIR /src/ExpenseControl.Api
RUN dotnet publish ExpenseControl.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ExpenseControl.Api.dll"]

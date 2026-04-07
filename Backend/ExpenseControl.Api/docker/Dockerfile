FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update && apt-get install -y curl

ENV ASPNETCORE_URLS=http://+:80
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY Backend/ExpenseControl.Api/ExpenseControl.Api.csproj Backend/ExpenseControl.Api/
RUN dotnet restore Backend/ExpenseControl.Api/ExpenseControl.Api.csproj
COPY Backend/ExpenseControl.Api/. Backend/ExpenseControl.Api/
WORKDIR /src/Backend/ExpenseControl.Api
RUN dotnet publish ExpenseControl.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ExpenseControl.Api.dll"]

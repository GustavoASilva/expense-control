# Dockerfile for ExpenseControl.Frontend (Blazor WebAssembly Hosted)

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5171

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ExpenseControl.Frontend.csproj .
RUN dotnet restore ExpenseControl.Frontend.csproj
COPY . .
RUN dotnet publish ExpenseControl.Frontend.csproj -c Release -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 5171
CMD ["nginx", "-g", "daemon off;"]

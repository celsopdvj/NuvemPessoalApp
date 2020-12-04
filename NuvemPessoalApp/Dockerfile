#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NuvemPessoalApp.csproj", ""]
RUN dotnet restore "./NuvemPessoalApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "NuvemPessoalApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NuvemPessoalApp.csproj" -c Release -o /app/publish

FROM base AS final
ENV ASPNETCORE_ENVIRONMENT Development
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NuvemPessoalApp.dll"]
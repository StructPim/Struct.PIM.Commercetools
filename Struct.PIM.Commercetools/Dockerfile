FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Struct.PIM.Commercetools/Struct.PIM.Commercetools.csproj", "Struct.PIM.Commercetools/"]
RUN dotnet restore "Struct.PIM.Commercetools/Struct.PIM.Commercetools.csproj"
COPY . .
WORKDIR "/src/Struct.PIM.Commercetools"
RUN dotnet build "Struct.PIM.Commercetools.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Struct.PIM.Commercetools.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Struct.PIM.Commercetools.dll"]

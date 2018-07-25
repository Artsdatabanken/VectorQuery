FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 53118
EXPOSE 44391

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY VectorQuery/VectorQuery.csproj VectorQuery/
RUN dotnet restore VectorQuery/VectorQuery.csproj
COPY . .
WORKDIR /src/VectorQuery
RUN dotnet build VectorQuery.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish VectorQuery.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "VectorQuery.dll"]

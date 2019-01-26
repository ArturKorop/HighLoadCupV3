FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY HighLoadCupV3/HighLoadCupV3.csproj HighLoadCupV3/
COPY NuGet.Config HighLoadCupV3/
RUN dotnet restore HighLoadCupV3/HighLoadCupV3.csproj
COPY . .
WORKDIR /src/HighLoadCupV3
RUN dotnet build HighLoadCupV3.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish HighLoadCupV3.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

CMD dotnet HighLoadCupV3.dll
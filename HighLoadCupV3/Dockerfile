FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

# Install DB
FROM base as db
#RUN apt-get update && apt-get install -y procps
#RUN echo "deb http://repo.mongodb.org/apt/debian stretch/mongodb-org/4.0 main" | tee /etc/apt/sources.list.d/mongodb-org-4.0.list
#RUN apt-get update
#RUN apt-get --assume-yes --allow-unauthenticated install -y mongodb-org
#RUN mongod --version
#RUN mkdir /data
#RUN mkdir /data/db
#RUN mongod --fork --config /etc/mongod.conf
#RUN mkdir /tmp/data

FROM db as small
COPY data.zip /tmp/data/
COPY options.txt /tmp/data
#
#FROM db as big
#COPY data_big.zip /tmp/data/data.zip
#COPY options.txt /tmp/data
#
FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY HighLoadCupV3/HighLoadCupV3.csproj HighLoadCupV3/
COPY NuGet.Config HighLoadCupV3/
RUN dotnet restore HighLoadCupV3/HighLoadCupV3.csproj
COPY . .
WORKDIR /src/HighLoadCupV3
RUN dotnet build HighLoadCupV3.csproj -c Debug -o /app

FROM build AS publish
RUN dotnet publish HighLoadCupV3.csproj -c Debug -o /app


FROM small AS final
WORKDIR /app
COPY --from=publish /app .

CMD dotnet HighLoadCupV3.dll

 
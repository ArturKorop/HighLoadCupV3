FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
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
FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY HighLoadCupWithMongo/HighLoadCupWithMongo.csproj HighLoadCupWithMongo/
COPY NuGet.Config HighLoadCupWithMongo/
RUN dotnet restore HighLoadCupWithMongo/HighLoadCupWithMongo.csproj
COPY . .
WORKDIR /src/HighLoadCupWithMongo
RUN dotnet build HighLoadCupWithMongo.csproj -c Debug -o /app

FROM build AS publish
RUN dotnet publish HighLoadCupWithMongo.csproj -c Debug -o /app


FROM small AS final
WORKDIR /app
COPY --from=publish /app .

CMD dotnet HighLoadCupWithMongo.dll

 
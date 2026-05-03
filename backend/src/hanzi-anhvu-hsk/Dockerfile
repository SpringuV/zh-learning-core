FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
#COPY ./backend/src/hanzi-anhvu-hsk/HanziAnhVuHsk.Api.csproj ./hanzi-anhvu-hsk/
WORKDIR /src/hanzi-anhvu-hsk 
RUN dotnet restore ./HanziAnhVuHsk.Api.csproj
RUN dotnet publish ./HanziAnhVuHsk.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "HanziAnhVuHsk.Api.dll"]

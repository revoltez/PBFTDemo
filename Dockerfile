FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
COPY bin/release/netcoreapp3.1/publish/ App/
WORKDIR /App
RUN mkdir /App/Keys
EXPOSE 5000
ENTRYPOINT ["dotnet","Pbft demo.dll"]
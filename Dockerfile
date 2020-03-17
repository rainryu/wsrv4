#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
#ENV ASPNETCORE_URLS http://+:8080
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["WebApplication4/WebApplication4.csproj", "WebApplication4/"]
RUN dotnet restore "WebApplication4/WebApplication4.csproj"
COPY . .
WORKDIR "/src/WebApplication4"
RUN dotnet build "WebApplication4.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApplication4.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication4.dll"]

#docker run -it --rm -p 8080:8080 -e ASPNETCORE_URLS="http://+:8080" --name wsrv4 wsrv4
#docker build -t wsrv4 -f ./Dockerfile .
#docker container prune
#docker run -it --name ub ubuntu
#docker cp ctos:/ids D:\rs256
#docker run -it --name ctos centos

#Linux系统生成证书：（推荐使用）--platform=linux
sudo yum install openssl (CentOS)
#生成私钥文件
openssl genrsa -out idsrv4.key 2048

#创建证书签名请求文件 CSR（Certificate Signing Request），用于提交给证书颁发机构（即 Certification Authority (CA)）即对证书签名，申请一个数字证书。
openssl req -new -key idsrv4.key -out idsrv4.csr 
#生成自签名证书（证书颁发机构（CA）签名后的证书，因为自己做测试那么证书的申请机构和颁发机构都是自己,crt 证书包含持有人的信息，持有人的公钥，以及签署者的签名等信息。当用户安装了证书之后，便意味着信任了这份证书，同时拥有了其中的公钥。）
openssl x509 -req -days 365 -in idsrv4.csr -signkey idsrv4.key -out idsrv4.crt （包含公钥）
#自签名证书与私匙合并成一个文件（注：.pfx中可以加密码保护，所以相对安全些）
openssl pkcs12 -export -in idsrv4.crt -inkey idsrv4.key -out idsrv4.pfx (注：在生成的过程中会让我们输入Export Password)

openssl req -newkey rsa:2048 -nodes -keyout idsrv4.key -x509 -days 365 -out idsrv4.cer
openssl pkcs12 -export -in idsrv4.cer -inkey idsrv4.key -out idsrv4.pfx


docker run --platform=linux --name redis-server -d -p 6379:6379 --restart=always redis

docker run --platform=linux -p 3306:3306 --name mysql -e MYSQL_ROOT_PASSWORD=123456 -d mysql

docker run -p 3306:3306 --name mysql -e MYSQL_ROOT_PASSWORD=123456 -d mysql
docker exec -it mysql bash

select user,host,authentication_string from mysql.user;
grant all PRIVILEGES on *.* to root@'%' WITH GRANT OPTION;
ALTER user 'root'@'%' IDENTIFIED BY '123456' PASSWORD EXPIRE NEVER;
ALTER user 'root'@'%' IDENTIFIED WITH mysql_native_password BY '123456';
FLUSH PRIVILEGES;


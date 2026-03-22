# 1. ビルド用の環境（SDK）
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# プロジェクトファイルをコピーして復元
COPY ["MuscleTrainingApi.csproj", "./"]
RUN dotnet restore "MuscleTrainingApi.csproj"

# 全てのファイルをコピーしてビルド
COPY . .
RUN dotnet build "MuscleTrainingApi.csproj" -c Release -o /app/build

# 2. 実行用の環境（Runtime）
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/build .

# Renderなどのポート番号に対応させるための設定
ENV ASPNETCORE_URLS=http://+:10000

ENTRYPOINT ["dotnet", "MuscleTrainingApi.dll"]
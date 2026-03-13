# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Solution və csproj-ları əvvəlcə kopyala (restore cache üçün)
COPY *.sln ./
COPY Core/SavaloApp.Application/SavaloApp.Application.csproj Core/SavaloApp.Application/
COPY Core/SavaloApp.Domain/SavaloApp.Domain.csproj Core/SavaloApp.Domain/
COPY Infrastructure/SavaloApp.Infrastructure/SavaloApp.Infrastructure.csproj Infrastructure/SavaloApp.Infrastructure/
COPY Infrastructure/SavaloApp.Persistance/SavaloApp.Persistance.csproj Infrastructure/SavaloApp.Persistance/
COPY Presentation/SavaloApp.WebApi/SavaloApp.WebApi.csproj Presentation/SavaloApp.WebApi/

# NuGet restore
RUN dotnet restore

# Bütün source-ları kopyala
COPY . .

# Publish
WORKDIR /src/Presentation/SavaloApp.WebApi
RUN dotnet publish -c Release -o /app/out /p:UseAppHost=false

# ===== Runtime stage =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Əgər ffmpeg lazımdırsa saxla, lazım deyilsə bu hissəni sil
RUN apt-get update \
 && apt-get install -y --no-install-recommends ffmpeg \
 && rm -rf /var/lib/apt/lists/*

# Publish output
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SavaloApp.WebApi.dll"]
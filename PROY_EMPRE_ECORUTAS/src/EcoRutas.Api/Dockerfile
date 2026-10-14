# ETAPA 1: Compilación y publicación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos el archivo de proyecto y restauramos dependencias
COPY ["EcoRutas.Api.csproj", "./"]
RUN dotnet restore "EcoRutas.Api.csproj"

# Copiamos el resto del código y compilamos en modo Release
COPY . .
RUN dotnet publish "EcoRutas.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ETAPA 2: Entorno de ejecución (Runtime liviano)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Exponemos el puerto estándar 8080 para la nube
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Copiamos los binarios compilados desde la primera etapa
COPY --from=build /app/publish .

# Comando de arranque del contenedor
ENTRYPOINT ["dotnet", "EcoRutas.Api.dll"]
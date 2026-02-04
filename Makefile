# Makefile ParkManager
all:
	@echo "Use um comando válido"

docker:
	docker compose -p parkmanager down
# 	docker volume rm parkmanager_postgres_data
	docker compose -p parkmanager up --build

docker-down:
	docker compose -p parkmanager down

docker-up:
	docker compose -p parkmanager up

docker-build:
	docker-compose -p parkmanager up --build

docker-lint:
	docker exec -it parkmanager_service dotnet format ParkManager-Service.csproj --verify-no-changes --verbosity d --no-restore

docker-test:
	docker exec -e ASPNETCORE_ENVIRONMENT=Test -it parkmanager_service dotnet test -l "console;verbosity=normal" --no-restore

docker-backend-restore:
	docker exec -it parkmanager_service dotnet restore

# docker-coverage:
# 	docker exec -it parkmanager_service dotnet test --collect:"XPlat Code Coverage"

# Criar migration
# dotnet ef migrations add NomeDaSuaMigration

# Adicionar migration no Banco de Dados
# dotnet ef database update NomeDaSuaMigration

# Resetar o Banco de Dados
# dotnet ef database update 0

# Remover a ultima migration (apaga os arquivos)
# dotnet ef migrations remove

__default:
	make deps;
	make build;
	make run;

deps:
	cd ./src/Ferro && dotnet restore;

build:
	cd ./src/Ferro && dotnet build;

run:
	cd ./src/Ferro && dotnet run;

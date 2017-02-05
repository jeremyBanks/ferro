__default:
	make deps;
	make build;
	make test;
	make run;

.PHONY: __default deps build test run

deps:
	cd ./src/Ferro/ && dotnet restore;
	cd ./test/Ferro.Tests/ && dotnet restore;

build:
	cd ./src/Ferro/ && dotnet build;

test:
	cd ./test/Ferro.Tests/ && dotnet test;

run:
	cd ./src/Ferro/ && dotnet run;

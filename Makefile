__default:
	test-peer/stop || true;
	test-peer/start;
	./test;
	./ferro --help;
	test-peer/stop;

.PHONY: __default deps build test run peer stop-peer

deps:
	cd ./src/Ferro/ && dotnet restore;
	cd ./test/Ferro.Tests/ && dotnet restore;

build:
	cd ./src/Ferro/ && dotnet build;

test:
	cd ./test/Ferro.Tests/ && dotnet test;

run:
	cd ./src/Ferro/ && dotnet run -- connect 127.0.0.1:9527;
	cd ./src/Ferro/ && dotnet run -- get-meta;

run-verbose:
	cd ./src/Ferro/ && dotnet run -- 127.0.0.1 -v;

run-help:
	cd ./src/Ferro/ && dotnet run -- --help;

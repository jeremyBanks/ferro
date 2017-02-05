__default:
	make deps;
	make build;
	make test;
	make run;

.PHONY: __default deps build test run env

deps:
	cd ./src/Ferro/ && dotnet restore;
	cd ./test/Ferro.Tests/ && dotnet restore;

build:
	cd ./src/Ferro/ && dotnet build;

test:
	cd ./test/Ferro.Tests/ && dotnet test;

run:
	cd ./src/Ferro/ && dotnet run;

env:
	docker pull camillebaronnet/docktorrent;
	docker run -d \
		-p 8042:80 -p 45566:45566 -p 9527:9527/udp \
		--dns 8.8.8.8 \
		-v $(PWD)/test-env-data/rtorrent \
		-e UPLOAD_RATE=1024 \
		camillebaronnet/docktorrent;
	docker ps;
	# Peer now running at: localhost:45566
	# You may control it at: http://localhost:8042
	# You can close it with `docker stop NAME`, where
	# NAME is auto-generated and should be list above.

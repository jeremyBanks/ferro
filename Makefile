__default:
	make deps;
	make build;
	make test;
	make run;

.PHONY: __default deps build test run peer stop-peer

deps:
	cd ./src/Ferro/ && dotnet restore;
	cd ./test/Ferro.Tests/ && dotnet restore;

build:
	cd ./src/Ferro/ && dotnet build;

test:
	cd ./test/Ferro.Tests/ && dotnet test;

run:
	cd ./src/Ferro/ && dotnet run 127.0.0.1;

peer:
	rm -rf ./test-peer-state/;
	cp -R ./test-peer-data/ ./test-peer-state/;
	docker pull registry.gitlab.com/banks/ferro:docktorrent || docker login registry.gitlab.com;
	docker run -d \
		-p 8042:80 -p 45566:45566 -p 9527:9527/udp \
		--dns 8.8.8.8 \
		-v $(PWD)/test-peer-state:/rtorrent  \
		-e UPLOAD_RATE=1024 \
		registry.gitlab.com/banks/ferro:docktorrent;
	docker ps;
	# Peer now running at: localhost:45566
	# You may control it at: http://localhost:8042
	# You can terminate it gracefully with `make stop-peer`

stop-peer:
	docker stop -t 120 "$$(docker ps -q --filter ancestor=registry.gitlab.com/banks/ferro:docktorrent)";

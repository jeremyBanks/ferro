# Ferro

Ferro is (going to be) a BitTorrent client written in C# with .NET Core by [Jeremy Banks](https://jeremy.ca) and [Chris Ronning](https://chrisronning.com) (see [LICENSE](./LICENSE)).

repo: [gitlab.com/banks/ferro](https://gitlab.com/banks/ferro) [<img src="https://gitlab.com/banks/ferro/badges/master/build.svg" height="12">](https://gitlab.com/banks/ferro/pipelines)  
mirror: [github.com/jeremyBanks/ferro](https://github.com/jeremyBanks/ferro)  
mirror: [bitbucket.org/jeremyBanks/ferro](https://bitbucket.org/jeremyBanks/ferro)  
mirror: [jeremy.gitly.io/ferro](https://jeremy.gitly.io/ferro)  
docs: [banks.gitlab.io/ferro/](https://banks.gitlab.io/ferro/)  

## Components

- [`Ferro.CLInterface`](src/Ferro/Ferro.CLInterface.cs)  
  The basic command-line user interface we're initially experimenting with.
- [`Ferro.BitTorrent.Client`](src/Ferro/BitTorrent/Ferro.BitTorrent.Client.cs)  
  The high-level programming interface for using BitTorrent, encapsulating all of the details.
- [`Ferro.Common`](src/Ferro/Common)  
  Utilities and simple data types that shared used by everthing else.
- [`Ferro.PeerProtocol`](src/Ferro/PeerProtocol)  
  Implemenetation of BitTorrent's TCP peer protocol.
- [`Ferro.DHT`](src/Ferro/DHT)  
  Client/read-only implementation BitTorrent's BEP-5 UDP Distributed Hash Table protocol.
- [`Ferro.Tests`](test/Ferro.Tests)  
  Our tests.

## Development Tips

### Using `dotnet` in the shell (maybe with Visual Studio Code)

`test-peer/start` and `test-peer/stop` will start and stop our test rTorrent/ruTorrent Docker image using data from `./test-peer/state`. Our example/test programs may require this peer or use it for bootstrapping. You can control it directly through the web interface at <http://localhost:8042>. If you want to commit updated data/state, you need to `test-peer/stop && rm -rf ./test-peer/state && mv ./test-peer/active-state ./test-peer/state`.

`./ferro` will install deps, build, and run our main command-line application. Run it to see a description of available subcommands.

`test/run` runs whatever tests we have.

### Using Visual Studio

Since this assumes Windows, you won't be able to use the above commands by default. I suggest using [Docker Toolbox](https://docs.docker.com/toolbox/overview/) to get a Docker instance running. In the Docker Command Prompt, copy and paste the commands from `test-peer/start`. This should get you set up with our test rTorrent/ruTorrent Docker image, using data from `.\test-peer-data\`. Once this is set up, I recommend [Kitematic](https://docs.docker.com/kitematic/userguide/), a nice GUI that will let you start and stop the Docker image, manage ports, etc.

Beyond that, you'll want to set up your Docker instance's IP Address as a command line argument in Visual Studio. Once you do that, you can run it like normal or with the debugger without ill effect.

## BitTorrent References

- [Official Index of BitTorrent Enhancement Proposals (Specifications)](http://www.bittorrent.org/beps/bep_0000.html)
- [BitTorrent Protocol and Bencoding (BEP 3)](http://www.bittorrent.org/beps/bep_0003.html)  
  Clients read the bencoded torrent file, use its HTTP tracker URL to get/update list of other clients (peers), and exchange data with them over TCP.
- [Unofficial BitTorrent Specification](https://wiki.theory.org/BitTorrentSpecification)  
  Alternative description that may help understanding of certain parts.
- [My brief summary of how decentralized torrents work](https://stackoverflow.com/a/22240583/1114)
- [Multiple Tracker Metadata Extension (BEP 12)](http://www.bittorrent.org/beps/bep_0012.html)  
  Small extension to allow multiple trackers in torrent file.
- [BitTorrent Extension Protocol (BEP 10)](http://www.bittorrent.org/beps/bep_0010.html)  
  Standard for non-conflicting extensions to peer protocol.
- [Web Seeding (BEP 19)](http://www.bittorrent.org/beps/bep_0019.html)  
  Standard for downloading torrent data from normal HTTP servers.
- [UDP Tracker Protocol (BEP 15)](http://www.bittorrent.org/beps/bep_0015.html)  
  More-efficient UDP version of tracker protocol.
- [Compact Peer Lists (BEP 23)](http://www.bittorrent.org/beps/bep_0023.html)  
  Minor optimization to HTTP tracker protocol.
- [Peer Exchange (BEP 11)](http://www.bittorrent.org/beps/bep_0011.html)  
  Extension allowing peers to exchange others peers addresses, so the tracker isn't required once connected to other torrent peers.
- [Canonical Peer Priority (BEP 40)](http://www.bittorrent.org/beps/bep_0040.html)  
  Defines a peer selection algorithm to slightly improve swarm security and efficiency.
- [Metadata Exchange and Magnet Links (BEP 9)](http://www.bittorrent.org/beps/bep_0009.html)  
  Extension allowing peers to exchange torrent file info directly, so the torrent file isn't required in advance.
- [Mainline DHT Protocol (BEP 5)](http://www.bittorrent.org/beps/bep_0005.html)  
  Distributed index of peers on a secondary P2P network, used to find initial torrent peers without any tracker.
- [Read-only DHT Nodes](http://www.bittorrent.org/beps/bep_0043.html)  
  Allows DHT nodes to indicate that they're read/client-only, and should not be queried.
- [Kademlia Protocol (Maymounkov and Mazières 2002)](https://pdos.csail.mit.edu/~petar/papers/maymounkov-kademlia-lncs.pdf)  
  Fuller description of the protocol on which the mainline DHT protocol is based.
- [Magnet URI Web Seeding Draft Proposal](https://wiki.theory.org/BitTorrent_Magnet-URI_Webseeding)  
  De-facto description of additional magnet link parameters including web seeds.

Ferro is a BitTorrent client, library, and/or something, written in C# on .NET Core.

We're not very experienced with C#, so look elsewhere if you want good code.

## Developer Resources

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

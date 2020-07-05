# PBFT in .NET CORE
Practical byzantine fault tolerance algorithm that can be used to build consensus in asynchronous networks implemented in .NET CORE
this is demonstaration of a simple p2p network simulated by Docker where each container has its own ip between 4 nodes to run
the pbft algorithm therefore reaching consensus 

This algorithm can be used as a consensus algorithm for a blockchain

# Crypto

Elliptic curve cryptography is used to create The public/private key pair 

# How to test 

run ./build.sh and configuire the list of ip addresses in configs.cs file ,each ip should represent the container ip ,the default docker ip range is 172.17. 0.0/16 so this will typically run without modifications 


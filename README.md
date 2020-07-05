# PBFT-Blockchain
A permissionned blockchain that uses PBFT as a consensus Algorithm, 
this is demonstaration of a simple p2p simulated by Docker where each container has its own ip between 4 nodes to run
the pbft algorithm and construct a blockchain 

# How to test 

run ./build.sh and configuire the list of ip addresses in configs.cs file ,each ip should represent the container ip ,the default docker ip range is 172.17. 0.0/16 so 
just typically this will run without modifications 


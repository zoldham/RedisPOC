# Redis POC
This repo is the Kore Wireless Redis POC. The stated goal was to determine if Redis was a viable option for caching the results of GraphQL queries against the global device store.

## Starting Redis
Navigate to redis-5.0.5 and run './src/redis-server redis.conf' Redis will start automatically and will load into the same state as when it was last closed. It will be accessible via localhost.

## Running the Project
This repo contains a Visual Studio Project that when run while connected to the Kore VPN will time GraphQL queries when they are in the Redis cache and when they are not.

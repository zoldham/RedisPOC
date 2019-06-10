# Redis POC
This repo is the Kore Wireless Redis POC. The stated goal was to determine if Redis was a viable option for caching the results of GraphQL queries against the global device store.

## Starting Redis
Navigate to redis-5.0.5 and run './src/redis-server redis.conf' Redis will start automatically and will load into the same state as when it was last closed. It will be accessible via localhost.

## Running the Project
This repo contains a Visual Studio Project that when run while connected to the Kore VPN will time GraphQL queries when they are in the Redis cache and when they are not.

## Conclusion
Using Redis as a cache for query results is easy to implement and, at least when the redis cache is local, offers a signficant performance improvement over standard querying. In testing, the time to retrive data from the redis cache was between 1/20th and 1/200th the time to query the database, depending on the complexity of the query and the size of the result set.

## Limitations
Redis limits key and value sizes to 500MB, so a query expecting a large result should not be cached. Additionally, Redis requires a significant amount of memory. When it runs out of memory, it will start to drop key-value pairs starting with older ones. Additionally, due to the simplistic nature of any caching logic, care needs to be taken to avoid trying to cache queries that have an effect on the database. If those queries are used, they will behave as expected when first used, but will have no effect after that.

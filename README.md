# quotespoller

1. `docker-compose.exe up` / `docker compose up` depending on OS
2. it will run 6 services:
three of which are the project

 a) `poller` - to poll the currencies quotes and publish them to kafka (not more than 100 calls per hour and not more than 3 per second) 
 this service now works in one instance but if you want to make many instances then there has to be the distributed locking say via redis
 or using a distributed job framework so that only one instance at a time can poll and avoid 429s. 
 
 b) `saver` - to consume records from kafka and put them into the pgdatabas
 
 c) `rest` -the rest interface to show the quotes history `http://localhost:5001/api/blockchain/history`
swagger is located under http://localhost:5001/swagger/index.html

3. Extra services are:

a) `zookeeper` for kafka

b) `kafka`

c) `postgresql` server

in this configuration our services are decoupled and indepenendent
and are scalable - you can spawn as many kubernetes pods with these images
as it's required - say by autoscaler

4. Kafka has now one partition in the ``blockchain-data`` topic but can be reconfigured
to as many consumers as it is required considering different kafka consumer groups

5. The indexes (`createdAt` field, `BlockchainApi` field (type of currency)) are created in the ``BlockchainData`` table automatically 

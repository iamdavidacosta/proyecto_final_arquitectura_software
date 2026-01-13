#!/bin/bash

echo "Waiting for MongoDB nodes to be ready..."
sleep 30

echo "Initiating MongoDB Replica Set..."

# Connect without authentication for replica set initialization (development mode)
mongosh --host mongodb-primary:27017 <<EOF
rs.initiate({
  _id: "rs0",
  members: [
    { _id: 0, host: "mongodb-primary:27017", priority: 2 },
    { _id: 1, host: "mongodb-secondary1:27017", priority: 1 },
    { _id: 2, host: "mongodb-secondary2:27017", priority: 1 }
  ]
})
EOF

echo "Waiting for replica set to initialize..."
sleep 15

echo "Checking replica set status..."
mongosh --host mongodb-primary:27017 --eval "rs.status()"

echo "Creating application database and collections..."
mongosh --host mongodb-primary:27017 <<EOF
use fileshare_metadata

db.createCollection("file_metadata")

db.file_metadata.createIndex({ "fileId": 1 }, { unique: true })
db.file_metadata.createIndex({ "userId": 1 })
db.file_metadata.createIndex({ "createdAt": -1 })

print("MongoDB initialization completed!")
EOF

echo "MongoDB Replica Set initialization complete!"

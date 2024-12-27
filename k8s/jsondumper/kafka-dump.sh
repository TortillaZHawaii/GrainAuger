#!/usr/bin/env bash

# Author: https://gist.github.com/kppullin/81c0e7a5e8c334271825679e79ee82d1

# Overview:
# 1) Get list of all topic names
# 2) Iterate list, dumping each one to json

# Pre-reqs:
# 1) kafkacat
# 2) jq

mkdir ./output

broker="kafka-svc:9092"

topics=$(kcat -b ${broker} -L -J | jq -r '.topics[].topic' | sort)

for topic in $topics; do
    # Ignore "private"/"internal" topics. Adjust as needed.
    if [[ $topic == "_"* ]]; then
        continue
    fi
    echo "Dumping $topic"
    
    kcat -b ${broker} -C -J -e -q -o beginning -t "${topic}" > "./output/$topic.json"
done

echo "Done!"

echo "Starting the download server"

cd ./output

python3 -m http.server -b 0.0.0.0 8080
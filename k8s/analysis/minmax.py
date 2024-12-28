import os
import json
from datetime import datetime

def find_json_files(directory):
    json_files = []
    for root, _, files in os.walk(directory):
        for file in files:
            if file.endswith(".json"):
                json_files.append(os.path.join(root, file))
    return json_files

def process_json_file(file_path):
    min_ts = float('inf')
    max_ts = float('-inf')
    
    with open(file_path, 'r') as file:
        for line in file:
            data = json.loads(line)
            ts = data.get("ts")
            if ts is not None:
                min_ts = min(min_ts, ts)
                max_ts = max(max_ts, ts)
    
    return min_ts, max_ts

def convert_to_human_readable(timestamp):
    return datetime.utcfromtimestamp(timestamp / 1000).strftime('%Y-%m-%d %H:%M:%S')

def main(directory):
    # json_files = find_json_files(directory)
    json_files = [
        "/Users/tortilla/Desktop/results/orleans/100k_full_mem3/inputTransactions.json",
    ]
    results = {}
    
    for json_file in json_files:
        min_ts, max_ts = process_json_file(json_file)
        results[json_file] = {
            "min_ts": convert_to_human_readable(min_ts),
            "max_ts": convert_to_human_readable(max_ts)
        }
    
    for file, timestamps in results.items():
        print(f"File: {file}")
        print(f"Min Timestamp: {timestamps['min_ts']}")
        print(f"Max Timestamp: {timestamps['max_ts']}")
        print()

if __name__ == "__main__":
    directory = "~/Desktop/results/orleans/100k_full_mem3"
    main(directory)
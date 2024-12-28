#!/bin/bash

# URL of the server
SERVER_URL=${1:-"http://localhost:8000"}

# Directory to save the downloaded files
DOWNLOAD_DIR=${2:-"./downloads"}

# Create the download directory if it doesn't exist
mkdir -p "$DOWNLOAD_DIR"

# Get the list of files from the server
file_list=$(curl -s "$SERVER_URL" | grep -o 'href="[^"]*' | cut -d'"' -f2)

# Download each file
for file in $file_list; do
    echo "Downloading $file..."
    curl -O "$SERVER_URL/$file" --output-dir "$DOWNLOAD_DIR"
    echo "Saved $file to $DOWNLOAD_DIR"
done

echo "Download completed."
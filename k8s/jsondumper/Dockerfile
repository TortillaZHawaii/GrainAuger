# Use an official Python runtime as a parent image
FROM python:3.9-slim

# Set the working directory in the container
WORKDIR /app

# Copy the current directory contents into the container at /app
COPY . /app

# Install any needed packages specified in requirements.txt
RUN pip install --no-cache-dir -r requirements.txt

# Install kafkacat to dump messages from Kafka and jq to parse JSON
RUN apt-get update && apt-get install -y kafkacat jq

# Make the init script executable
RUN chmod +x /app/kafka-dump.sh

# Run kafka-dump.sh when the container launches
CMD ["/app/kafka-dump.sh"]

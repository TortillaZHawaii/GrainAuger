import os
import json
import yaml
import matplotlib.pyplot as plt
import pandas as pd
import numpy as np
from datetime import datetime
import re

def parse_creation_timestamp(yaml_file):
    # Name:                   fraud-detection-app
    # Namespace:              default
    # CreationTimestamp:      Sat, 28 Dec 2024 16:44:11 +0100
    with open(yaml_file, 'r') as file:
        yaml_data = yaml.safe_load(file)
        creation_timestamp = yaml_data['CreationTimestamp']
        creation_timestamp = datetime.strptime(creation_timestamp, '%a, %d %b %Y %H:%M:%S %z')
        return creation_timestamp

def parse_kafka_messages(json_file):
    messages = []
    with open(json_file, 'r') as file:
        for line in file:
            messages.append(json.loads(line))
    return messages

def extract_expected_messages(folder_name):
    match = re.search(r'(\d+)([mk])', folder_name)
    if match:
        number = int(match.group(1))
        unit = match.group(2)
        if unit == 'm':
            return number * 1_000_000
        elif unit == 'k':
            return number * 1_000
    return 0

def get_unified_topic_name(topic):
    if 'cpu' in topic or 'palindrome' in topic:
        return 'cpu'
    if 'io' in topic:
        return 'io'
    if 'base' in topic:
        return 'base'
    if 'inputStream' in topic:
        return 'intermediate'
    return topic

def calculate_metrics(folder_path):
    creation_timestamps = []
    kafka_messages = []

    for root, _, files in os.walk(folder_path):
        for file in files:
            if file.endswith('.yaml') and file != 'kafka.yaml':
                creation_timestamps.append(parse_creation_timestamp(os.path.join(root, file)))
            elif file.endswith('.json') and file != 'inputTransactions.json' and file != 'metrics_output.json':
                kafka_messages.extend(parse_kafka_messages(os.path.join(root, file)))

    if not creation_timestamps or not kafka_messages:
        raise ValueError("No valid data found in the specified folder.")

    deployment_time = min(creation_timestamps).astimezone(None)
    kafka_df = pd.DataFrame(kafka_messages)
    # Convert timestamps to datetime with UTC timezone
    kafka_df['ts'] = pd.to_datetime(kafka_df['ts'], unit='ms').dt.tz_localize('UTC').dt.tz_convert('Europe/Warsaw')
    kafka_df.sort_values(by='ts', inplace=True)

    first_message_time = kafka_df['ts'].iloc[0]
    last_message_time = kafka_df['ts'].iloc[-1]

    cold_start_time = (first_message_time - deployment_time).total_seconds()
    processing_time = (last_message_time - first_message_time).total_seconds()
    total_messages = len(kafka_df)
    expected_messages_per_topic = extract_expected_messages(os.path.basename(folder_path))
    topics = kafka_df['topic'].unique()


    metrics = {
        'cold_start_time': cold_start_time,
        'topics_real': topics.tolist(),
        'topics': [get_unified_topic_name(topic) for topic in topics],
        'expected_messages_per_topic': expected_messages_per_topic,
        'total': {
            'messages': total_messages,
            'processing_time': processing_time,
            'delta_messages': expected_messages_per_topic * len(topics) - total_messages,
            'average_processing_time': processing_time / total_messages,
            'average_throughput': total_messages / processing_time
        },
    }

    print(f"Topics found {topics}")
    for topic in kafka_df['topic'].unique():
        topic_messages = kafka_df[kafka_df['topic'] == topic]
        messages = len(topic_messages)
        processing_time = (topic_messages['ts'].iloc[-1] - topic_messages['ts'].iloc[0]).total_seconds()
        topic = get_unified_topic_name(topic)
        print(f"Topic: {topic}, Messages: {messages}, Processing Time: {processing_time} s")
        metrics[topic] = {
            'messages': messages,
            'processing_time': processing_time,
            'delta_messages': expected_messages_per_topic - messages,
            'average_processing_time': processing_time / messages,
            'average_throughput': messages / processing_time,
        }

    print("Metrics:")
    print(json.dumps(metrics, indent=4))
    with open(os.path.join(folder_path, 'metrics_output.json'), 'w') as outfile:
        json.dump(metrics, outfile, indent=4)

    # calculate throughput over time
    # split by topic, each topic should have its own color on the plot
    # use unified topic names, ignore intermediate topic
    # on the x-axis we have time, on the y-axis we have messages per second
    # split the x axis into seconds, calculate the throughput for each second
    # plot the throughput for each part
    throughput = {}
    total_throughput = []

    colors = {
        'cpu': 'red',
        'io': 'green',
        'base': 'blue',
        'intermediate': 'gray',
        'total': 'black'
    }

    # sort the topics by the name
    topics = sorted(kafka_df['topic'].unique())

    for topic in topics:
        topic_messages = kafka_df[kafka_df['topic'] == topic]
        topic = get_unified_topic_name(topic)
        processing_time = (topic_messages['ts'].iloc[-1] - topic_messages['ts'].iloc[0]).total_seconds()
        throughput[topic] = []
        for i in range(int(processing_time)):
            messages = len(topic_messages[(topic_messages['ts'] >= topic_messages['ts'].iloc[0] + pd.Timedelta(seconds=i)) & (topic_messages['ts'] < topic_messages['ts'].iloc[0] + pd.Timedelta(seconds=i + 1))])
            throughput[topic].append(messages)
            if len(total_throughput) <= i:
                total_throughput.append(messages)
            else:
                total_throughput[i] += messages

    fig, ax = plt.subplots()
    for topic, values in throughput.items():
        ax.plot(values, label=topic, color=colors.get(topic, 'gray'))
    
    ax.plot(total_throughput, label='total', linestyle='--', color=colors['total'])

    ax.set_xlabel('Time (s)')
    ax.set_ylabel('Messages per second')
    ax.legend()
    plt.savefig(os.path.join(folder_path, 'throughput.pdf'), bbox_inches='tight')
    print(f"Throughput plot saved to {os.path.join(folder_path, 'throughput.pdf')}")


if __name__ == "__main__":
    import sys
    if len(sys.argv) != 2:
        print("Usage: python metrics.py <folder_path>")
        sys.exit(1)

    folder_path = sys.argv[1]
    calculate_metrics(folder_path)
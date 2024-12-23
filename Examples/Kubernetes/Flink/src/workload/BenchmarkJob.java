package workload;

import java.io.Console;

import org.apache.flink.api.common.serialization.SimpleStringSchema;
import org.apache.flink.connector.kafka.source.KafkaSource;
import org.apache.flink.streaming.api.datastream.DataStream;
import org.apache.flink.streaming.api.environment.StreamExecutionEnvironment;

public class BenchmarkJob {
    public static void main(String[] args) {
        StreamExecutionEnvironment env = StreamExecutionEnvironment.getExecutionEnvironment();

        String bootstrapServer = "kafka-svc:9092";
        
        KafkaSource<String> kafkaSource = KafkaSource.<String>builder()
            .setBootstrapServers(bootstrapServer)
            .setTopics("inputTransactions")
            .setGroupId("flink-benchmark")
            .setStartingOffsets(KafkaSource.InitialOffset.earliest())
            .setValueOnlyDeserializer(new SimpleStringSchema())
            .build();

        DataStream<String> kafkaStream = env.fromSource(kafkaSource, WatermarkStrategy.noWatermarks(), "Kafka Source");

        KeyedStream<CardTransaction> inputStream = kafkaStream
            .map(CardTransaction::fromJSON)
            .keyBy(CardTransaction::getCardNumber);

        // Base case
        KafkaSink<String> baseSink = KafkaSink.<String>builder()
            .setBootstrapServers(bootstrapServer)
            .setTopic("baseTransactions")
            .setValueSerializer(new SimpleStringSchema())
            .build();
        KeyStream<CardTransaction> baseStream = inputStream
            .process(new BaseProcessFunction())
            .map(CardTransaction::toJSON)
            .sinkTo(baseSink);
    }
}
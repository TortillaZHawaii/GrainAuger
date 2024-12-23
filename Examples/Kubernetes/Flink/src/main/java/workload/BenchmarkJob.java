package workload;

import org.apache.flink.api.common.eventtime.WatermarkStrategy;
import org.apache.flink.api.common.serialization.SimpleStringSchema;
import org.apache.flink.connector.kafka.sink.KafkaRecordSerializationSchema;
import org.apache.flink.connector.kafka.sink.KafkaSink;
import org.apache.flink.connector.kafka.source.KafkaSource;
import org.apache.flink.streaming.api.datastream.DataStream;
import org.apache.flink.streaming.api.datastream.KeyedStream;
import org.apache.flink.streaming.api.environment.StreamExecutionEnvironment;

import workload.functions.CardCompany;
import workload.functions.CardTransaction;
import workload.functions.PassthroughFunction;
import workload.functions.RemoteFunction;

public class BenchmarkJob {
    public static void main(String[] args) throws Exception {
        StreamExecutionEnvironment env = StreamExecutionEnvironment.getExecutionEnvironment();

        String bootstrapServer = "kafka-svc:9092";
        
        KafkaSource<String> kafkaSource = KafkaSource.<String>builder()
            .setBootstrapServers(bootstrapServer)
            .setTopics("inputTransactions")
            .setGroupId("flink-benchmark")
            .setValueOnlyDeserializer(new SimpleStringSchema())
            .build();

        DataStream<String> kafkaStream = env.fromSource(kafkaSource, WatermarkStrategy.noWatermarks(), "Kafka Source");

        DataStream<CardTransaction> inputStream = kafkaStream
            .map(CardTransaction::fromJSON);
        KeyedStream<CardTransaction, String> keyedStream = inputStream
            .keyBy(CardTransaction::getCardNumber);
        
        // Base case
        KafkaSink<String> baseSink = KafkaSink.<String>builder()
            .setBootstrapServers(bootstrapServer)
            .setRecordSerializer(KafkaRecordSerializationSchema.builder()
                .setTopic("baseOutput")
                .setValueSerializationSchema(new SimpleStringSchema())
                .build())
            .build();
        keyedStream.process(new PassthroughFunction())
            .map(CardTransaction::toJSON)
            .sinkTo(baseSink);

        // Remote function
        KafkaSink<String> remoteSink = KafkaSink.<String>builder()
            .setBootstrapServers(bootstrapServer)
            .setRecordSerializer(KafkaRecordSerializationSchema.builder()
                .setTopic("remoteOutput")
                .setValueSerializationSchema(new SimpleStringSchema())
                .build())
            .build();
        keyedStream.process(new RemoteFunction())
            .map(CardCompany::toJSON)
            .sinkTo(remoteSink);

        env.execute("Flink Benchmark Job");
    }
}